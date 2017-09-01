using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

//S.A.L. | Station Automation and Logistics
/* To-do: 
 *                                                    Done?
 * Reserve workbench----------------------------------DONE
 * Resolve pawn error on map load---------------------DONE
 * Check if workbench has power-----------------------DONE
 * Take bill off bill stack once done-----------------DONE
 * change colour of ouput direction in Draw()---------DONE
 * Make them be bad at art----------------------------DONE
 * Allow/disallow taking forbidden items--------------DONE
 * Make sound while crafting--------------------------DONE
 * Make sound when sucking item in--------------------DONE
 * Check if work table is deconstructed---------------DONE
 * Clear reservation if power off---------------------DONE
 * Customisable pawns via defs; skills----------------DONE
 * Sort out problem with nutrition/cooking------------DONE
 * Eject items in thingRecord if deconstructed--------DONE
 * Edit defs: Not forbiddable, flickable--------------DONE
 * Make smart hopper----------------------------------DONE
 * Check for small volume-----------------------------DONE
 * Redo corpse calculations---------------------------DONE
 * Add ingredients to unfinished items----------------DONE
 * Patch for Mending
 * Maintenance intervals------------------------------DONE
 * Move AssemblerDef to a ModExtension----------------DONE
 * Tiered crafters------------------------------------DONE
 *From xlilcasper (Ludeon Forums)
 * Let items get accepted from adjacent cells---------DONE
 * Check if colony has enough resources for bill
 *From Kadan Joelavich (Steam)
 * "This may not be possible, but would there 
 * be any way to have their global work speed 
 * factor in the material they are make from?---------DONE
 * Rework smart hopper
 * */
namespace ProjectSAL
{
    public partial class Building_Assembler : Building_WorkTable
    {
        #region Fields
        public int rotOutput;
        public RecipeDef currentRecipe;
        public float workLeft;
        public List<_IngredientCount> ingredients = new List<_IngredientCount>();
        public List<Thing> thingRecord = new List<Thing>();
        public List<Thing> thingPlacementQueue = new List<Thing>();
        public bool allowForbidden = true;
        public Pawn buildingPawn;
        [Unsaved]
        public Sustainer sustainer;
        /// <summary>
        /// Cache only. <see cref="CheckIfShouldActivate"/>
        /// </summary>
        [Unsaved]
        bool cachedShouldActivate = true;
        static readonly FieldInfo cachedTotallyDisabled = typeof(SkillRecord).GetField("cachedTotallyDisabled", BindingFlags.NonPublic | BindingFlags.Instance);
        #endregion

        #region Nutrition/Small volume calculations
        protected static bool ShouldUseNutritionMath(Thing t, _IngredientCount ingredient)
        {
            return (t.def.ingestible?.nutrition ?? 0f) > 0f && !(t is Corpse) && IngredientFilterHasNutrition(ingredient.filter);
        }

        protected static bool IngredientFilterHasNutrition(ThingFilter filter)
        {
            if (filter != null)
            {
                Func<string, bool> isNutrition = str => str == "Foods" || str == "PlantMatter";
                var field = typeof(ThingFilter).GetField("categories", BindingFlags.NonPublic | BindingFlags.Instance);
                var categories = (List<string>)(field.GetValue(filter) ?? new List<string>());
                foreach (string s in categories)
                {
                    if (DefDatabase<ThingCategoryDef>.GetNamed(s).Parents.Select(t => t.defName).Any(isNutrition) || isNutrition(s)) return true;
                }
            }
            return false;
        }
        protected static int CalculateIngredientIntFinalised(Thing item, _IngredientCount ingredient)
        {
            float basecount = ingredient.count;
            if (ShouldUseNutritionMath(item, ingredient))
            {
                basecount /= item.def.ingestible.nutrition;
            }
            if (item.def.smallVolume)
            {
                basecount /= 0.05f;
            }
            return Mathf.RoundToInt(basecount);
        }

        protected static float CalculateBaseCountFinalised(Thing item, _IngredientCount ingredient)
        {
            float basecount = item.stackCount;
            if (ShouldUseNutritionMath(item, ingredient))
            {
                basecount *= item.def.ingestible.nutrition;
            }
            if (item.def.smallVolume)
            {
                basecount *= 0.05f;
            }
            return basecount;
        }
        #endregion


        public virtual void SetRecipe(Bill b)
        {
            currentRecipe = b.recipe;
            ingredients = new List<_IngredientCount>();
            for (int i = 0; i < (currentRecipe.ingredients?.Count ?? 0); i++)
            {
                ingredients.Add(currentRecipe.ingredients[i]);
            }
        }

        #region Products
        public virtual void TryMakeProducts()
        {
            if (currentRecipe == null)
            {
                Log.Warning(ToString() + " had workLeft > 0 when the currentRecipe is NULL. Resetting. (workLeft probably isn't synchronised with recipe. Use resetRecipe() to set currentRecipe to NULL and to synchronise workLeft.)");
                ResetRecipe();
                return;
            }
            foreach (Thing obj in GenRecipe.MakeRecipeProducts(currentRecipe, buildingPawn, thingRecord, CalculateDominantIngredient(currentRecipe, thingRecord)))
            {
                thingPlacementQueue.Add(obj);
            }
            FindBillAndChangeRepeatCount(BillStack, currentRecipe);
            ResetRecipe();
        }

        public virtual void TryOutputItem()
        {
            if (!OutputSlotOccupied && thingPlacementQueue.Count > 0)
            {
                GenPlace.TryPlaceThing(thingPlacementQueue.First(), OutputSlot, Map, ThingPlaceMode.Direct);
                thingPlacementQueue.RemoveAt(0);
            }
            else if (thingPlacementQueue.Count > 0)
            {
                foreach (var t in thingPlacementQueue)
                {
                    var thing = OutputSlot.GetThingList(Map).Find(th => th.CanStackWith(t));
                    thing?.TryAbsorbStack(t, true);
                    if (t.Destroyed || t.stackCount == 0)
                    {
                        thingPlacementQueue.Remove(t);
                        break;
                    }
                }
            }
        }
        #endregion
        
        public void PlayDropSound(Thing t)
        {
            if (t.def.soundDrop != null)
                t.def.soundDrop.PlayOneShot(SoundInfo.InMap(new TargetInfo(this)));
        }

        #region Resetting
        public virtual void ResetRecipe()
        {
            currentRecipe = null;
            ingredients.Clear();
            thingRecord.ForEach(t => t.Destroy());
            thingRecord.Clear();
            workLeft = 0;
            ReleaseAll();
        }

        public void DropAllThings()
        {
            if (currentRecipe == null) return;
            if (!currentRecipe.UsesUnfinishedThing)
            {
                foreach (var t in thingRecord)
                {
                    if (!t.Spawned) GenPlace.TryPlaceThing(t, Position, Map, ThingPlaceMode.Near);
                }
            }
            else
            {
                var stuff = (currentRecipe.unfinishedThingDef.MadeFromStuff) ? CalculateDominantIngredient(currentRecipe, thingRecord).def : null;
                var unfinished = (UnfinishedThing)ThingMaker.MakeThing(currentRecipe.unfinishedThingDef, stuff);
                unfinished.workLeft = workLeft;
                unfinished.ingredients = thingRecord;
                GenPlace.TryPlaceThing(unfinished, Position, Map, ThingPlaceMode.Near);
            }
            thingRecord.Clear();
        }

        public bool ResetIfWorkTableIsNull()
        {
            var isNull = WorkTable == null;
            if (isNull)
            {
                DropAllThings();
                ResetRecipe();
            }
            return isNull;
        }
        #endregion

        #region Reservation
        public void TryReserve(Thing thing = null)
        {
            if (thing == null)
            {
                if (WorkTable == null)
                {
                    Log.Error("Tried to reserve workTable but workTable was null.");
                    return;
                }
                if (WorkTable == this) return;
                thing = WorkTable;
            }
            Map.physicalInteractionReservationManager.Reserve(buildingPawn, new LocalTargetInfo(thing));
            //Automatically checks if already reserved in core game code
            if (Map.reservationManager.CanReserve(buildingPawn, new LocalTargetInfo(thing))) Map.reservationManager.Reserve(buildingPawn, new LocalTargetInfo(thing));
        }
        
        public void ReleaseAll()
        {
        	Map.physicalInteractionReservationManager.ReleaseAllClaimedBy(buildingPawn);
        	Map.reservationManager.ReleaseAllClaimedBy(buildingPawn);
        }
        #endregion

        public void DoSelfPawnAnalysis()
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine("Beginning S.A.L. pawn analysis.");
            b.AppendFormat("Pawn '{0}':\nSkills:\n", buildingPawn.Name);
            for (int i = 0; i < buildingPawn.skills.skills.Count; i++)
            {
                SkillRecord skill = buildingPawn.skills.skills[i];
                b.AppendFormat("Skill {0}: Level: {1}, Incapable: {2}\n",skill.def.label,skill.Level,skill.TotallyDisabled);
            }
            b.AppendFormat("Backstories: CHILD: {0} ADULT: {1}\n", buildingPawn.story.childhood, buildingPawn.story.adulthood);
            b.AppendLine("End S.A.L. pawn analysis.");
            Log.Message(b.ToString());
        }
    }
}

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ProjectRimFactory.SAL3.Things
{
    public class Building_SchematicBuilder : Building_Storage
    {
        static readonly IntVec3 Up = new IntVec3(0, 0, 1);
        //================================ Fields
        protected Thing_Schematic schematicItem;
        protected RecipeDef schematicRecipe;
        protected float workAmount;

        //================================ Misc
        public Building_WorkTable Table => (Position + Up).GetFirstBuilding(Map) as Building_WorkTable;
        public virtual IEnumerable<RecipeDef> GetAllProvidedRecipeDefs()
        {
            if (Table != null)
            {
                foreach (RecipeDef recipe in Table.def.AllRecipes)
                {
                    if (recipe.AvailableNow)
                        yield return recipe;
                }
            }
        }
        protected virtual float GetProduceSchematicWorkAmount(RecipeDef recipe)
        {
            return recipe.WorkAmountTotal(ThingDefOf.Steel) * 10;
        }
        
        //================================ Overrides
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            if (schematicRecipe == null)
            {
                foreach (RecipeDef recipe in GetAllProvidedRecipeDefs())
                {
                    yield return new Command_Action()
                    {
                        defaultLabel = "MakeNewSALShematic".Translate(recipe.label),
                        defaultDesc = "MakeNewSALSchematicDesc".Translate(recipe.label, recipe.workAmount),
                        action = () =>
                        {
                            Thing blankSchematic = slotGroup.HeldThings.First(t => t is Thing_Schematic);
                            if (blankSchematic != null)
                            {
                                schematicItem = (Thing_Schematic)blankSchematic;
                                schematicRecipe = recipe;
                                workAmount = GetProduceSchematicWorkAmount(recipe);
                            }
                            else
                            {
                                Messages.Message("SALMessage_NoSchematicFound".Translate(), MessageTypeDefOf.RejectInput);
                            }
                        }
                    };
                }
            }
            else
            {
                yield return new Command_Action()
                {
                    defaultLabel = "SALSchematicCancelBills".Translate(),
                    defaultDesc = "SALSchematicCancelBillsDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true),
                    action = ResetProgress
                };
            }
        }

        private void ResetProgress()
        {
            workAmount = 0f;
            schematicRecipe = null;
            schematicItem = null;
        }

        public override void Tick()
        {
            if (this.IsHashIntervalTick(60) && schematicRecipe != null)
            {
                workAmount -= 60f;
                if (workAmount < 0)
                {
                    // Encode recipe
                    schematicItem.recipe = schematicRecipe;
                    ResetProgress();
                }
            }
            base.Tick();
        }
    }
}

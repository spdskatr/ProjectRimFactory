using RimWorld;
using System.Linq;
using System.Reflection;
using Verse;

namespace ProjectSAL
{
    public partial class Building_Assembler
    {
        /// <summary>
        /// Accepts a new load of items into ingredient list
        /// </summary>
        public virtual void AcceptItems()
        {
            NextItems.ForEach(AcceptEachItem);
        }

        protected virtual void AcceptEachItem(Thing t)
        {
            if (t.TryGetComp<CompForbiddable>() != null
                && (!t.TryGetComp<CompForbiddable>()?.Forbidden ?? false
                || allowForbidden)
                && Map.reservationManager.IsReserved(new LocalTargetInfo(t), Faction.OfPlayer))
                return;
            for (int i = 0; i < ingredients.Count; i++)
            {
                AcceptEachIngredient(ingredients[i], t);
            }
        }

        protected virtual void AcceptEachIngredient(_IngredientCount ingredient, Thing t)
        {
            if ((decimal)ingredient.count == 0)
                return;
            AcceptItemWithFilter(t, ingredient);
        }

        protected virtual void AcceptItemWithFilter(Thing t, _IngredientCount ingredient)
        {
            var bill = (WorkTable != null && WorkTableBillStack != null) ? WorkTableBillStack.FirstShouldDoNow : null;
            if (bill?.recipe != currentRecipe)
            {
                ResetRecipe();
                DropAllThings();
                return;
            }
            //                                         Just in case bill doesn't have item in fixed ingredient filter VVV
            if (ingredient.filter.Allows(t) && ((bill?.ingredientFilter?.Allows(t) ?? true) || !currentRecipe.fixedIngredientFilter.Allows(t)))
            {
                PlayDropSound(t);
                ProcessItem(t, ingredient);
            }
        }

        protected virtual void ProcessItem(Thing t, _IngredientCount ingredient)
        {
            float baseCount = CalculateBaseCountFinalised(t, ingredient);
            if (ingredient.count >= baseCount)
            {
                TakeItemWhenBaseCountDoesNotSatisfy(t, ingredient, baseCount);
            }
            else
            {
                SplitItemWhenBaseCountSatisfies(t, ingredient);
            }
        }

        protected virtual void SplitItemWhenBaseCountSatisfies(Thing t, _IngredientCount ingredient)
        {
            var countToSplitOff = CalculateIngredientIntFinalised(t, ingredient);
            if (countToSplitOff > 0)
            {
                Thing dup = t.SplitOff(countToSplitOff);
                if (!thingRecord.Any(thing => t.def == thing.def))
                    thingRecord.Add(dup);
            }
            ingredient.count = 0;
        }

        protected virtual void TakeItemWhenBaseCountDoesNotSatisfy(Thing t, _IngredientCount ingredient, float basecount)
        {
            Thing dup;
            if (t is Corpse corpse)
            {
                corpse.Strip();
                /*
                Map.dynamicDrawManager.DeRegisterDrawable(t);
                var listoflists = typeof(ListerThings).GetField("listsByGroup", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Map.listerThings) as List<Thing>[];
                var list = listoflists[(int)ThingRequestGroup.HasGUIOverlay];
                if (list.Contains(t)) list.Remove(t);
                t.Position = Position;*/
                t.DeSpawn();
                dup = t;
                typeof(Thing).GetField("mapIndexOrState", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(dup, (sbyte)-1);
            }
            else
            {
                dup = t.SplitOff(t.stackCount);
            }
            if (!thingRecord.Any(thing => t.def == thing.def))
                thingRecord.Add(dup);
            else thingRecord.Find(thing => t.def == thing.def);
            ingredient.count -= basecount;
        }
    }
}

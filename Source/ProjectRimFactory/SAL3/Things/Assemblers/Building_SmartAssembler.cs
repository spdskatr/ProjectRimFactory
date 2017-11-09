using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ProjectRimFactory.SAL3.Things.Assemblers
{
    public class Building_SmartAssembler : Building_ProgrammableAssembler
    {
        protected override float ProductionSpeedFactor => 1f;

        public override IEnumerable<RecipeDef> GetAllRecipes()
        {
            foreach (IntVec3 cell in GenAdj.CellsAdjacent8Way(this))
            {
                if (cell.GetFirstBuilding(Map) is Building_RecipeHolder holder)
                {
                    foreach (RecipeDef recipe in holder.recipes)
                    {
                        yield return recipe;
                    }
                }
            }
        }

        public virtual void Notify_RecipeHolderRemoved()
        {
            int count = BillStack.Bills.Count;
            bool removed = false;
            HashSet<RecipeDef> set = new HashSet<RecipeDef>(GetAllRecipes());
            BillStack.Bills.RemoveAll(b => !set.Contains(b.recipe));
            removed = BillStack.Bills.Count < count;
            if (currentBillReport != null && !set.Contains(currentBillReport.bill.recipe))
            {
                for (int i = 0; i < currentBillReport.selected.Count; i++)
                {
                    GenPlace.TryPlaceThing(currentBillReport.selected[i], Position, Map, ThingPlaceMode.Near);
                }
                currentBillReport = null;
                removed = true;
            }
            if (removed)
            {
                Messages.Message("SAL3Alert_SomeBillsRemoved".Translate(), this, MessageTypeDefOf.NegativeEvent);
            }
        }
    }
}

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ProjectRimFactory.SAL3.Things.Assemblers
{
    [StaticConstructorOnStartup]
    public abstract class Building_DynamicBillGiver : Building, IBillGiver
    {
        public abstract BillStack BillStack { get; }

        public IEnumerable<IntVec3> IngredientStackCells => GenAdj.CellsOccupiedBy(this);

        public bool CurrentlyUsable() => false;

        public abstract IEnumerable<RecipeDef> GetAllRecipes();
    }
}

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ProjectRimFactory.SAL3.Things.Assemblers
{
    public class Building_SimpleAssembler : Building_ProgrammableAssembler
    {
        public override bool AllowForbidden => false;

        protected override IntVec3 OutputSlot => GenAdj.CellsAdjacentCardinal(this).First();

        protected override IEnumerable<IntVec3> InputCells => GenAdj.CellsAdjacent8Way(this);

        protected override float ProductionSpeedFactor => 1f;

        public override IEnumerable<RecipeDef> GetAllRecipes()
        {
            return from r in def.recipes
                   where r.AvailableNow
                   select r;
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            GenDraw.DrawFieldEdges(InputCells.ToList());
            GenDraw.DrawFieldEdges(new List<IntVec3> { OutputSlot }, Color.yellow);
        }
    }
}

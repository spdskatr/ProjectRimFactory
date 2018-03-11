using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProjectRimFactory.Industry
{
    public class Building_FuelingMachine : Building
    {
        public IntVec3 FuelableCell => Rotation.FacingCell + Position;
        public override void Tick()
        {
            base.Tick();
            if (this.IsHashIntervalTick(10) && GetComp<CompPowerTrader>().PowerOn)
            {
                foreach (IntVec3 cell in GenAdj.CellsAdjacent8Way(this))
                {
                    Thing item = cell.GetFirstItem(Map);
                    if (item != null)
                    {
                        CompRefuelable refuelableComp = FuelableCell.GetFirstBuilding(Map)?.GetComp<CompRefuelable>();
                        if (refuelableComp != null && refuelableComp.Props.fuelFilter.Allows(item))
                        {
                            refuelableComp.Refuel(item);
                        }
                    }
                }
            }
        }
        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            GenDraw.DrawFieldEdges(new List<IntVec3>(GenAdj.CellsAdjacent8Way(this)));
            GenDraw.DrawFieldEdges(new List<IntVec3>() { FuelableCell }, Color.yellow);
        }
    }
}

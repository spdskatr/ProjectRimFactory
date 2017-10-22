using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ProjectRimFactory.CultivatorTools
{
    public abstract class Building_SquareCellIterator : Building
    {
        public SquareCellIterator iter;
        protected int currentPosition;
        public IntVec3 Current => iter.cellPattern[currentPosition] + Position;

        public bool Fueled => GetComp<CompRefuelable>()?.HasFuel ?? true;

        public bool Powered => GetComp<CompPowerTrader>()?.PowerOn ?? true;

        public virtual int TickRate => 250;

        public virtual bool CellValidator(IntVec3 c)
        {
            return c.InBounds(Map);
        }

        public abstract bool DoIterationWork(IntVec3 c);

        public override void PostMake()
        {
            base.PostMake();
            iter = new SquareCellIterator(def.GetModExtension<CultivatorDefModExtension>().squareAreaRadius);
        }

        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % TickRate == 0 && Powered && Fueled)
                DoTickerWork();
        }
        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            GenDraw.DrawFieldEdges(new List<IntVec3> { Current }, Color.yellow);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref currentPosition, "currentNumber", 1);
        }
        public void DoTickerWork()
        {
            base.TickRare();
            var cell = Current;
            var zone = cell.GetZone(Map);
            if (CellValidator(cell))
            {
                if (!DoIterationWork(cell)) return;
            }
            MoveNextInternal();
        }

        protected virtual void MoveNextInternal()
        {
            for (int i = 0; i < 10; i++)
            {
                currentPosition++;
                var num = iter.cellPattern.Length;
                if (currentPosition + 1 >= num)
                    currentPosition = 0;
                var cell = Current;
                var zone = cell.GetZone(Map);
                if (CellValidator(cell))
                {
                    break;
                }
            }
        }
    }
    public class SquareCellIterator
    {
        int rangeInt;
        public int Range => rangeInt;
        public IntVec3[] cellPattern;
        public SquareCellIterator(int range)
        {
            rangeInt = range;
            cellPattern = new IntVec3[(range * 2 + 1) * (range * 2 + 1)];
            int currentIter = 0;
            for (int i = -range; i <= range; i++)
            {
                if ((i & 1) == 0)
                {
                    for (int j = -range; j <= range; j++, currentIter++)
                    {
                        cellPattern[currentIter] = new IntVec3(i, 0, j);
                    }
                }
                else
                {
                    for (int j = range; j >= -range; j--, currentIter++)
                    {
                        cellPattern[currentIter] = new IntVec3(i, 0, j);
                    }
                }
            }
        }
    }
    public class PlaceWorker_HighlightPlaceableCells : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
        {
            base.DrawGhost(def, center, rot);
            int squareAreaRadius = def.GetModExtension<CultivatorDefModExtension>().squareAreaRadius;
            List<IntVec3> list = new List<IntVec3>((squareAreaRadius * 2 + 1) * (squareAreaRadius * 2 + 1));
            for (int i = -squareAreaRadius; i <= squareAreaRadius; i++)
            {
                for (int j = -squareAreaRadius; j <= squareAreaRadius; j++)
                {
                    list.Add(new IntVec3(i, 0, j) + center);
                }
            }
            GenDraw.DrawFieldEdges(list);
        }
    }
}

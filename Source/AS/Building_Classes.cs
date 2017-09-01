using System.Collections.Generic;
using Verse;
using RimWorld;
using UnityEngine;
using System.Reflection;

namespace AnimalStation
{
    public abstract class Building_CompHarvester : Building_Storage
    {
        public IEnumerable<IntVec3> ScannerCells
        {
            get
            {
                return GenAdj.OccupiedRect(this).ExpandedBy(1).Cells;
            }
        }

        public abstract CompHasGatherableBodyResource GetProperComp(Pawn pawn);

        public override void TickRare()
        {
            base.TickRare();
            if (!GetComp<CompPowerTrader>().PowerOn) return;
            foreach (IntVec3 c in ScannerCells)
            {
                var p = c.GetThingList(Map).Find(t => t is Pawn pawn && GetProperComp(pawn) != null);
                if (p == null || p.Faction != Faction.OfPlayer) continue;
                var comp = GetProperComp(p as Pawn);
                var reflection = typeof(CompHasGatherableBodyResource);
                int i = GenMath.RoundRandom((int)reflection.GetProperty("ResourceAmount", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(comp, null) * comp.Fullness);
                if (i == 0) continue;
                var resource = (ThingDef)reflection.GetProperty("ResourceDef", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(comp, null);
                while (i > 0)
                {
                    int num = Mathf.Clamp(i, 1, resource.stackLimit);
                    i -= num;
                    Thing thing = ThingMaker.MakeThing(resource, null);
                    thing.stackCount = num;
                    GenPlace.TryPlaceThing(thing, p.Position, p.Map, ThingPlaceMode.Near, null);
                }
                reflection.GetField("fullness", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(comp, 0f);
            }
        }
    }
    public class Building_Shearer : Building_CompHarvester
    {
        public override CompHasGatherableBodyResource GetProperComp(Pawn pawn)
        {
            return pawn.GetComp<CompShearable>();
        }
    }
    public class Building_Milker : Building_CompHarvester
    {
        public override CompHasGatherableBodyResource GetProperComp(Pawn pawn)
        {
            return pawn.GetComp<CompMilkable>();
        }
    }
    public class Building_GenericBodyResourceGatherer : Building_CompHarvester
    {
        public override CompHasGatherableBodyResource GetProperComp(Pawn pawn)
        {
            return (pawn.GetComps<CompHasGatherableBodyResource>().TryRandomElement(out CompHasGatherableBodyResource result)) ? result : null;
        }
    }
}

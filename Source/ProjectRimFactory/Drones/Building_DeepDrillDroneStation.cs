using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;

namespace ProjectRimFactory.Drones
{
    public class Building_DeepDrillDroneStation : Building_DroneStation
    {
        public override Job TryGiveJob()
        {
            CompDeepDrill comp = Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.DeepDrill)
                       .FirstOrDefault(b => !Map.reservationManager.IsReservedByAnyoneOf(b, Faction.OfPlayer) && b.GetComp<CompDeepDrill>().CanDrillNow())
                       ?.GetComp<CompDeepDrill>();
            if (comp != null)
            {
                return new Job(JobDefOf.OperateDeepDrill, comp.parent)
                {
                    expiryInterval = 30000
                };
            }
            return null;
        }
    }
}

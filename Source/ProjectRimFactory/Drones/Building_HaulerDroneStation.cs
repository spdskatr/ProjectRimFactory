using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace ProjectRimFactory.Drones
{
    public class Building_HaulerDroneStation : Building_DroneStation
    {
        public override Job TryGiveJob()
        {
            Job result = null;
            Pawn pawn = MakeDrone();
            GenSpawn.Spawn(pawn, Position, Map);
            foreach (Thing t in Map.listerHaulables.ThingsPotentiallyNeedingHauling())
            {
                if (!Map.reservationManager.IsReservedByAnyoneOf(t, Faction) && HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, t, false))
                {
                    result = HaulAIUtility.HaulToStorageJob(pawn, t);
                    if (result != null)
                    {
                        break;
                    }
                }
            }
            pawn.DeSpawn();
            return result;
        }
    }
}

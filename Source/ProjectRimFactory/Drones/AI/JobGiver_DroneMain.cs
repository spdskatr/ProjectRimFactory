using ProjectRimFactory.Common;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace ProjectRimFactory.Drones.AI
{
    public class JobGiver_DroneMain : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Pawn_Drone drone = (Pawn_Drone)pawn;
            Job job = drone.setJob;
            if (job != null && !drone.jobStarted && job.targetA.Thing.TryGetComp<CompDeepDrill>().CanDrillNow())
            {
                drone.jobStarted = true;
                return job;
            }

            if (drone.station != null)
            {
                return new Job(PRFDefOf.PRFDrone_ReturnToStation, drone.station);
            }
            return null;
        }
    }
}

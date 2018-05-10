using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;
using Verse.AI;

namespace ProjectRimFactory.Drones
{
    public class Building_WorkGiverDroneStation : Building_DroneStation
    {
        public static readonly MethodInfo TryGiveJobPrioritizedMethod = typeof(JobGiver_Work).GetMethod("GiverTryGiveJobPrioritized", BindingFlags.NonPublic | BindingFlags.Instance);
        public override Job TryGiveJob()
        {
            Job result = null;
            Pawn pawn = MakeDrone();
            GenSpawn.Spawn(pawn, Position, Map);
            pawn.workSettings = new Pawn_WorkSettings(pawn);
            pawn.workSettings.EnableAndInitialize();
            pawn.workSettings.DisableAll();
            foreach (WorkTypeDef def in extension.workTypes)
            {
                pawn.workSettings.SetPriority(def, 3);
            }
            JobGiver_Work jobGiver = new JobGiver_Work();
            result = jobGiver.TryIssueJobPackage(pawn, default(JobIssueParams)).Job;
            Log.Message($"ThinkResult.Job: {result?.ToString() ?? "(null)"}");
            pawn.DeSpawn();
            return result;
        }
    }
}

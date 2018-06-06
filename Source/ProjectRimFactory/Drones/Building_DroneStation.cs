using ProjectRimFactory.Common;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ProjectRimFactory.Drones
{
    public abstract class Building_DroneStation : Building
    {
        public int dronesLeft;
        protected DefModExtension_DroneStation extension;
        protected List<Pawn_Drone> spawnedDrones = new List<Pawn_Drone>();
        public override void PostMake()
        {
            base.PostMake();
            extension = def.GetModExtension<DefModExtension_DroneStation>();
            dronesLeft = extension.maxNumDrones;
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            extension = def.GetModExtension<DefModExtension_DroneStation>();
        }
        public override void Draw()
        {
            base.Draw();
            if (extension.displayDormantDrones)
            {
                DrawDormantDrones();
            }
        }

        public override void DeSpawn()
        {
            base.DeSpawn();
            List<Pawn_Drone> drones = spawnedDrones.ToList();
            for (int i = 0; i < drones.Count; i++)
            {
                drones[i].Destroy();
            }
        }

        public virtual void DrawDormantDrones()
        {
            foreach (IntVec3 cell in GenAdj.CellsOccupiedBy(this).Take(dronesLeft))
            {
                PRFDefOf.PRFDrone.graphic.DrawFromDef(cell.ToVector3ShiftedWithAltitude(AltitudeLayer.LayingPawn), default(Rot4), PRFDefOf.PRFDrone);
            }
        }

        public abstract Job TryGiveJob();

        public override void Tick()
        {
            base.Tick();
            if (dronesLeft > 0 && this.IsHashIntervalTick(60) && GetComp<CompPowerTrader>()?.PowerOn != false)
            {
                Job job = TryGiveJob();
                if (job != null)
                {
                    Pawn_Drone drone = MakeDrone();
                    GenSpawn.Spawn(drone, Position, Map);
                    drone.jobs.StartJob(job);
                }
            }
        }

        public void Notify_DroneMayBeLost(Pawn_Drone drone)
        {
            if (spawnedDrones.Contains(drone))
            {
                spawnedDrones.Remove(drone);
                dronesLeft++;
            }
        }

        public override string GetInspectString()
        {
            StringBuilder builder = new StringBuilder();
            string str = base.GetInspectString();
            if (!string.IsNullOrEmpty(str))
            {
                builder.AppendLine(str);
            }
            builder.Append("PRFDroneStation_NumberOfDrones".Translate(dronesLeft));
            return builder.ToString();
        }

        public virtual Pawn_Drone MakeDrone()
        {
            dronesLeft--;
            Pawn_Drone drone = (Pawn_Drone)PawnGenerator.GeneratePawn(PRFDefOf.PRFDroneKind, Faction);
            drone.station = this;
            spawnedDrones.Add(drone);
            return drone;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref dronesLeft, "dronesLeft");
        }
    }
}

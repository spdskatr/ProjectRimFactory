using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI;

namespace ProjectRimFactory.Drones
{
    public class Pawn_Drone : Pawn
    {
        public Job setJob;
        public bool jobStarted;
        public Building_DroneStation station;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            skills = new Pawn_SkillTracker(this);
            foreach (SkillRecord record in skills.skills)
            {
                record.levelInt = 10;
                record.passion = Passion.None;
            }
            story = new Pawn_StoryTracker(this)
            {
                bodyType = BodyType.Thin,
                crownType = CrownType.Average,
                childhood = DroneBackstories.childhood,
                adulthood = DroneBackstories.adulthood
            };
            drafter = new Pawn_DraftController(this);
            Name = new NameSingle("Drone");
        }

        public override void Tick()
        {
            base.Tick();
            foreach (SkillRecord sr in skills.skills)
            {
                if (sr.xpSinceLastLevel > 1f)
                {
                    sr.xpSinceMidnight = 1f;
                    sr.xpSinceLastLevel = 1f;
                }
            }
        }
    }
}

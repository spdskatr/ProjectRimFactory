using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ProjectRimFactory.Industry
{
    public class Building_DeepQuarry : Building
    {
        public const float ProduceMtbHours = 4f;
        static IEnumerable<ThingDef> cachedPossibleRockDefCandidates;
        protected static IEnumerable<ThingDef> PossibleRockDefCandidates
        {
            get
            {
                if (cachedPossibleRockDefCandidates != null)
                {
                    return cachedPossibleRockDefCandidates;
                }
                return cachedPossibleRockDefCandidates = from def in DefDatabase<ThingDef>.AllDefs
                                                         where def.building != null && def.building.isNaturalRock && !def.building.isResourceRock
                                                         select def;
            }
        }
        public virtual IntVec3 OutputCell => Position + Rotation.FacingCell * 2;
        public override void TickLong()
        {
            if (Rand.MTBEventOccurs(ProduceMtbHours, GenDate.TicksPerHour, GenTicks.TickLongInterval))
            {
                GenerateChunk();
            }
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            GenDraw.DrawFieldEdges(new List<IntVec3>() { OutputCell }, Color.yellow);
        }

        public virtual void GenerateChunk()
        {
            GenPlace.TryPlaceThing(GetChunkThingToPlace(), OutputCell, Map, ThingPlaceMode.Near);
        }

        protected virtual Thing GetChunkThingToPlace()
        {
            TerrainDef terrainDef = Position.GetTerrain(Map);
            foreach (ThingDef def in PossibleRockDefCandidates)
            {
                if (def.leaveTerrain == terrainDef)
                {
                    return ThingMaker.MakeThing(def.building.mineableThing);
                }
            }
            return ThingMaker.MakeThing(PossibleRockDefCandidates.RandomElement());
        }
    }
}

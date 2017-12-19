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
                return cachedPossibleRockDefCandidates = from def in ThingCategoryDef.Named("StoneChunks").DescendantThingDefs
                                                         select def;
            }
        }

        public int ProducedChunksTotal = 0;

        public virtual IntVec3 OutputCell => Position + Rotation.FacingCell * 2;
        public override void TickLong()
        {
            if (Rand.MTBEventOccurs(ProduceMtbHours, GenDate.TicksPerHour, GenTicks.TickLongInterval))
            {
                GenerateChunk();
            }
        }
        public override void ExposeData()
        {
            Scribe_Values.Look(ref ProducedChunksTotal, "producedTotal");
            base.ExposeData();
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            GenDraw.DrawFieldEdges(new List<IntVec3>() { OutputCell }, Color.yellow);
        }

        public virtual void GenerateChunk()
        {
            GenPlace.TryPlaceThing(GetChunkThingToPlace(), OutputCell, Map, ThingPlaceMode.Near);
            ProducedChunksTotal++;
        }

        protected virtual Thing GetChunkThingToPlace()
        {
            return ThingMaker.MakeThing(PossibleRockDefCandidates.RandomElement());
        }
    }
}

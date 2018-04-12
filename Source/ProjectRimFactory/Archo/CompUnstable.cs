using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace ProjectRimFactory.Archo
{
    public class CompUnstable : ThingComp
    {
        public int ticksLeft;
        public CompProperties_Unstable Props
        {
            get
            {
                return (CompProperties_Unstable)props;
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            ticksLeft--;
            if (ticksLeft <= 0)
            {
                Messages.Message("PRF_DisintegrationMessage".Translate(parent.LabelCap), new GlobalTargetInfo(parent.Position, parent.Map), MessageTypeDefOf.NegativeEvent);
                parent.Destroy();
            }
        }
        public override string CompInspectStringExtra()
        {
            return "PRF_TimeLeftToDisintegrate".Translate(ticksLeft.ToStringTicksToPeriod());
        }
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            ticksLeft = Props.ticksToDisintegrate;
        }
        public override void PostExposeData()
        {
            Scribe_Values.Look(ref ticksLeft, "ticksLeft");
        }
    }
}
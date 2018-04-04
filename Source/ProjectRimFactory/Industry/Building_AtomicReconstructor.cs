using ProjectRimFactory.Common;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ProjectRimFactory.Industry
{
    public class Building_AtomicReconstructor : Building
    {
        private ThingDef thingToGenerate;
        public int progressTicks;
        CompPowerTrader powerComp;
        CompRefuelable refuelableComp;
        CompOutputAdjustable outputComp;

        public int TotalWorkRequired
        {
            get
            {
                if (ThingToGenerate == null)
                    return 0;
                return Mathf.RoundToInt(StatDefOf.MarketValue.Worker.GetValue(StatRequest.For(ThingToGenerate, null)) * 100);
            }
        }

        public float FuelConsumptionPerTick
        {
            get
            {
                if (ThingToGenerate == null)
                    return 0;
                return ThingToGenerate.PaperclipAmount() / TotalWorkRequired;
            }
        }

        public string ProgressToStringPercent
        {
            get
            {
                return ThingToGenerate == null ? 0f.ToStringPercent() : (progressTicks / (float)TotalWorkRequired).ToStringPercent();
            }
        }

        public ThingDef ThingToGenerate
        {
            get
            {
                return thingToGenerate;
            }
            set
            {
                thingToGenerate = value;
                progressTicks = 0;
            }
        }
        public override void PostMake()
        {
            base.PostMake();
            powerComp = GetComp<CompPowerTrader>();
            refuelableComp = GetComp<CompRefuelable>();
            outputComp = GetComp<CompOutputAdjustable>();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            refuelableComp = GetComp<CompRefuelable>();
            outputComp = GetComp<CompOutputAdjustable>();
        }

        public override void Tick()
        {
            base.Tick();
            if (powerComp.PowerOn && this.IsHashIntervalTick(10))
            {
                if (ThingToGenerate != null)
                {
                    float fuel = refuelableComp.Fuel;
                    float consumption = FuelConsumptionPerTick * 10;
                    if (fuel >= consumption)
                    {
                        refuelableComp.ConsumeFuel(consumption);
                        progressTicks += 10;
                        if (progressTicks >= TotalWorkRequired)
                        {
                            Thing thing = ThingMaker.MakeThing(ThingToGenerate);
                            GenPlace.TryPlaceThing(thing, outputComp.CurrentCell, Map, ThingPlaceMode.Near);
                            progressTicks = 0;
                        }
                    }
                }
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
            builder.Append("AtomicReconstructorProgress".Translate(ProgressToStringPercent));
            return builder.ToString().TrimEndNewlines();
        }
    }
}

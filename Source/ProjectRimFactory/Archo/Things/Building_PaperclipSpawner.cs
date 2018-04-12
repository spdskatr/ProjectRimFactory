using ProjectRimFactory.Common;
using ProjectRimFactory.Industry;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ProjectRimFactory.Archo.Things
{
    public class Building_PaperclipSpawner : Building
    {
        CompPowerTrader powerComp;
        CompOutputAdjustable outputComp;
        public override void PostMake()
        {
            base.PostMake();
            powerComp = GetComp<CompPowerTrader>();
            outputComp = GetComp<CompOutputAdjustable>();
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            outputComp = GetComp<CompOutputAdjustable>();
        }
        public override void Tick()
        {
            if (this.IsHashIntervalTick(60) && powerComp.PowerOn) // Spawns one batch of paperclips per second, if powered
            {
                Thing result = ThingMaker.MakeThing(PRFDefOf.Paperclip); // Spawns 10000 paperclips per batch
                result.stackCount = 10000;
                GenPlace.TryPlaceThing(result, outputComp.CurrentCell, Map, ThingPlaceMode.Near);
            }
        }
    }
}

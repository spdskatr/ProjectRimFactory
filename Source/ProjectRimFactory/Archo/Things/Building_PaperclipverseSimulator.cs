using ProjectRimFactory.Common;
using ProjectRimFactory.Industry;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ProjectRimFactory.Archo.Things
{
    public class Building_PaperclipverseSimulator : Building
    {
        int paperclipsPerSecond = 1;
        CompPowerTrader powerComp;
        CompOutputAdjustable outputComp;
        public int ProductionFactor
        {
            get
            {
                return paperclipsPerSecond;
            }
            set
            {
                paperclipsPerSecond = value;
                powerComp.PowerOutput = -paperclipsPerSecond * powerComp.Props.basePowerConsumption;
            }
        }
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
            if (this.IsHashIntervalTick(60))
            {
                Thing t = ThingMaker.MakeThing(PRFDefOf.Paperclip);
                t.stackCount = paperclipsPerSecond;
                GenPlace.TryPlaceThing(t, outputComp.CurrentCell, Map, ThingPlaceMode.Near);
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref paperclipsPerSecond, "paperclipsPerSecond", 1);
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos()) yield return g;
            yield return new Command_Action()
            {
                defaultLabel = "SetPaperclipProductionPerSecond".Translate(),
                defaultDesc = "SetPaperclipProductionPerSecond_Desc".Translate(),
                icon = CompPaperclipPowerPlant.SetTargetFuelLevelCommand,
                action = () => Find.WindowStack.Add(new Dialog_Slider(j => "PaperclipProductionPerSecond".Translate(j), 1, 100, i => paperclipsPerSecond = i, paperclipsPerSecond))
            };
        }
    }
}

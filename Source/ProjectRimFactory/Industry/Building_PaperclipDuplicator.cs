using ProjectRimFactory.Common;
using ProjectRimFactory.Storage;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ProjectRimFactory.Industry
{
    public class Building_PaperclipDuplicator : Building
    {
        long paperclipCount;
        int lastTick = Find.TickManager.TicksGame;
        public Building_MassStorageUnit boundStorageUnit;

        CompOutputAdjustable outputComp;
        CompPowerTrader powerComp;
        public long PaperclipsActual
        {
            get
            {
                return (long)(paperclipCount * Math.Pow(1.05, (Find.TickManager.TicksGame - lastTick).TicksToDays()));
            }
            set
            {
                paperclipCount = value;
                lastTick = Find.TickManager.TicksGame;
            }
        }
        public virtual void DepositPaperclips(int count)
        {
            PaperclipsActual = PaperclipsActual + count;
        }
        public virtual void WithdrawPaperclips(int count)
        {
            PaperclipsActual = PaperclipsActual - count;
        }
        public override void PostMake()
        {
            base.PostMake();
            outputComp = GetComp<CompOutputAdjustable>();
            powerComp = GetComp<CompPowerTrader>();
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            outputComp = GetComp<CompOutputAdjustable>();
            powerComp = GetComp<CompPowerTrader>();
        }
        public override string GetInspectString()
        {
            StringBuilder builder = new StringBuilder();
            string str = base.GetInspectString();
            if (!string.IsNullOrEmpty(str))
            {
                builder.AppendLine(str);
            }
            builder.AppendLine("PaperclipsInBank".Translate(PaperclipsActual));
            if (boundStorageUnit != null)
            {
                builder.AppendLine("PaperclipsInStorageUnit".Translate(boundStorageUnit.StoredItems.Where(t => t.def == PRFDefOf.Paperclip).Sum(t => t.stackCount)));
            }
            else
            {
                builder.AppendLine("PRFNoBoundStorageUnit".Translate());
            }
            return builder.ToString().TrimEndNewlines();
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "DEBUG: Double paperclip amount",
                    action = () => PaperclipsActual *= 2
                };
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref paperclipCount, "paperclipCount");
            Scribe_Values.Look(ref lastTick, "lastTick");
            Scribe_References.Look(ref boundStorageUnit, "boundStorageUnit");
        }
    }
}

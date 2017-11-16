using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ProjectRimFactory.SAL3.Things.Assemblers.Special
{
    public class Building_Assembler_Learning : Building_SmartAssembler
    {
        public float FactorOffset
        {
            get
            {
                return 0.75f;
            }
        }
        WorkSpeedFactorManager manager = new WorkSpeedFactorManager();
        protected override float ProductionSpeedFactor
        {
            get
            {
                return currentBillReport == null ? FactorOffset : manager.GetFactorFor(currentBillReport.bill.recipe) + FactorOffset;
            }
        }
        public override void Tick()
        {
            base.Tick();
            if (currentBillReport != null && this.IsHashIntervalTick(60))
            {
                manager.IncreaseWeight(currentBillReport.bill.recipe, 0.001f * currentBillReport.bill.recipe.workSkillLearnFactor);
            }
        }
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.GetInspectString());
            if (currentBillReport != null)
            {
                stringBuilder.AppendLine("SALCurrentProductionSpeed".Translate(currentBillReport.bill.recipe.label,ProductionSpeedFactor.ToStringPercent()));
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }
        public QualityCategory GetRandomProductionQuality()
        {
            float centerX = ProductionSpeedFactor * 2f;
            float num = Rand.Gaussian(centerX, 1.25f);
            num = Mathf.Clamp(num, 0f, QualityUtility.AllQualityCategories.Count - 0.5f);
            return (QualityCategory)((int)num);
        }
        protected override void PostProcessRecipeProduct(Thing thing)
        {
            CompQuality compQuality = thing.TryGetComp<CompQuality>();
            if (compQuality != null)
            {
                compQuality.SetQuality(GetRandomProductionQuality(), ArtGenerationContext.Colony);
            }
        }
        public override void ExposeData()
        {
            Scribe_Deep.Look(ref manager, "manager");
            base.ExposeData();
        }
    }
}

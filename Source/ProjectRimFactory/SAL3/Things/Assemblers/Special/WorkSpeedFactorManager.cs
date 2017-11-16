using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ProjectRimFactory.SAL3.Things.Assemblers.Special
{
    public sealed class WorkSpeedFactorEntry : IExposable
    {
        public const float LearningRateCachedDefault = 1f / GenDate.TicksPerTwelfth;
        int lastTick = 0;
        float factorCached = 0;
        float learningRateCached = LearningRateCachedDefault;
        public float LearningRate
        {
            get
            {
                return learningRateCached;
            }
            set
            {
                UpdateFactorCache();
                learningRateCached = value;
            }
        }
        public int DeltaTicks => Find.TickManager.TicksAbs - lastTick;
        public float FactorFinal
        {
            get
            {
                return factorCached * Mathf.Pow(2, -(DeltaTicks * LearningRate));
            }
            set
            {
                factorCached = value;
                lastTick = Find.TickManager.TicksAbs;
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref lastTick, "lastTick");
            Scribe_Values.Look(ref factorCached, "factorCached");
            Scribe_Values.Look(ref learningRateCached, "learningRateCached", forceSave: true);
        }

        private void UpdateFactorCache()
        {
            if (factorCached != 0f)
            {
                factorCached *= Mathf.Pow(2, -(DeltaTicks * LearningRate));
            }
            lastTick = Find.TickManager.TicksAbs;
        }
    }
    public class WorkSpeedFactorManager : IExposable
    {
        public float factorOffset = 0.75f;
        public Dictionary<RecipeDef, WorkSpeedFactorEntry> factors = new Dictionary<RecipeDef, WorkSpeedFactorEntry>();
        float learningRateCached = WorkSpeedFactorEntry.LearningRateCachedDefault;
        public float LearningRate
        {
            get
            {
                return learningRateCached;
            }
            set
            {
                foreach (RecipeDef recipe in factors.Keys)
                {
                    factors[recipe].LearningRate = value;
                }
                learningRateCached = value;
            }
        }
        public void IncreaseWeight(RecipeDef recipe, float factor)
        {
            if (factors.TryGetValue(recipe, out WorkSpeedFactorEntry entry))
            {
                entry.FactorFinal += factor;
            }
            else
            {
                factors.Add(recipe, new WorkSpeedFactorEntry() { FactorFinal = factor });
            }
        }
        public float GetFactorFor(RecipeDef recipe)
        {
            if (factors.TryGetValue(recipe, out WorkSpeedFactorEntry val))
            {
                return val.FactorFinal + factorOffset;
            }
            return factorOffset;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref factors, "factors", LookMode.Def, LookMode.Deep);
        }
    }
}

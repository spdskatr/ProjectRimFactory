using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using System.Linq;

namespace ProjectSAL
{
    public class ModExtension_Assembler : DefModExtension
    {
        public float powerUsageLowPower = 50f;
        public List<SkillLevel> skills = new List<SkillLevel>();
        public float globalFactor = 1f;
        public int defaultSkillLevel = 1;
        public int FindSkillAndGetLevel(SkillDef skillDef, int defaultVal)
        {
            for (int i = 0; i < (skills?.Count ?? 0); i++)
            {
                if (skills[i].skillDef == skillDef)
                {
                    return skills[i].level;
                }
            }
            return defaultVal;
        }
    }
	public class SkillLevel
	{
		public SkillDef skillDef;
		public int level = 5;
        public float workSpeedFactorExtra = 1f;
        public override string ToString()
        {
            return (ProjectSAL_Utilities.FuzzyCompareFloat(workSpeedFactorExtra, 1f, 0.00001f)) ? level.ToString() : level + " x " + workSpeedFactorExtra.ToStringPercent();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;

namespace ProjectSAL
{
    public class ProjectSAL_OnGameLoadChecker : WorldComponent
    {
        public ProjectSAL_OnGameLoadChecker(World world) : base(world)
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            CheckCoreDriller();
        }

        /// <summary>
        /// Change the name of this method to know you've thoroughly disabled it. :P
        /// </summary>
        static void DoBackstoryStartupDisabled()
        {
            var sb = new StringBuilder();
            foreach (var item in BackstoryDatabase.allBackstories)
            {
                if (!(item.Value.DisabledWorkTypes?.Any() ?? false))
                {
                    sb.AppendLine("Discovered backstory with no work types! " + item.Key + " -> " + item.Value);
                }
            }
            Log.Message(sb.ToString());
        }

        public static void CheckCoreDriller()
        {
            var def = DefDatabase<ThingDef>.GetNamedSilentFail("CoreDrill");
            if (def != null)
            {
                if (LoadedModManager.GetMod<SALMod>().settings.FixCoreDriller)
                {
                    DefDatabase<ThingDef>.GetNamedSilentFail("CoreDrill").interactionCellOffset = new IntVec3(0,0,-2);
                }
                else
                {
                    DefDatabase<ThingDef>.GetNamedSilentFail("CoreDrill").interactionCellOffset = new IntVec3(0,0,-1);
                }
            }
        }
    }
    static class ProjectSAL_Utilities
    {
        /// <summary>
        /// This will reset every time the game initialises (not when map loads).
        /// </summary>
        public static List<string> indexes = new List<string>();
    	/// <summary>
    	/// This value is normally revision number + 1
    	/// </summary>
        public static bool FuzzyCompareFloat(float a, float b, float marginOfError)
        {
            return Mathf.Abs(a - b) < marginOfError;
        }
        /// <summary>
        /// Returns current Rot4 as a compass direction.
        /// </summary>
        public static string AsCompassDirection(this Rot4 rot)
        {
            switch (rot.AsByte)
            {
                case 0:
                    return "SAL_North".Translate();
                case 1:
                    return "SAL_East".Translate();
                case 2:
                    return "SAL_South".Translate();
                case 3:
                    return "SAL_West".Translate();
                default:
                    return "SAL_InvalidDirection".Translate();
            }
        }
        public static float CalculateCraftingSpeedFactor(this StatDef workSpeedStat, Pawn pawn, ModExtension_Assembler extension)
        {
            if (workSpeedStat == null || pawn == null) return 1f;
        	float basenum = pawn.GetStatValue(workSpeedStat, true);
            List<SkillNeed> skillNeedFactors = workSpeedStat.skillNeedFactors ?? new List<SkillNeed>();
            for (int i = 0; i < skillNeedFactors.Count; i++) 
        	{
                var skillNeed = skillNeedFactors[i];
                var extraFactor = extension.skills.Find(s => s.skillDef == skillNeed.skill)?.workSpeedFactorExtra ?? 1;
                basenum *= extraFactor;
            }
            return basenum;
        }
        public static void ReceiveLetterOnce(string label, string text, LetterDef textLetterDef, GlobalTargetInfo lookTarget, string debugInfo)
        {
            //Only send if both letter stack and local indexes do not have the letter
            if (!indexes.Contains(debugInfo) && !Find.LetterStack.LettersListForReading.Any(l => l.debugInfo == debugInfo))
            {
                indexes.Add(debugInfo);
                Find.LetterStack.ReceiveLetter(label, text, textLetterDef, lookTarget, debugInfo);
            }
        }

        /// <summary>
        /// Provides warning for core driller + S.A.L. combination.
        /// </summary>
        public static void CheckForCoreDrillerSetting(this Building_Assembler crafter)
        {
            var table = crafter.Map.thingGrid.ThingsListAt(crafter.WorkTableCell).OfType<Building>().FirstOrDefault(b => b.def.defName == "CoreDrill");
            if (table != null && !LoadedModManager.GetMod<SALMod>().settings.FixCoreDriller)
            {
                ReceiveLetterOnce("SALInformation_CoreDriller".Translate(), "SALInformation_CoreDriller_Desc".Translate(), DefDatabase<LetterDef>.GetNamed("SALInformation"), crafter.InteractionCell.GetFirstBuilding(crafter.Map), "SALInformation_CoreDriller");
            }
        }

        public static void DoSkillsAnalysis(this Pawn p)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Beginning skills analysis.");
            foreach (var skill in p.skills.skills)
            {
                stringBuilder.AppendLine(skill.def + ": " + skill.levelInt + " Disabled: " + skill.TotallyDisabled);
            }
            stringBuilder.AppendLine("Beginning food poison test.");
            /*
            foreach (var stat in DefDatabase<StatDef>.AllDefs)
            {
                try
                {
                    stringBuilder.AppendLine(stat.defName + ": " + p.GetStatValue(stat));
                }
                catch
                {
                    stringBuilder.AppendLine(stat.defName + ": ERROR");
                }
            }
            */
            stringBuilder.AppendLine(StatDefOf.FoodPoisonChance.Worker.GetExplanation(StatRequest.For(p), ToStringNumberSense.Absolute));
            Log.Message(stringBuilder.ToString());
        }
    }
    /// <summary>
    /// Programmer trick to save IngredientCount.
    /// </summary>
    public class _IngredientCount : IExposable
    {
        public ThingFilter filter = new ThingFilter();
        public float count = 1f;
        
        public float Count
        {
        	get
        	{
        		return count;
        	}
        }

        /// <summary>
        /// IMPORTANT DO NOT REMOVE
        /// </summary>
        public _IngredientCount()
        {

        }

        public _IngredientCount(ThingFilter f, float c)
        {
            filter = f;
            count = c;
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref filter, "filter");
            Scribe_Values.Look(ref count, "count");
        }
		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"(",
				count,
				"x ",
			    filter.ToString(),
				")"
			});
		}
        public static explicit operator IngredientCount(_IngredientCount ingredient)
        {
            var New = new IngredientCount
            {
                filter = ingredient.filter,
            };
            New.SetBaseCount(ingredient.count);
            return New;
        }
        public static implicit operator _IngredientCount(IngredientCount old)
        {
            return new _IngredientCount(old.filter, old.GetBaseCount());
        }
    }
}

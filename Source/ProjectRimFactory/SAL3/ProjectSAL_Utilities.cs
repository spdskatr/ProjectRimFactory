using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;
using System.Reflection;

namespace ProjectRimFactory.SAL3
{
    static class ProjectSAL_Utilities
    {
        /// <summary>
        /// This will reset every time the game initialises (not when map loads).
        /// </summary>
        public static List<string> indices = new List<string>();
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

        public static void ReceiveLetterOnce(string label, string text, LetterDef textLetterDef, GlobalTargetInfo lookTarget, string debugInfo)
        {
            //Only send if both letter stack and local indexes do not have the letter
            if (!indices.Contains(debugInfo) && !Find.LetterStack.LettersListForReading.Any(l => l.debugInfo == debugInfo))
            {
                indices.Add(debugInfo);
                Find.LetterStack.ReceiveLetter(label, text, textLetterDef, lookTarget, debugInfo);
            }
        }

        public static Thing CalculateDominantIngredient(RecipeDef currentRecipe, IEnumerable<Thing> thingRecord)
        {
            var stuffs = thingRecord.Where(t => t.def.IsStuff);
            if (!thingRecord.Any())
            {
                if (currentRecipe.ingredients.Count > 0) Log.Warning("S.A.L.: Had no thingRecord of items being accepted, but crafting recipe has ingredients. Did you reload a save?");
                return ThingMaker.MakeThing(ThingDefOf.Steel);
            }
            if (stuffs.Any())
            {
                if (currentRecipe.productHasIngredientStuff)
                {
                    return stuffs.OrderByDescending(x => x.stackCount).First();
                }
                if (currentRecipe.products.Any(x => x.thingDef.MadeFromStuff))
                {
                    return stuffs.RandomElementByWeight(x => x.stackCount);
                }
            }
            return ThingMaker.MakeThing(ThingDefOf.Steel);
        }

        public static IEnumerable<Thing> ButcherProductsNoPawn(this Thing thing, Map map, Faction faction)
        {
            if (thing is Corpse c)
            {
                foreach (Thing prod in c.InnerPawn.ButcherProducts(null, 1f))
                {
                    yield return prod;
                }
                if (c.InnerPawn.RaceProps.Humanlike)
                {
                    foreach (Pawn p in map.mapPawns.SpawnedPawnsInFaction(faction))
                    {
                        p.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.KnowButcheredHumanlikeCorpse);
                    }
                }
                yield break;
            }
            foreach (Thing t in thing.ButcherProducts(null, 1f))
            {
                yield return t;
            }
        }
    }
}

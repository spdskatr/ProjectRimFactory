using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;
using System.Reflection;

namespace ProjectRimFactory.SAL3.Tools
{
    static class ProjectSAL_Utilities
    {
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
    }
}

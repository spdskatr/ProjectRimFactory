using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ProjectRimFactory.SAL3.Defs
{
    //public class RecipeReaderResolverDef : Def
    //{
    //    public const string RecipeReaderResolverPrefix = "SALMakeSchematic_";
    //    public override void ResolveReferences()
    //    {
    //        List<RecipeDef> recipes = DefDatabase<RecipeDef>.AllDefsListForReading;
    //        for (int i = 0; i < recipes.Count; i++)
    //        {
    //            if (recipes[i].removesHediff == null)
    //            {
    //                ThingDef thingDef = new ThingDef()
    //                {
    //                    defName = "Schematic_" + recipes[i].defName,
    //                    label = "SchematicLabel".Translate(recipes[i].label),
    //                    description = "SchematicDescription".Translate(),
    //                };
    //                ThingFilter thingFilter1 = new ThingFilter();
    //                thingFilter1.SetAllow(ThingDefOf.Steel, true);
    //                ThingFilter thingFilter2 = new ThingFilter();
    //                thingFilter2.SetAllow(ThingDefOf.Component, true);
    //                IngredientCount ingredient1 = new IngredientCount() { filter = thingFilter1 };
    //                ingredient1.SetBaseCount(50);
    //                IngredientCount ingredient2 = new IngredientCount() { filter = thingFilter2 };
    //                ingredient2.SetBaseCount(2);
    //                RecipeDef recipe = new RecipeDef()
    //                {
    //                    defName = RecipeReaderResolverPrefix + recipes[i].defName,
    //                    label = "SchematicRecipeLabel".Translate(recipes[i].label),
    //                    description = "SchematicRecipeDescription".Translate(),
    //                    workAmount = recipes[i].workAmount,
    //                    ingredients = new List<IngredientCount>() { ingredient1, ingredient2 },
    //                    products = new List<ThingCountClass>() { new ThingCountClass(thingDef, 1) },
    //                    jobString = "SchematicJobString".Translate(),
                        
    //                };
    //            }
    //        }
    //    }
    //}
}

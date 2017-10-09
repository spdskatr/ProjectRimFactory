using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ProjectSAL
{
    public partial class Building_Assembler
    {
        public static Thing CalculateDominantIngredient(RecipeDef currentRecipe, List<Thing> thingRecord)
        {
            var stuffs = thingRecord.Where(t => t.def.IsStuff);
            if (thingRecord.Count == 0)
            {
                if (currentRecipe.ingredients.Count > 0) Log.Warning("S.A.L.: Had no thingRecord of items being accepted, but crafting recipe has ingredients. Did you reload a save?");
                return ThingMaker.MakeThing(ThingDefOf.Steel);
            }
            if (currentRecipe.productHasIngredientStuff)
            {
                return stuffs.OrderByDescending(t => t.stackCount).First();
            }
            if (currentRecipe.products.Any(x => x.thingDef.MadeFromStuff) || stuffs.Any())
            {
                return stuffs.RandomElementByWeight(x => x.stackCount);
            }
            return ThingMaker.MakeThing(ThingDefOf.Steel);
        }

        public static void FindBillAndChangeRepeatCount(BillStack billStack, RecipeDef currentRecipe)
        {
            if (billStack != null && billStack.Bills != null)
            {
                var billrepeater = billStack.Bills.OfType<Bill_Production>().ToList().Find(b => b.ShouldDoNow() && b.recipe == currentRecipe);
                if (billrepeater != null && billrepeater.repeatMode == BillRepeatModeDefOf.RepeatCount)
                    billrepeater.repeatCount -= 1;
            }
        }
    }
}

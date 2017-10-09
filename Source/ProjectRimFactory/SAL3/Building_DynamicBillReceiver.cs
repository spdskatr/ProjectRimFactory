using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SAL3
{
    public class Building_DynamicBillReceiver : Building_WorkTable
    {
        public List<RecipeDef> allRecipesCachedPublic = new List<RecipeDef>();
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            // Reflection binds 2 references together... hopefully
            try
            {
                typeof(ThingDef).GetField("allRecipesCached", GenGeneric.BindingFlagsAll).SetValue(def, allRecipesCachedPublic);
                allRecipesCachedPublic.Add(RecipeDefOf.CookMealSimple);
                Log.Message(((List<RecipeDef>)typeof(ThingDef).GetField("allRecipesCached", GenGeneric.BindingFlagsAll).GetValue(def))[0].defName);
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }
    }
}

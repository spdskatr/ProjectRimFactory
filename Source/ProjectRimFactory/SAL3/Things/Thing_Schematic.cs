using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ProjectRimFactory.SAL3.Things
{
    public class Thing_Schematic : ThingWithComps
    {
        public RecipeDef recipe;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref recipe, "recipe");
        }
        public override string Label => string.Concat(base.Label, " (", recipe != null ? recipe.label : "no recipe", ")");
    }
}

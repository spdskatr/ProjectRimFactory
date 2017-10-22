using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ProjectRimFactory.SAL3.Things.Assemblers
{
    public class Building_SmartAssembler : Building_ProgrammableAssembler
    {
        public bool allowForbidden;
        public override bool AllowForbidden => allowForbidden;

        protected override float ProductionSpeedFactor => 1f;

        public override IEnumerable<RecipeDef> GetAllRecipes()
        {
            yield break;
        }
    }
}

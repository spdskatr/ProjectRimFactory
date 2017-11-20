using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ProjectRimFactory.SAL3.Things.Assemblers
{
    public class Building_SimpleAssembler : Building_ProgrammableAssembler
    {
        protected override float ProductionSpeedFactor => 1f;

        public override IEnumerable<RecipeDef> GetAllRecipes()
        {
            return from r in def.recipes
                   where r.AvailableNow
                   select r;
        }
    }
}

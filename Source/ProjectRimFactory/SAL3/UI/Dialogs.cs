using System;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.Sound;
using ProjectRimFactory.SAL3.Things;

namespace ProjectRimFactory.SAL3.UI
{
    public class Dialog_SmartHopperSetTargetAmount : Dialog_Rename
    {
        protected Building_SmartHopper smartHopper;
        public Dialog_SmartHopperSetTargetAmount(Building_SmartHopper building)
        {
            smartHopper = building;
        }
        protected override AcceptanceReport NameIsValid(string name)
        {
            return int.TryParse(name, out int i);
        }
        protected override void SetName(string name)
        {
            smartHopper.limit = int.Parse(name);
        }
    }
}

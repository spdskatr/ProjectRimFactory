using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace ProjectRimFactory.SAL3.Things.Assemblers.Special
{
    public class Building_PaperclipFactory : Building_SimpleAssembler
    {
        public int PaperclipsPerKilogram
        {
            get
            {
                return 250;
            }
        }

        protected override void PostProcessRecipeProduct(Thing thing)
        {
            int limit = thing.def.stackLimit;
            int paperclips = Mathf.RoundToInt(currentBillReport.selected.Sum(t => t.GetStatValue(StatDefOf.Mass) * PaperclipsPerKilogram));
            if (paperclips <= limit)
            {
                thing.stackCount = paperclips;
            }
            else
            {
                thing.stackCount = limit;
                paperclips -= limit;
                while (paperclips > 0)
                {
                    int count = Math.Min(paperclips, limit);
                    Thing newThing = ThingMaker.MakeThing(thing.def);
                    newThing.stackCount = count;
                    thingQueue.Add(newThing);
                    paperclips -= count;
                }
            }
        }
    }
}

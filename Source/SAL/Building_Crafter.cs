using System;
using System.Linq;

using RimWorld;
using Verse;

namespace ProjectSAL
{
    public class Building_Crafter : Building_Assembler
    {
        public override Building_WorkTable WorkTable => Map.thingGrid.ThingsListAt (WorkTableCell).OfType<Building_WorkTable> ()?.FirstOrDefault (t => t.InteractionCell == Position);
    }
}

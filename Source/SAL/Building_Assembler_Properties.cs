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
        public ModExtension_Assembler Extension => def.GetModExtension<ModExtension_Assembler>();

        public IntVec3 OutputSlot => GenAdj.CellsAdjacentCardinal (this).ElementAt (rotOutput);

        public int OutputSlots => GenAdj.CellsAdjacentCardinal (this).Count ();

        public List<Thing> NextItems
        {
            get
            {
                var query = (
                    from c in GenAdj.CellsAdjacent8Way (this)
                    from t in c.GetThingList (Map)
                    where t.def.category == ThingCategory.Item
                    select t
                );

                return query.ToList ();
            }
        }

        public IntVec3 WorkTableCell => Position + GenAdj.CardinalDirections[Rotation.AsInt];

        public virtual Building_WorkTable WorkTable => this;

        public new BillStack BillStack => WorkTable?.BillStack;

        protected bool OutputSlotOccupied => OutputSlot.GetFirstItem(Map) != null || OutputSlot.Impassable(Map);

        protected bool ShouldDoWork => currentRecipe != null && !ingredients.Any(ingredient => ingredient.count > 0) && ShouldDoWorkInCurrentTimeAssignment;

        protected bool ShouldStartBill => currentRecipe == null && BillStack != null && BillStack.AnyShouldDoNow;

        protected bool ShouldDoWorkInCurrentTimeAssignment => buildingPawn.timetable.times[GenLocalDate.HourOfDay(this)] != TimeAssignmentDefOf.Sleep;

        protected bool WorkDone => currentRecipe != null && ShouldDoWork && (int)workLeft == 0;

        protected SoundDef SoundOfCurrentRecipe => currentRecipe?.soundWorking;

        protected bool WorkTableisReservedByOther
        {
            get
            {
                if (WorkTable == this) return false;
                if (WorkTable == null) return false;
                var target = new LocalTargetInfo(WorkTable);
                return Map.reservationManager.IsReserved(target, Faction) && !Map.physicalInteractionReservationManager.IsReservedBy(buildingPawn, target) && !Map.reservationManager.ReservedBy(target, buildingPawn);
            }
        }

        /// <summary>
        /// If worktable is reserved by someone else, or dependent on power and has no power, return false
        /// </summary>
        protected bool WorkTableIsDisabled => WorkTable != null && (WorkTableisReservedByOther || WorkTableIsPoweredOff);

        /// <summary>
        /// If power is off, or broken down, then return true
        /// </summary>
        protected bool WorkTableIsPoweredOff => !(WorkTable.GetComp<CompPowerTrader>()?.PowerOn ?? true) || WorkTable.IsBrokenDown();

        /// <summary>
        /// If worktable has no bills that we should do now, return true
        /// </summary>
        protected bool WorkTableIsDormant => !(BillStack?.AnyShouldDoNow ?? false);
    }
}

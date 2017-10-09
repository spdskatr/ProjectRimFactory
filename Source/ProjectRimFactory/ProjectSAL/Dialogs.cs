using System;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.Sound;

namespace ProjectSAL
{
    public class PawnColumnWorker_TimeTableSimple : PawnColumnWorker_Timetable
    {
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn.timetable == null)
            {
                return;
            }
            float num = rect.x;
            float num2 = rect.width / 24f;
            for (int i = 0; i < 24; i++)
            {
                Rect rect2 = new Rect(num, rect.y, num2, rect.height);
                DoTimeAssignmentEdited(rect2, pawn, i);
                num += num2;
            }
            GUI.color = Color.white;
        }
        private void DoTimeAssignmentEdited(Rect rect, Pawn p, int hour)
        {
            rect = rect.ContractedBy(1f);
            TimeAssignmentDef assignment = p.timetable.GetAssignment(hour);
            GUI.DrawTexture(rect, assignment.ColorTexture);
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawBox(rect, 2);
                if (assignment != TimeAssignmentDefOf.Work && Input.GetMouseButton(0))
                {
                    SoundDefOf.DesignateDragStandardChanged.PlayOneShotOnCamera(null);
                    p.timetable.SetAssignment(hour, TimeAssignmentDefOf.Work);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeAssignments, KnowledgeAmount.SmallInteraction);
                }
                if (assignment != TimeAssignmentDefOf.Sleep && Input.GetMouseButton(1))
                {
                    SoundDefOf.DesignateDragStandardChanged.PlayOneShotOnCamera(null);
                    p.timetable.SetAssignment(hour, TimeAssignmentDefOf.Sleep);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeAssignments, KnowledgeAmount.SmallInteraction);
                }
            }
        }
    }
    public class Dialog_SALTimeTable : Window
    {
        public Pawn pawn;

        public Dialog_SALTimeTable() : base()
        {
            doCloseX = true;
            closeOnClickedOutside = true;
            closeOnEscapeKey = true;
        }

        public Dialog_SALTimeTable (Pawn pawn)
        {
            this.pawn = pawn;
        }

        public override Vector2 InitialSize => new Vector2(525f, 100f);

        public override void DoWindowContents(Rect inRect)
        {
            var rect1 = new Rect(inRect.x, inRect.y, inRect.width, 25f);
            var rect2 = inRect;
            rect2.yMin += 25;
            PawnColumnWorker_Timetable timetable = new PawnColumnWorker_TimeTableSimple();
            timetable.DoHeader(rect1, null);
            timetable.DoCell(rect2, pawn, null);
        }
    }

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

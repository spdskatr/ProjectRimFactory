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

    public class Dialog_SmartHopperMinMax : Window
    {
        protected Building_SmartHopper smartHopper;
        private const float TitleLabelHeight = 32f;
        private readonly Color TitleLineColor = new Color(0.3f, 0.3f, 0.3f);

        public Dialog_SmartHopperMinMax(Building_SmartHopper building)
        {
            smartHopper = building;
            doCloseX = true;
            closeOnEscapeKey = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
            draggable = true;
            drawShadow = true;
            focusWhenOpened = true;
            forcePause = true;
        }
        
        public override Vector2 InitialSize
        {
            get { return new Vector2(500f, 250f); }
        }

        public override void DoWindowContents(Rect rect)
        {
            //rect.height = 150f;
            Listing_Standard list = new Listing_Standard(GameFont.Small);
            list.ColumnWidth = rect.width;
            list.Begin(rect);
            var titleRect = new Rect(0f, 0f, rect.width, TitleLabelHeight);
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, "Set Min / Max for Hopper");
            Text.Font = GameFont.Small;
            list.Gap();
            list.Gap();
            list.Gap();
            list.CheckboxLabeled("Use Min?", ref smartHopper.useMin, "Do you want the min field to be in use?");
            list.Gap();
            {
                Rect rectLine = list.GetRect(Text.LineHeight);
                Rect rectLeft = rectLine.LeftHalf().Rounded();
                Rect rectRight = rectLine.RightHalf().Rounded();
                Widgets.DrawHighlightIfMouseover(rectLine);
                TooltipHandler.TipRegion(rectLine, "Minimum Stack to take");
                TextAnchor anchorBuffer = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rectLeft, "Minimum");
                Text.Anchor = anchorBuffer;
                Widgets.TextFieldNumeric(rectRight, ref smartHopper.min, ref smartHopper.minBufferString, 0);
            }
            list.Gap();
            list.CheckboxLabeled("Use Max?", ref smartHopper.useMax, "Do you want the max field to be in use?");
            list.Gap();
            {
                Rect rectLine = list.GetRect(Text.LineHeight);
                Rect rectLeft = rectLine.LeftHalf().Rounded();
                Rect rectRight = rectLine.RightHalf().Rounded();
                Widgets.DrawHighlightIfMouseover(rectLine);
                TooltipHandler.TipRegion(rectLine, "Maximum stack to take");
                TextAnchor anchorBuffer = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rectLeft, "Maximum");
                Text.Anchor = anchorBuffer;
                Widgets.TextFieldNumeric(rectRight, ref smartHopper.max, ref smartHopper.maxBufferString, 0);
            }
            list.End();
        }
    }
}

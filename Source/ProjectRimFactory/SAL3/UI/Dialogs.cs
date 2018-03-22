using System;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.Sound;
using ProjectRimFactory.SAL3.Things;

namespace ProjectRimFactory.SAL3.UI
{
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
            if (smartHopper.maxBufferString == null)
            {
                smartHopper.maxBufferString = smartHopper.max.ToString();
            }
            if (smartHopper.minBufferString == null)
            {
                smartHopper.minBufferString = smartHopper.min.ToString();
            }
            Listing_Standard list = new Listing_Standard(GameFont.Small);
            list.ColumnWidth = rect.width;
            list.Begin(rect);
            var titleRect = new Rect(0f, 0f, rect.width, TitleLabelHeight);
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, "SmartHopper_SetTargetAmount".Translate());
            Text.Font = GameFont.Small;
            list.Gap();
            list.Gap();
            list.Gap();
            list.CheckboxLabeled("SmartHopper_Minimum_Label".Translate(), ref smartHopper.useMin, "SmartHopper_Minimum_UseTooltip".Translate());
            list.Gap();
            {
                Rect rectLine = list.GetRect(Text.LineHeight);
                Rect rectLeft = rectLine.LeftHalf().Rounded();
                Rect rectRight = rectLine.RightHalf().Rounded();
                Widgets.DrawHighlightIfMouseover(rectLine);
                TooltipHandler.TipRegion(rectLine, "SmartHopper_Minimum_ToolTip".Translate());
                TextAnchor anchorBuffer = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rectLeft, "SmartHopper_Minimum_Keyword".Translate());
                Text.Anchor = anchorBuffer;
                Widgets.TextFieldNumeric(rectRight, ref smartHopper.min, ref smartHopper.minBufferString, 0);
            }
            list.Gap();
            list.CheckboxLabeled("SmartHopper_Maximum_Label".Translate(), ref smartHopper.useMax, "SmartHopper_Maximum_UseTooltip".Translate());
            list.Gap();
            {
                Rect rectLine = list.GetRect(Text.LineHeight);
                Rect rectLeft = rectLine.LeftHalf().Rounded();
                Rect rectRight = rectLine.RightHalf().Rounded();
                Widgets.DrawHighlightIfMouseover(rectLine);
                TooltipHandler.TipRegion(rectLine, "SmartHopper_Maximum_ToolTip".Translate());
                TextAnchor anchorBuffer = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rectLeft, "SmartHopper_MaximumKeyword".Translate());
                Text.Anchor = anchorBuffer;
                Widgets.TextFieldNumeric(rectRight, ref smartHopper.max, ref smartHopper.maxBufferString, 0);
            }
            list.End();
        }
    }
}

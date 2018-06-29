using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace ProjectRimFactory.Storage.UI
{
    public class ITab_Items : ITab
    {
        public ITab_Items()
        {
            size = new Vector2(480f, 480f);
            labelKey = "PRFItemsTab";
        }
        public Building_MassStorageUnit SelBuilding => (Building_MassStorageUnit)SelThing;
        protected override void FillTab()
        {
            Text.Font = GameFont.Small;
            Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
            IEnumerable<Thing> selected = from Thing t in SelBuilding.StoredItems
                                          where string.IsNullOrEmpty(searchQuery) || t.Label.ToLower().Contains(searchQuery.ToLower())
                                          select t;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 25f), SelBuilding.GetITabString(Math.Min(500, selected.Count())));
            searchQuery = Widgets.TextArea(new Rect(rect.x, rect.y + 25f, rect.width, 25f), searchQuery ?? string.Empty, false);
            Rect position = new Rect(rect);
            GUI.BeginGroup(position);
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, 60f, position.width, position.height - 60f);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, scrollViewHeight);
            Widgets.BeginScrollView(outRect, ref scrollPos, viewRect);
            float curY = 0;
            foreach (Thing thing in selected.Take(500))
            {
                DrawThingRow(ref curY, viewRect.width, thing);
            }
            if (Event.current.type == EventType.Layout)
            {
                scrollViewHeight = curY + 30f;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }
        private void DrawThingRow(ref float y, float width, Thing thing)
        {
            Rect rect = new Rect(0f, y, width, 28f);
            Widgets.InfoCardButton(rect.width - 24f, y, thing);
            rect.width -= 84f;
            if (Mouse.IsOver(rect))
            {
                GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
            if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
            {
                Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), thing, 1f);
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            Rect rect5 = new Rect(36f, y, rect.width - 36f, rect.height);
            Text.WordWrap = false;
            Widgets.Label(rect5, thing.LabelCap.Truncate(rect5.width, null));
            Text.WordWrap = true;
            string text2 = thing.LabelCap;
            if (thing.def.useHitPoints)
            {
                text2 = string.Concat(new object[]
                {
                    thing.LabelCap,
                    "\n",
                    thing.HitPoints,
                    " / ",
                    thing.MaxHitPoints
                });
            }
            TooltipHandler.TipRegion(rect, text2);
            if (GUI.Button(rect, "", Widgets.EmptyStyle))
            {
                Find.Selector.ClearSelection();
                Find.Selector.Select(thing);
            }
            y += 28f;
        }
        Vector2 scrollPos;
        float scrollViewHeight;
        string searchQuery;
    }
}

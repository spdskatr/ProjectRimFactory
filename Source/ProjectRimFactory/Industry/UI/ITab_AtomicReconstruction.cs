using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using ProjectRimFactory.Storage.UI;
using Verse.Sound;
using ProjectRimFactory.Common;

namespace ProjectRimFactory.Industry.UI
{
    public class ITab_AtomicReconstruction : ITab
    {
        public ITab_AtomicReconstruction()
        {
            size = new Vector2(400f, 400f);
            labelKey = "PRFAtomicReconstructionTab";
        }
        public Building_AtomicReconstructor SelBuilding => (Building_AtomicReconstructor)SelThing;
        protected override void FillTab()
        {
            Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);
            listing.Label(SelThing.LabelCapNoCount);
            listing.LabelDouble("AtomicReconstructionTab_NowProducing".Translate(), (SelBuilding.ThingToGenerate?.LabelCap ?? "NoneBrackets".Translate()));
            listing.LabelDouble("AtomicReconstructionTab_PaperclipCost".Translate(), (SelBuilding.ThingToGenerate?.PaperclipAmount() ?? 0f).ToStringDecimalIfSmall());
            listing.LabelDouble("AtomicReconstructionTab_ConsumptionPerSecond".Translate(), (SelBuilding.FuelConsumptionPerTick * 60f).ToStringDecimalIfSmall());
            listing.LabelDouble("AtomicReconstructionTab_Progress".Translate(), SelBuilding.ProgressToStringPercent);
            searchQuery = listing.TextEntry(searchQuery);
            Rect rect2 = new Rect(0, listing.CurHeight, rect.width, rect.height - listing.CurHeight);
            Rect viewRect = new Rect(0f, 0f, rect2.width - 16f, scrollViewHeight);
            Widgets.BeginScrollView(rect2, ref scrollPos, viewRect);
            float curY = 0;
            foreach (ThingDef tDef in AllAllowedThingDefsColonyCanProduce())
            {
                if (searchQuery == null || tDef.label.ToLower().Contains(searchQuery))
                {
                    try
                    {
                        DrawThingDefRow(ref curY, viewRect.width, tDef);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Project RimFactory :: Exception displaying row for {tDef}:{e}");
                    }
                }
            }
            if (Event.current.type == EventType.Layout)
            {
                scrollViewHeight = curY + 30f;
            }
            Widgets.EndScrollView();
            listing.End();
        }
        private void DrawThingDefRow(ref float y, float width, ThingDef thingDef)
        {
            Rect rect = new Rect(0f, y, width, 28f);
            if (Mouse.IsOver(rect))
            {
                GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
            if (thingDef.DrawMatSingle != null && thingDef.DrawMatSingle.mainTexture != null)
            {
                if (thingDef.graphicData != null && GenUI.IconDrawScale(thingDef) <= 1f)
                {
                    Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), thingDef);
                }
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            Rect rect5 = new Rect(36f, y, rect.width - 36f, rect.height);
            Text.WordWrap = false;
            Widgets.Label(rect5, thingDef.LabelCap.Truncate(rect5.width, null));
            Text.WordWrap = true;
            string text2 = thingDef.description;
            TooltipHandler.TipRegion(rect, string.IsNullOrEmpty(text2) ? "PRFNoDesc".Translate() : text2);
            if (GUI.Button(rect, "", Widgets.EmptyStyle))
            {
                SelBuilding.ThingToGenerate = thingDef;
                SoundDefOf.Click.PlayOneShot(SoundInfo.OnCamera());
            }
            Text.Anchor = TextAnchor.UpperLeft;
            y += 28f;
        }
        public static IEnumerable<ThingDef> AllAllowedThingDefsColonyCanProduce()
        {
            if (PRFDefOf.PRFAtomicReconstruction.IsFinished)
            {
                foreach (ThingDef tDef in ThingCategoryDefOf.ResourcesRaw.DescendantThingDefs)
                {
                    if (!tDef.MadeFromStuff)
                    {
                        if (!PRFDefOf.PRFNanoMaterials.IsFinished && (tDef == PRFDefOf.PRFXComposite || tDef == PRFDefOf.PRFYComposite))
                        {
                            continue;
                        }
                        yield return tDef;
                    }
                }
            }
            if (PRFDefOf.PRFEdiblesSynthesis.IsFinished)
            {
                foreach (ThingDef tDef in ThingCategoryDefOf.Foods.DescendantThingDefs)
                {
                    if (!tDef.MadeFromStuff)
                        yield return tDef;
                }
            }
            if (PRFDefOf.PRFManufacturablesProduction.IsFinished)
            {
                foreach (ThingDef tDef in ThingCategoryDefOf.Manufactured.DescendantThingDefs)
                {
                    if (!tDef.MadeFromStuff)
                        yield return tDef;
                }
            }
        }
        Vector2 scrollPos;
        float scrollViewHeight;
        string searchQuery;
    }
}

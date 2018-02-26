using ProjectRimFactory.Storage.Editables;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ProjectRimFactory.Storage.UI
{
    [StaticConstructorOnStartup]
    public class ITab_StorageUnitIOPort : ITab
    {
        static List<ThingDef> itemList;
        static ITab_StorageUnitIOPort()
        {
            LongEventHandler.QueueLongEvent(InitItemList, "Initializing", true, e => Log.Error($"Project RimFactory: Exception initializing items list: {e}"));
        }
        private static void InitItemList()
        {
            itemList = new List<ThingDef>(from ThingDef tDef in DefDatabase<ThingDef>.AllDefs
                                          where tDef.category == ThingCategory.Item
                                          orderby tDef.LabelCap
                                          select tDef);
        }
        public ITab_StorageUnitIOPort()
        {
            size = new Vector2(400f, 400f);
            labelKey = "PRFStorageUnitIOTab";
        }
        public Building_StorageUnitIOPort SelBuilding => (Building_StorageUnitIOPort)SelThing;
        protected override void FillTab()
        {
            Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);
            listing.Label(SelBuilding.LabelCap);
            if (listing.ButtonTextLabeled("PRFIOMode".Translate(), SelBuilding.IOMode.ToString()))
            {
                Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>()
                {
                    new FloatMenuOption(StorageIOMode.Input.ToString(), () => SelBuilding.IOMode = StorageIOMode.Input),
                    new FloatMenuOption(StorageIOMode.Output.ToString(), () => SelBuilding.IOMode = StorageIOMode.Output)
                }));
            }
            if (listing.ButtonTextLabeled("PRFBoundStorageBuilding".Translate(), SelBuilding.BoundStorageUnit?.LabelCap ?? "NoneBrackets".Translate()))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>(
                    from Building_MassStorageUnit b in Find.VisibleMap.listerBuildings.AllBuildingsColonistOfClass<Building_MassStorageUnit>()
                    where b.def.GetModExtension<DefModExtension_CanUseStorageIOPorts>() != null
                    select new FloatMenuOption(b.LabelCap, () => SelBuilding.BoundStorageUnit = b)
                );
                if (list.Count == 0)
                {
                    list.Add(new FloatMenuOption("NoneBrackets".Translate(), null));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            listing.Label("PRFOutputItem".Translate((SelBuilding.BoundThingDef?.LabelCap ?? "NoneBrackets".Translate())));
            searchQuery = listing.TextEntry(searchQuery);
            Rect rect2 = new Rect(0, listing.CurHeight, rect.width, rect.height - listing.CurHeight);
            Rect viewRect = new Rect(0f, 0f, rect2.width - 16f, scrollViewHeight);
            Widgets.BeginScrollView(rect2, ref scrollPos, viewRect);
            float curY = 0;
            for (int i = 0; i < itemList.Count; i++)
            {
                if (searchQuery == null || itemList[i].label.ToLower().Contains(searchQuery))
                {
                    try
                    {
                        DrawThingDefRow(ref curY, viewRect.width, itemList[i]);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Project RimFactory :: Exception displaying row for {itemList[i]}:{e}");
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
                SelBuilding.BoundThingDef = thingDef;
                SoundDefOf.Click.PlayOneShot(SoundInfo.OnCamera());
            }
            Text.Anchor = TextAnchor.UpperLeft;
            y += 28f;
        }
        Vector2 scrollPos;
        float scrollViewHeight;
        string searchQuery;
    }
}

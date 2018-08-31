using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI;

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
                if (Event.current.button == 1)
                {
                    List<FloatMenuOption> opts = new List<FloatMenuOption>()
                    {
                        new FloatMenuOption("PRFMassStorageRightClickSelectPawn".Translate(), null) { Disabled = true }
                    };
                    foreach (Pawn p in from Pawn col in thing.Map.mapPawns.FreeColonists
                                       where col.IsColonistPlayerControlled && !col.Dead && col.Spawned && !col.Downed
                                       select col)
                    {
                        opts.Add(new FloatMenuOption(p.Name.ToStringShort, () =>
                        {
                            Find.WindowStack.Add(new FloatMenu(ChoicesForThing(thing, p)));
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(opts));
                }
                else
                {
                    Find.Selector.ClearSelection();
                    Find.Selector.Select(thing);
                }
            }
            y += 28f;
        }
        public static List<FloatMenuOption> ChoicesForThing(Thing thing, Pawn selPawn)
        {
            List<FloatMenuOption> opts = new List<FloatMenuOption>();
            Thing t = thing;

            // Copied from FloatMenuMakerMap.AddHumanlikeOrders
            if (t.def.ingestible != null && selPawn.RaceProps.CanEverEat(t) && t.IngestibleNow)
            {
                string text;
                if (t.def.ingestible.ingestCommandString.NullOrEmpty())
                {
                    text = "ConsumeThing".Translate(new object[]
                    {
                        t.LabelShort
                    });
                }
                else
                {
                    text = string.Format(t.def.ingestible.ingestCommandString, t.LabelShort);
                }
                if (!t.IsSociallyProper(selPawn))
                {
                    text = text + " (" + "ReservedForPrisoners".Translate() + ")";
                }
                FloatMenuOption item7;
                if (t.def.IsNonMedicalDrug && selPawn.IsTeetotaler())
                {
                    item7 = new FloatMenuOption(text + " (" + TraitDefOf.DrugDesire.DataAtDegree(-1).label + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                }
                else if (!selPawn.CanReach(t, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
                {
                    item7 = new FloatMenuOption(text + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                }
                else
                {
                    MenuOptionPriority priority2 = (!(t is Corpse)) ? MenuOptionPriority.Default : MenuOptionPriority.Low;
                    item7 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, delegate ()
                    {
                        t.SetForbidden(false, true);
                        Job job = new Job(JobDefOf.Ingest, t);
                        job.count = FoodUtility.WillIngestStackCountOf(selPawn, t.def, t.GetStatValue(StatDefOf.Nutrition, true));
                        selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }, priority2, null, null, 0f, null, null), selPawn, t, "ReservedBy");
                }
                opts.Add(item7);
            }

            // Add equipment commands
            // Copied from FloatMenuMakerMap.AddHumanlikeOrders
            if (thing is ThingWithComps equipment && equipment.GetComp<CompEquippable>() != null)
            {
                string labelShort = equipment.LabelShort;
                FloatMenuOption item4;
                if (equipment.def.IsWeapon && selPawn.story.WorkTagIsDisabled(WorkTags.Violent))
                {
                    item4 = new FloatMenuOption("CannotEquip".Translate(new object[]
                    {
                            labelShort
                    }) + " (" + "IsIncapableOfViolenceLower".Translate(new object[]
                    {
                            selPawn.LabelShort
                    }) + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                }
                else if (!selPawn.CanReach(equipment, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
                {
                    item4 = new FloatMenuOption("CannotEquip".Translate(new object[]
                    {
                            labelShort
                    }) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                }
                else if (!selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                {
                    item4 = new FloatMenuOption("CannotEquip".Translate(new object[]
                    {
                            labelShort
                    }) + " (" + "Incapable".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                }
                else
                {
                    string text5 = "Equip".Translate(new object[]
                    {
                            labelShort
                    });
                    if (equipment.def.IsRangedWeapon && selPawn.story != null && selPawn.story.traits.HasTrait(TraitDefOf.Brawler))
                    {
                        text5 = text5 + " " + "EquipWarningBrawler".Translate();
                    }
                    item4 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text5, delegate ()
                    {
                        equipment.SetForbidden(false, true);
                        selPawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.Equip, equipment), JobTag.Misc);
                        MoteMaker.MakeStaticMote(equipment.DrawPos, equipment.Map, ThingDefOf.Mote_FeedbackEquip, 1f);
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.EquippingWeapons, KnowledgeAmount.Total);
                    }, MenuOptionPriority.High, null, null, 0f, null, null), selPawn, equipment, "ReservedBy");
                }
                opts.Add(item4);
            }

            if (opts.Count == 0)
            {
                opts.Add(new FloatMenuOption("NoneBrackets".Translate(), null) { Disabled = true });
            }
            return opts;
        }
        Vector2 scrollPos;
        float scrollViewHeight;
        string searchQuery;
    }
}

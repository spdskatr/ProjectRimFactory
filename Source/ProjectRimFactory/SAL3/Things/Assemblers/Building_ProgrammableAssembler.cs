using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;
using ProjectRimFactory.SAL3.Tools;
using UnityEngine;

namespace ProjectRimFactory.SAL3.Things.Assemblers
{
    public abstract class Building_ProgrammableAssembler : Building_DynamicBillGiver
    {
        protected class BillReport : IExposable
        {
            public BillReport()
            {
            }
            public BillReport(Bill b, List<Thing> list)
            {
                bill = b;
                selected = list;
                workLeft = b.recipe.WorkAmountTotal(ProjectSAL_Utilities.CalculateDominantIngredient(b.recipe, list).def);
            }
            public Bill bill;
            public List<Thing> selected;
            public float workLeft;

            public void ExposeData()
            {
                Scribe_References.Look(ref bill, "bill");
                Scribe_Collections.Look(ref selected, "selected", LookMode.Deep);
                Scribe_Values.Look(ref workLeft, "workLeft");
            }
        }

        // Pawn

        public Pawn buildingPawn;
        
        public virtual void DoPawn()
        {
            Pawn p = PawnGenerator.GeneratePawn(PawnKindDefOf.Slave, Faction);
            p.Name = new NameTriple(LabelCap, "SAL_Name".Translate(), GetUniqueLoadID());
            //Assign skills
            foreach (var s in p.skills.skills)
            {
                int level = 10; // Skill level
                s.levelInt = level;
            }
            //Assign Pawn's mapIndexOrState to building's mapIndexOrState
            ReflectionUtility.mapIndexOrState.SetValue(p, ReflectionUtility.mapIndexOrState.GetValue(this));
            //Assign Pawn's position without nasty errors
            p.SetPositionDirect(Position);
            //Clear pawn relations
            p.relations.ClearAllRelations();
            //Set backstories
            SetBackstoryAndSkills(p);
            //Pawn work-related stuffs
            for (int i = 0; i < 24; i++)
            {
                p.timetable.SetAssignment(i, TimeAssignmentDefOf.Work);
            }

            buildingPawn = p;
        }

        private static void SetBackstoryAndSkills(Pawn p)
        {
            if (BackstoryDatabase.TryGetWithIdentifier("ChildSpy95", out Backstory bs))
            {
                p.story.childhood = bs;
            }
            else
            {
                Log.Error("Tried to assign child backstory ChildSpy95, but not found");
            }
            if (BackstoryDatabase.TryGetWithIdentifier("ColonySettler43", out Backstory bstory))
            {
                p.story.adulthood = bstory;
            }
            else
            {
                Log.Error("Tried to assign child backstory ColonySettler43, but not found");
            }
            //Clear traits
            p.story.traits.allTraits = new List<Trait>();
            //Reset cache
            ReflectionUtility.cachedDisabledWorkTypes.SetValue(p.story, null);
            //Reset cache for each skill
            for (int i = 0; i < p.skills.skills.Count; i++)
            {
                ReflectionUtility.cachedTotallyDisabled.SetValue(p.skills.skills[i], BoolUnknown.Unknown);
            }
        }

        // Misc
        public BillStack billStack;
        public override BillStack BillStack => billStack;
        public Building_ProgrammableAssembler()
        {
            billStack = new BillStack(this);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref billStack, "bills", this);
            Scribe_Deep.Look(ref currentBillReport, "currentBillReport");
            Scribe_Collections.Look(ref thingQueue, "thingQueue", LookMode.Deep);
            Scribe_Values.Look(ref allowForbidden, "allowForbidden");
            Scribe_Values.Look(ref outputSlotIndex, "outputSlotIndex");
            Scribe_Deep.Look(ref buildingPawn, "buildingPawn");
            if (buildingPawn == null)
                DoPawn();
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            yield return new Command_Action()
            {
                defaultLabel = "AdjustDirection_Output".Translate(),
                action = () => outputSlotIndex++,
                icon = TexUI.RotRightTex,
                defaultIconColor = Color.green
            };
            yield return new Command_Toggle()
            {
                defaultLabel = "SALToggleForbidden".Translate(),
                defaultDesc = "SALToggleForbidden_Desc".Translate(),
                isActive = () => allowForbidden,
                toggleAction = () => allowForbidden ^= true,
                icon = TexCommand.Forbidden,
            };
            if (Prefs.DevMode)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "DEBUG: Debug actions",
                    action = () => Find.WindowStack.Add(new FloatMenu(GetDebugOptions().ToList()))
                };
            }
        }
        protected virtual IEnumerable<FloatMenuOption> GetDebugOptions()
        {
            string StringConverter(Thing t)
            {
                return t.GetUniqueLoadID();
            }
            yield return new FloatMenuOption("View selected things", () => {
                if (currentBillReport != null)
                {
                    Log.Message("Selected things: " + string.Join(", ", currentBillReport.selected.Select(StringConverter).ToArray()));
                }
            });
            yield return new FloatMenuOption("View all items available for input", () =>
            {
                Log.Message(string.Join(", ", AllAccessibleThings.Select(StringConverter).ToArray()));
            });
            yield break;
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (buildingPawn == null)
                DoPawn();

            //Assign Pawn's mapIndexOrState to building's mapIndexOrState
            ReflectionUtility.mapIndexOrState.SetValue(buildingPawn, ReflectionUtility.mapIndexOrState.GetValue(this));
            //Assign Pawn's position without nasty errors
            buildingPawn.SetPositionDirect(Position);
        }

        // Logic
        protected BillReport currentBillReport;

        // thingQueue is List to save properly
        protected List<Thing> thingQueue = new List<Thing>();

        protected virtual bool Active => GetComp<CompPowerTrader>()?.PowerOn ?? true;

        protected IEnumerable<Thing> AllAccessibleThings => from c in IngredientStackCells
                                                            from t in Map.thingGrid.ThingsListAt(c)
                                                            where AllowForbidden || !t.IsForbidden(Faction)
                                                            select t;
        protected IEnumerable<Bill> AllBillsShouldDoNow => from b in billStack.Bills
                                                           where b.ShouldDoNow()
                                                           select b;
        public override void Tick()
        {
            base.Tick();
            if (this.IsHashIntervalTick(10) && Active)
            {
                if (thingQueue.Count > 0 && OutputSlot.Walkable(Map) && 
                    (OutputSlot.GetFirstItem(Map)?.TryAbsorbStack(thingQueue[0], true) ?? GenPlace.TryPlaceThing(thingQueue[0], OutputSlot, Map, ThingPlaceMode.Direct)))
                {
                    thingQueue.RemoveAt(0);
                }
                if (currentBillReport != null)
                {
                    currentBillReport.workLeft -= 10f * ProductionSpeedFactor;
                    if (currentBillReport.workLeft <= 0)
                    {
                        ProduceItems();
                        currentBillReport.bill.Notify_IterationCompleted(buildingPawn, currentBillReport.selected);
                        Notify_RecipeCompleted(currentBillReport.bill.recipe);
                        currentBillReport = null;
                    }
                }
                else if (this.IsHashIntervalTick(60))
                    currentBillReport = CheckBills();
            }
        }

        protected virtual BillReport CheckBills()
        {
            foreach (Bill b in AllBillsShouldDoNow)
            {
                List<ThingAmount> chosen = new List<ThingAmount>();
                if (TryFindBestBillIngredientsInSet(AllAccessibleThings.ToList(), b, chosen))
                {
                    return new BillReport(b, (from ta in chosen select ta.thing.SplitOff(ta.count)).ToList());
                }
            }
            return null;
        }

        bool TryFindBestBillIngredientsInSet(List<Thing> accessibleThings, Bill b, List<ThingAmount> chosen)
        {
            ReflectionUtility.MakeIngredientsListInProcessingOrder.Invoke(null, new object[] { ReflectionUtility.ingredientsOrdered.GetValue(null), b });
            return (bool)ReflectionUtility.TryFindBestBillIngredientsInSet.Invoke(null, new object[] { accessibleThings, b, chosen });
        }

        protected virtual void ProduceItems()
        {
            if (currentBillReport == null)
            {
                Log.Error("S.A.L. 3.0 :: Tried to make products when assembler isn't engaged in a bill.");
                return;
            }
            IEnumerable<Thing> products = GenRecipe.MakeRecipeProducts(currentBillReport.bill.recipe, buildingPawn, currentBillReport.selected, ProjectSAL_Utilities.CalculateDominantIngredient(currentBillReport.bill.recipe, currentBillReport.selected));
            foreach (Thing thing in products)
            {
                PostProcessRecipeProduct(thing);
                thingQueue.Add(thing);
            }
            for (int i = 0; i < currentBillReport.selected.Count; i++)
            {
                if (currentBillReport.selected[i] is Corpse c)
                {
                    List<Apparel> apparel = new List<Apparel>(c.InnerPawn.apparel.WornApparel);
                    for (int j = 0; j < apparel.Count; j++)
                    {
                        thingQueue.Add(apparel[j]);
                        c.InnerPawn.apparel.Remove(apparel[j]);
                    }
                }
                currentBillReport.bill.recipe.Worker.ConsumeIngredient(currentBillReport.selected[i], currentBillReport.bill.recipe, Map);
            }
            thingQueue.AddRange(from Thing t in currentBillReport.selected where t.Spawned select t);
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.GetInspectString());
            if (currentBillReport == null)
            {
                stringBuilder.AppendLine("SearchingForIngredients".Translate());
            }
            else
            {
                stringBuilder.AppendLine("SAL3_BillReport".Translate(currentBillReport.bill.Label.ToString(), currentBillReport.workLeft.ToStringWorkAmount()));
            }
            stringBuilder.AppendLine("SAL3_Products".Translate(thingQueue.Count));
            return stringBuilder.ToString().TrimEndNewlines();
        }

        // Settings
        public int outputSlotIndex;
        public bool allowForbidden;
        public virtual bool AllowForbidden => allowForbidden;
        protected virtual IntVec3 OutputSlot
        {
            get
            {
                List<IntVec3> cells = GenAdj.CellsAdjacentCardinal(this).ToList();
                return cells[outputSlotIndex %= cells.Count];
            }
        }
        protected abstract float ProductionSpeedFactor { get; }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            GenDraw.DrawFieldEdges(IngredientStackCells.ToList());
            GenDraw.DrawFieldEdges(new List<IntVec3>() { OutputSlot }, Color.yellow);
        }
        public override void DrawGUIOverlay()
        {
            base.DrawGUIOverlay();
            if (Find.CameraDriver.CurrentZoom < CameraZoomRange.Middle)
            {
                GenMapUI.DrawThingLabel(GenMapUI.LabelDrawPosFor(this, 0f), currentBillReport == null ? "AssemblerIdle".Translate() : currentBillReport.bill.LabelCap, Color.white);
            }
        }

        // Other virtual methods
        protected virtual void Notify_RecipeCompleted(RecipeDef recipe)
        {
        }

        protected virtual void PostProcessRecipeProduct(Thing thing)
        {
        }
    }
}

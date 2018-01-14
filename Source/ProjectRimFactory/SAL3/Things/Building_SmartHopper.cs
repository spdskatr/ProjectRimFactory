using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using ProjectRimFactory.SAL3.UI;
using ProjectRimFactory.SAL3.Tools;

namespace ProjectRimFactory.SAL3.Things
{
    public class Building_SmartHopper : Building, IStoreSettingsParent
    {
        public int limit = 75;
        
        public IEnumerable<IntVec3> cachedDetectorCells;

        protected virtual bool ShouldRespectStackLimit => true;

        public StorageSettings settings;

        public Thing StoredThing => Position.GetFirstItem(Map);

        public IEnumerable<IntVec3> CellsToSelect
        {
            get
            {
                if (Find.TickManager.TicksGame % 50 != 0 && cachedDetectorCells != null)
                {
                    return cachedDetectorCells;
                }

                var resultCache = from IntVec3 c in GenRadial.RadialCellsAround(Position, def.specialDisplayRadius, false)
                                  where c.HasSlotGroupParent(Map)
                                  select c;
                cachedDetectorCells = resultCache;
                return resultCache;
            }
        }

        public IEnumerable<Thing> ThingsToSelect
        {
            get
            {
                foreach (var c in CellsToSelect)
                {
                    foreach (var t in Map.thingGrid.ThingsListAt(c))
                    {
                        yield return t;
                    }
                }
            }
        }

        public bool StorageTabVisible => true;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (settings == null)
            {
                settings = new StorageSettings();
                settings.CopyFrom(GetParentStoreSettings());
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref limit, "limit", 75);
            Scribe_Deep.Look(ref settings, "settings", this);
        }

        public override string GetInspectString() => base.GetInspectString() + "\n" + "SmartHopper_Limit".Translate(limit);

        public override void Tick()
        {
            base.Tick();
            if (GetComp<CompPowerTrader>().PowerOn && Find.TickManager.TicksGame % 35 == 0)
            {
                foreach (var element in ThingsToSelect)
                {
                    if (element.def.category == ThingCategory.Item && settings.AllowedToAccept(element))
                    {
                        TryStoreThing(element);
                        break;
                    }
                }
                if (StoredThing != null)
                {
                    if (settings.AllowedToAccept(StoredThing))
                        StoredThing.SetForbidden(true, false);
                    else
                        StoredThing.SetForbidden(false, false);
                }
            }
        }

        public virtual void TryStoreThing(Thing element)
        {
            if (StoredThing != null)
            {
                if (StoredThing.CanStackWith(element))
                {
                    var num = Mathf.Min(element.stackCount, Mathf.Min(limit, StoredThing.def.stackLimit) - StoredThing.stackCount);
                    if (num > 0)
                    {
                        var t = element.SplitOff(num);
                        StoredThing.TryAbsorbStack(t, true);
                    }
                }
            }
            else
            {
                var num = Mathf.Min(element.stackCount, limit);
                if (num == element.stackCount)
                {
                    element.Position = Position;
                }
                else if (num > 0)
                {
                    var t = element.SplitOff(num);
                    GenPlace.TryPlaceThing(t, Position, Map, ThingPlaceMode.Direct);
                }
            }
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            GenDraw.DrawFieldEdges(CellsToSelect.ToList(), Color.green);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
                yield return g;
            foreach (Gizmo g2 in StorageSettingsClipboard.CopyPasteGizmosFor(settings))
                yield return g2;
            yield return new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("UI/Commands/SetTargetFuelLevel"),
                defaultLabel = "SmartHopper_SetTargetAmount".Translate(),
                action = () => Find.WindowStack.Add(new Dialog_SmartHopperSetTargetAmount(this)),
            };
        }

        public StorageSettings GetStoreSettings()
        {
            if (settings == null)
            {
                settings = new StorageSettings();
                settings.CopyFrom(GetParentStoreSettings());
            }
            return settings;
        }

        public StorageSettings GetParentStoreSettings() => def.building.fixedStorageSettings;
    }
}

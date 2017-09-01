using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

//S.A.L. | Station Automation and Logistics
/* To-do: 
 *                                                    Done?
 * Reserve workbench----------------------------------DONE
 * Resolve pawn error on map load---------------------DONE
 * Check if workbench has power-----------------------DONE
 * Take bill off bill stack once done-----------------DONE
 * change colour of ouput direction in Draw()---------DONE
 * Make them be bad at art----------------------------DONE
 * Allow/disallow taking forbidden items--------------DONE
 * Make sound while crafting--------------------------DONE
 * Make sound when sucking item in--------------------DONE
 * Check if work table is deconstructed---------------DONE
 * Clear reservation if power off---------------------DONE
 * Customisable pawns via defs; skills----------------DONE
 * Sort out problem with nutrition/cooking------------DONE
 * Eject items in thingRecord if deconstructed--------DONE
 * Edit defs: Not forbiddable, flickable--------------DONE
 * Make smart hopper----------------------------------DONE
 * Check for small volume-----------------------------DONE
 * Redo corpse calculations---------------------------DONE
 * Add ingredients to unfinished items----------------DONE
 * Patch for Mending
 * Maintenance intervals------------------------------DONE
 * Move AssemblerDef to a ModExtension----------------DONE
 * Tiered crafters------------------------------------DONE
 *From xlilcasper (Ludeon Forums)
 * Let items get accepted from adjacent cells---------DONE
 * Check if colony has enough resources for billC:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\[SS] ProjectSAL\Source\Building_Crafter.cs
 *From Kadan Joelavich (Steam)
 * "This may not be possible, but would there 
 * be any way to have their global work speed 
 * factor in the material they are make from?---------DONE
 * Rework smart hopper
 * */
namespace ProjectSAL
{
    public class Building_SmartHopper : Building, IStoreSettingsParent
    {
        public int limit = 75;

        [Unsaved]
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

                var resultCache = from IntVec3 c in (GenRadial.RadialCellsAround(Position, def.specialDisplayRadius, false) ?? new List<IntVec3>()) where c.GetZone(Map) != null && c.GetZone(Map) is Zone_Stockpile select c;
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

        public override string GetInspectString() => base.GetInspectString() + "SmartHopper_Limit".Translate(limit);

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
                    //Verse.ThingUtility.TryAsborbStackNumToTake <- That's not a typo, that's an actual method. Seriously. Good thing it's fixed in A17, eh? (LudeonLeaks, 2017)
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

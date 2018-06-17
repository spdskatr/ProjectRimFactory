using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ProjectRimFactory.Storage
{
    public class Building_StorageUnitIOPort : Building_Storage
    {
        StorageIOMode mode = StorageIOMode.Input;
        Building_MassStorageUnit boundStorageUnit;
        ThingDef boundThingDef;

        CompPowerTrader powerComp;

        public StorageIOMode IOMode
        {
            get
            {
                return mode;
            }
            set
            {
                if (mode == value) return;
                mode = value;
                Notify_NeedRefresh();
            }
        }

        public Building_MassStorageUnit BoundStorageUnit
        {
            get
            {
                return boundStorageUnit;
            }
            set
            {
                boundStorageUnit?.DeregisterPort(this);
                boundStorageUnit = value;
                value?.RegisterPort(this);
                Notify_NeedRefresh();
            }
        }

        public ThingDef BoundThingDef
        {
            get
            {
                return boundThingDef;
            }
            set
            {
                boundThingDef = value;
                RefreshStoreSettings();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref mode, "mode");
            Scribe_References.Look(ref boundStorageUnit, "boundStorageUnit");
            Scribe_Defs.Look(ref boundThingDef, "boundThingDef");
        }

        public override void PostMake()
        {
            base.PostMake();
            powerComp = GetComp<CompPowerTrader>();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            Notify_NeedRefresh();
        }

        protected override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            if (signal == CompPowerTrader.PowerTurnedOnSignal)
            {
                Notify_NeedRefresh();
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn();
            boundStorageUnit?.DeregisterPort(this);
        }

        public void Notify_NeedRefresh()
        {
            switch (IOMode)
            {
                case StorageIOMode.Input:
                    RefreshInput();
                    break;
                case StorageIOMode.Output:
                    RefreshOutput();
                    break;
            }
            RefreshStoreSettings();
        }

        public override void Notify_ReceivedThing(Thing newItem)
        {
            base.Notify_ReceivedThing(newItem);
            RefreshInput();
        }

        public override void Notify_LostThing(Thing newItem)
        {
            base.Notify_LostThing(newItem);
            if (mode == StorageIOMode.Output && boundThingDef != null)
            {
                RefreshOutput();
            }
        }


        public override void Tick()
        {
            base.Tick();
            if (this.IsHashIntervalTick(10))
            {
                if (mode == StorageIOMode.Output && boundThingDef != null)
                {
                    RefreshOutput();
                }
                RefreshStoreSettings();
            }
        }

        public void RefreshStoreSettings()
        {
            if (mode == StorageIOMode.Output)
            {
                settings = new StorageSettings(this);
                if (boundStorageUnit != null)
                {
                    settings.Priority = boundStorageUnit.settings.Priority;
                }
                if (boundThingDef != null)
                {
                    settings.filter.SetAllow(boundThingDef, true);
                }
            }
            else if (boundStorageUnit != null)
            {
                settings = boundStorageUnit.settings;
            }
            else
            {
                settings = new StorageSettings(this);
            }
        }

        public void RefreshInput()
        {
            if (powerComp.PowerOn)
            {
                Thing item = Position.GetFirstItem(Map);
                if (mode == StorageIOMode.Input && item != null && boundStorageUnit != null && boundStorageUnit.settings.AllowedToAccept(item) && boundStorageUnit.CanReceiveIO)
                {
                    foreach (IntVec3 cell in boundStorageUnit.AllSlotCells())
                    {
                        if (cell.GetFirstItem(Map) == null)
                        {
                            boundStorageUnit.RegisterNewItem(item);
                            if (item.def.drawGUIOverlay)
                            {
                                Map.listerThings.ThingsInGroup(ThingRequestGroup.HasGUIOverlay).Remove(item);
                            }
                            break;
                        }
                    }
                }
            }
        }

        protected void RefreshOutput()
        {
            if (powerComp.PowerOn)
            {
                Thing currentItem = Position.GetFirstItem(Map);
                bool storageSlotAvailable = (currentItem == null || (currentItem.def == boundThingDef && currentItem.stackCount < currentItem.def.stackLimit));
                if (boundStorageUnit != null && boundStorageUnit.CanReceiveIO && storageSlotAvailable)
                {
                    foreach (Thing item in boundStorageUnit.StoredItems.ToList()) // ToList very important - evaluates enumerable
                    {
                        if (item.def == boundThingDef)
                        {
                            if (currentItem != null && currentItem.CanStackWith(item))
                            {
                                currentItem.TryAbsorbStack(item, true);
                            }
                            else
                            {
                                item.Position = Position;
                                currentItem = item;
                            }
                            if (currentItem != null && currentItem.stackCount >= currentItem.def.stackLimit)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}

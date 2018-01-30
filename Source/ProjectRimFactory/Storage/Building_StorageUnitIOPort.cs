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
        public ThingDef boundThingDef;

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
                switch (value)
                {
                    case StorageIOMode.Input:
                        RefreshInput();
                        if (boundStorageUnit != null)
                        {
                            settings = boundStorageUnit.settings;
                        }
                        break;
                    case StorageIOMode.Output:
                        RefreshOutput();
                        break;
                }
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
                boundStorageUnit = value;
                switch (IOMode)
                {
                    case StorageIOMode.Input:
                        RefreshInput();
                        if (boundStorageUnit != null)
                        {
                            settings = boundStorageUnit.settings;
                        }
                        break;
                    case StorageIOMode.Output:
                        RefreshOutput();
                        break;
                }
            }
        }

        public override void Notify_ReceivedThing(Thing newItem)
        {
            base.Notify_ReceivedThing(newItem);
            RefreshInput();
        }
        public override void Notify_LostThing(Thing newItem)
        {
            base.Notify_LostThing(newItem);
            Thing currentItem = Position.GetFirstItem(Map);
            if (mode == StorageIOMode.Output && currentItem == null && boundStorageUnit != null && boundThingDef != null)
            {
                if (currentItem == null || (currentItem.def == boundThingDef && currentItem.stackCount < currentItem.def.stackLimit))
                {
                    RefreshOutput();
                }
            }
        }


        public override void Tick()
        {
            base.Tick();
            if (mode == StorageIOMode.Output && this.IsHashIntervalTick(10) && boundStorageUnit != null && boundThingDef != null)
            {
                Thing currentItem = Position.GetFirstItem(Map);
                if (currentItem == null || (currentItem.def == boundThingDef && currentItem.stackCount < currentItem.def.stackLimit))
                {
                    RefreshOutput();
                }
            }
        }

        public void RefreshInput()
        {
            Thing item = Position.GetFirstItem(Map);
            if (mode == StorageIOMode.Input && item != null && boundStorageUnit != null && boundStorageUnit.settings.AllowedToAccept(item))
            {
                foreach (IntVec3 cell in AllSlotCells())
                {
                    if (cell.GetFirstItem(Map) == null)
                    {
                        item.Position = cell;
                        break;
                    }
                }
            }
        }

        protected void RefreshOutput()
        {
            if (boundStorageUnit != null)
            {
                Thing currentItem = Position.GetFirstItem(Map);
                foreach (Thing item in boundStorageUnit.StoredItems)
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

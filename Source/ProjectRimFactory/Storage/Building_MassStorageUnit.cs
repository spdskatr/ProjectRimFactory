using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using ProjectRimFactory.Storage.Editables;

namespace ProjectRimFactory.Storage
{
    public class Building_MassStorageUnit : Building_Storage
    {
        int totalStoredItems;
        public virtual bool CanStoreMoreItems => Position.GetThingList(Map).Count(t => t.def.category == ThingCategory.Item) < (Extension.limit - def.Size.Area + 1);
        public DefModExtension_MassStorage Extension => def.GetModExtension<DefModExtension_MassStorage>();

        public override void Notify_ReceivedThing(Thing newItem)
        {
            base.Notify_ReceivedThing(newItem);
            if (newItem.Position != Position)
            {
                RegisterNewItem(newItem);
            }
            totalStoredItems++;
        }

        protected virtual void RegisterNewItem(Thing newItem)
        {
            List<Thing> things = Position.GetThingList(Map);
            for (int i = 0; i < things.Count; i++)
            {
                Thing item = things[i];
                if (item.def.category == ThingCategory.Item && item.CanStackWith(newItem))
                {
                    item.TryAbsorbStack(newItem, true);
                }
                if (newItem.Destroyed)
                {
                    break;
                }
            }
            if (CanStoreMoreItems && !newItem.Destroyed)
            {
                newItem.Position = Position;
            }
        }
        public override string GetInspectString()
        {
            string original = base.GetInspectString();
            StringBuilder stringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(original))
            {
                stringBuilder.AppendLine(original);
            }
            stringBuilder.Append("PRF_TotalStacksNum".Translate(totalStoredItems, Extension.limit));
            return stringBuilder.ToString();
        }
        public override void DeSpawn()
        {
            List<Thing> thingsToSplurge = new List<Thing>(Position.GetThingList(Map));
            for (int i = 0; i < thingsToSplurge.Count; i++)
            {
                if (thingsToSplurge[i].def.category == ThingCategory.Item)
                {
                    thingsToSplurge[i].DeSpawn();
                    GenPlace.TryPlaceThing(thingsToSplurge[i], Position, Map, ThingPlaceMode.Near);
                }
            }
            base.DeSpawn();
        }
        public override void Notify_LostThing(Thing newItem)
        {
            base.Notify_LostThing(newItem);
            RefreshStorage();
            totalStoredItems--;
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            RefreshStorage();
        }
        protected virtual void RefreshStorage()
        {
            foreach (IntVec3 cell in AllSlotCells())
            {
                if (cell != Position)
                {
                    List<Thing> things = cell.GetThingList(Map);
                    for (int i = 0; i < things.Count; i++)
                    {
                        Thing item = things[i];
                        Log.Message($"{item.GetUniqueLoadID()} at {item.Position}");
                        if (item.def.category == ThingCategory.Item)
                        {
                            RegisterNewItem(item);
                        }
                    }
                }
            }
        }
        public override void ExposeData()
        {
            Scribe_Values.Look(ref totalStoredItems, "total");
            base.ExposeData();
        }
    }
}

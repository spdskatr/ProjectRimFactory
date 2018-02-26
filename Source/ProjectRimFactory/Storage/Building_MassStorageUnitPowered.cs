using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ProjectRimFactory.Storage
{
    public class Building_MassStorageUnitPowered : Building_MassStorageUnit
    {
        public override bool CanStoreMoreItems => GetComp<CompPowerTrader>().PowerOn;
        public override bool CanReceiveIO => base.CanReceiveIO && GetComp<CompPowerTrader>().PowerOn;

        public override void Notify_ReceivedThing(Thing newItem)
        {
            base.Notify_ReceivedThing(newItem);
            UpdatePowerConsumption();
        }
        public void UpdatePowerConsumption()
        {
            GetComp<CompPowerTrader>().PowerOutput = -10 * StoredItemsCount;
        }

        protected override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            switch (signal)
            {
                case "PowerTurnedOn":
                    RefreshStorage();
                    break;
                default:
                    break;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            UpdatePowerConsumption();
        }
    }
}

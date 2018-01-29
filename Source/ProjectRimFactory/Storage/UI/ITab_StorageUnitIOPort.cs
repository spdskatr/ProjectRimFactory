using ProjectRimFactory.Storage.Editables;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ProjectRimFactory.Storage.UI
{
    public class ITab_StorageUnitIOPort : ITab
    {
        public ITab_StorageUnitIOPort()
        {
            size = new Vector2(480f, 480f);
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
                    select new FloatMenuOption(b.LabelCap, () => SelBuilding.BoundStorageUnit = b, MenuOptionPriority.Default, () => GenDraw.DrawArrowPointingAt(b.TrueCenter()), b)
                );
                if (list.Count == 0)
                {
                    list.Add(new FloatMenuOption("NoneBrackets".Translate(), null));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            listing.End();
        }
    }
}

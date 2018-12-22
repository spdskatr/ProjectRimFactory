using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using SimpleFixes;
using ProjectRimFactory.Storage;

namespace ProjectRimFactory.Common
{
    public class ProjectRimFactory_ModComponent : Mod
    {
        public ProjectRimFactory_ModComponent(ModContentPack content) : base(content)
        {
            settings = GetSettings<ProjectRimFactory_ModSettings>();
            harmony = HarmonyInstance.Create("com.spdskatr.projectrimfactory");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            NoMessySpawns.Instance.Add(ShouldSuppressDisplace, (Building_MassStorageUnit b, Map map) => true);
        }

        public HarmonyInstance harmony;

        public ProjectRimFactory_ModSettings settings;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "ProjectRimFactoryModName".Translate();
        }

        public override void WriteSettings()
        {
            settings.Write();
        }

        public static bool ShouldSuppressDisplace(IntVec3 cell, Map map, bool respawningAfterLoad)
        {
            return !respawningAfterLoad || map?.thingGrid.ThingsListAtFast(cell).OfType<Building_MassStorageUnit>().Any() != true;
        }
    }
}

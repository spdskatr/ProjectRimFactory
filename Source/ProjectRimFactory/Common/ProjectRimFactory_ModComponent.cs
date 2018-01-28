using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace ProjectRimFactory.Common
{
    public class ProjectRimFactory_ModComponent : Mod
    {
        public ProjectRimFactory_ModComponent(ModContentPack content) : base(content)
        {
            settings = GetSettings<ProjectRimFactory_ModSettings>();
            harmony = HarmonyInstance.Create("com.spdskatr.projectrimfactory");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
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
    }
}

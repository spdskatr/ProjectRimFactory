using UnityEngine;
using Verse;
using System.IO;
using System.Reflection;

namespace ProjectSAL
{
    public class SALModSettings : ModSettings
    {
        public bool FixCoreDriller;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref FixCoreDriller, "FixCoreDriller");
            ProjectSAL_OnGameLoadChecker.CheckCoreDriller();
        }

        public void DoWindowContents(Rect inRect)
        {
            var list = new Listing_Standard()
            {
                ColumnWidth = inRect.width
            };
            list.Begin(inRect);
            list.CheckboxLabeled("SALSettings_CoreDrillerFix".Translate(), ref FixCoreDriller, "SALSettings_CoreDrillerFix_Desc".Translate());
            list.End();
        }

        public void WriteSettings(SALMod instance) => LoadedModManager.WriteModSettings(instance.Content.Identifier, instance.GetType().Name, this);
    }
    public class SALMod : Mod
    {
        public SALModSettings settings = new SALModSettings();

        public ModContentPack contentPack;

        public SALMod(ModContentPack content) : base(content)
        {
            contentPack = content;
            string path = (string)typeof(LoadedModManager).GetMethod("GetSettingsFilename", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { Content.Identifier, GetType().Name });
            if (File.Exists(path))
                settings = GetSettings<SALModSettings>();
        }

        public override void WriteSettings() => settings.WriteSettings(this);

        public override string SettingsCategory() => contentPack.Name;

        public override void DoSettingsWindowContents(Rect inRect) => settings.DoWindowContents(inRect);
    }
}

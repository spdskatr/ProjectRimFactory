using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace ProjectRimFactory.Drones
{
    [StaticConstructorOnStartup]
    public static class DroneBackstories
    {
        public static FieldInfo nameField = typeof(Backstory).GetField("title", BindingFlags.Instance | BindingFlags.NonPublic);
        public static Backstory childhood;
        public static Backstory adulthood;
        static DroneBackstories()
        {
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                childhood = new Backstory()
                {
                    identifier = "PRFNoneBracketsC",
                    workDisables = WorkTags.Social,
                    slot = BackstorySlot.Childhood,
                    baseDesc = "NoneBrackets".Translate()
                };
                nameField.SetValue(childhood, "NoneBrackets".Translate());
                BackstoryDatabase.AddBackstory(childhood);
                adulthood = new Backstory()
                {
                    identifier = "PRFNoneBracketsA",
                    workDisables = WorkTags.Social,
                    slot = BackstorySlot.Adulthood,
                    baseDesc = "NoneBrackets".Translate()
                };
                nameField.SetValue(childhood, "NoneBrackets".Translate());
                BackstoryDatabase.AddBackstory(adulthood);
            });
        }
    }
}

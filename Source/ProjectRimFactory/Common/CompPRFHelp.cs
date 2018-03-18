using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ProjectRimFactory.Common
{
    [StaticConstructorOnStartup]
    public class CompPRFHelp : ThingComp
    {
        public static readonly Texture2D LaunchReportTex = ContentFinder<Texture2D>.Get("UI/Commands/LaunchReport", true);
        public string HelpText
        {
            get
            {
                if (Translator.TryTranslate($"{parent.def.defName}_HelpText", out string text))
                {
                    return text;
                }
                return null;
            }
        }
        public string OrdoText
        {
            get
            {
                if (Translator.TryTranslate($"{parent.def.defName}_OrdoText", out string text))
                {
                    return text;
                }
                return null;
            }
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra()) yield return g;
            string helpText = HelpText;
            if (!string.IsNullOrEmpty(helpText))
            {
                yield return new Command_Action
                {
                    defaultLabel = "PRFHelp".Translate(),
                    defaultDesc = "PRFHelpDesc".Translate(),
                    icon = LaunchReportTex,
                    action = () =>
                    {
                        Find.WindowStack.Add(new Dialog_MessageBox(helpText));
                    }
                };
            }
            if (PRFDefOf.PRFOrdoDataRummaging.IsFinished)
            {
                string ordoText = OrdoText;
                if (!string.IsNullOrEmpty(ordoText))
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "PRFViewOrdo".Translate(parent.LabelCapNoCount),
                        icon = LaunchReportTex,
                        action = () =>
                        {
                            Find.WindowStack.Add(new Dialog_MessageBox(ordoText));
                        }
                    };
                }
            }
        }
    }
}

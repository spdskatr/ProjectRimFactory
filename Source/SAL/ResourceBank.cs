using System;

using UnityEngine;
using Verse;

namespace ProjectSAL
{
    public static class ResourceBank
    {
        [StaticConstructorOnStartup]
        public static class Texture
        {
            public static Texture2D Compass = ContentFinder<Texture2D>.Get ("UI/Misc/Compass");
            public static Texture2D ForbiddenOverlay = ContentFinder<Texture2D>.Get ("Things/Special/ForbiddenOverlay");
            public static Texture2D DesignatorCancel = ContentFinder<Texture2D>.Get ("UI/Designators/Cancel");
            public static Texture2D EditActiveHours = ContentFinder<Texture2D>.Get ("UI/Designators/EditActiveHours");
        }

        public static class String
        {
            public static string AdjustDirection_Output = "AdjustDirection_Output".Translate ();
            public static string AdjustDirection_Desc (object direction) => "AdjustDirection_Desc".Translate (direction);
            public static string SALToggleForbidden = "SALToggleForbidden".Translate ();
            public static string SALToggleForbidden_Desc = "SALToggleForbidden_Desc".Translate ();
            public static string SALCancelBills = "SALCancelBills".Translate ();
            public static string SALCancelBills_Desc = "SALCancelBills_Desc".Translate ();
            public static string SALAssignTimeTable = "SALAssignTimeTable".Translate ();
            public static string SALAssignTimeTable_Desc = "SALAssignTimeTable_Desc".Translate ();
        }
    }
}

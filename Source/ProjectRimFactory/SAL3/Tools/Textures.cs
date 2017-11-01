using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ProjectRimFactory.SAL3.Tools
{
    [StaticConstructorOnStartup]
    public static class Textures
    {
        public static readonly Texture2D button_play_red = ContentFinder<Texture2D>.Get("SAL3/button_play_red", true);
        public static readonly Texture2D button_record_red = ContentFinder<Texture2D>.Get("SAL3/button_record_red", true);
        public static readonly Texture2D button_pause_black = ContentFinder<Texture2D>.Get("SAL3/button_pause_black", true);
        public static readonly Texture2D button_rewind_black = ContentFinder<Texture2D>.Get("SAL3/button_rewind_black", true);
    }
}

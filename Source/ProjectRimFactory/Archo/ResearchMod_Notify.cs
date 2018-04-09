using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ProjectRimFactory.Archo
{
    public class ResearchMod_Notify : ResearchMod
    {
        public string text;
        public override void Apply()
        {
            if (Find.WindowStack.WindowOfType<Dialog_MessageBox>() == null)
            {
                Find.WindowStack.Add(new Dialog_MessageBox(text));
            }
        }
    }
}

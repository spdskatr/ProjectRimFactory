using ProjectRimFactory.SAL3.Things.Assemblers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ProjectRimFactory.SAL3.UI
{
    public class ITab_SAL3Bills : ITab
    {
        public ITab_SAL3Bills()
        {
            size = WinSize;
            labelKey = "SAL3_BillsTabLabel";
        }
        
        protected Building_DynamicBillGiver SelAssembler
        {
            get
            {
                return (Building_DynamicBillGiver)SelThing;
            }
        }
        
        protected override void FillTab()
        {
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.BillsTab, KnowledgeAmount.FrameDisplayed);
            Rect rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
            Func<List<FloatMenuOption>> recipeOptionsMaker = () =>
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach(RecipeDef r in SelAssembler.GetAllRecipes())
                {
                    if (r.AvailableNow)
                    {
                        list.Add(new FloatMenuOption(r.LabelCap, () =>
                        {
                            Bill bill = r.MakeNewBill();
                            SelAssembler.BillStack.AddBill(bill);
                        }));
                    }
                }
                if (!list.Any())
                {
                    list.Add(new FloatMenuOption("NoneBrackets".Translate(), null));
                }
                return list;
            };
            mouseoverBill = SelAssembler.BillStack.DoListing(rect, recipeOptionsMaker, ref scrollPosition, ref viewHeight);
        }
        
        private float viewHeight = 1000f;
        
        private Vector2 scrollPosition = default(Vector2);
        
        private Bill mouseoverBill;
        
        private static readonly Vector2 WinSize = new Vector2(420f, 480f);
    }
}

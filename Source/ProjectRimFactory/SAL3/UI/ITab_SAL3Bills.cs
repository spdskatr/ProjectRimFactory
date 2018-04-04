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
        private float viewHeight = 1000f;

        private Vector2 scrollPosition = default(Vector2);

        private Bill mouseoverBill;

        private static readonly Vector2 WinSize = new Vector2(420f, 480f);

        protected Building_DynamicBillGiver SelAssembler
        {
            get
            {
                return (Building_DynamicBillGiver)SelThing;
            }
        }

        public ITab_SAL3Bills()
        {
            size = WinSize;
            labelKey = "SAL3_BillsTabLabel";
        }

        public override bool IsVisible => SelThing is Building_DynamicBillGiver;

        protected override void FillTab()
        { 
            Rect rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
            Func<List<FloatMenuOption>> recipeOptionsMaker = delegate
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (RecipeDef recipe in SelAssembler.GetAllRecipes())
                {
                    if (recipe.AvailableNow)
                    {
                        list.Add(new FloatMenuOption(recipe.LabelCap, delegate
                        {
                            Bill bill = recipe.MakeNewBill();
                            SelAssembler.BillStack.AddBill(bill);
                        }, MenuOptionPriority.Default, null, null, 29f, (Rect r) => Widgets.InfoCardButton(r.x + 5f, r.y + (r.height - 24f) / 2f, recipe), null));
                    }
                }
                if (list.Count == 0)
                {
                    list.Add(new FloatMenuOption("NoneBrackets".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                return list;
            };
            mouseoverBill = SelAssembler.BillStack.DoListing(rect, recipeOptionsMaker, ref this.scrollPosition, ref this.viewHeight);
        }

        public override void TabUpdate()
        {
            if (mouseoverBill != null)
            {
                mouseoverBill.TryDrawIngredientSearchRadiusOnMap(SelAssembler.Position);
                mouseoverBill = null;
            }
        }
    }
}

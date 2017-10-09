using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ProjectSAL
{
    public partial class Building_Assembler
    {
        public void DoSelfPawnAnalysis()
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine("Beginning S.A.L. pawn analysis.");
            b.AppendFormat("Pawn '{0}':\nSkills:\n", buildingPawn.Name);
            for (int i = 0; i < buildingPawn.skills.skills.Count; i++)
            {
                SkillRecord skill = buildingPawn.skills.skills[i];
                b.AppendFormat("Skill {0}: Level: {1}, Incapable: {2}\n", skill.def.label, skill.Level, skill.TotallyDisabled);
            }
            b.AppendFormat("Backstories: CHILD: {0} ADULT: {1}\n", buildingPawn.story.childhood, buildingPawn.story.adulthood);
            b.AppendLine("End S.A.L. pawn analysis.");
            Log.Message(b.ToString());
        }
        public void ShowBillStack()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("S.A.L.: Analysis for '{0}'\nWorkTable != null: {1}\nShouldDoWork: {2}\nShouldStartBill: {3}\ncurrentRecipe: {4}\nWorkTableIsDisabled: {5}\nWorkTableIsDormant: {6}\nWorkTableIsPoweredOff: {7}\nWorkTableisReservedByOther: {8}\n!ingredients.Any(ingredient => ingredient.count > 0f): {9}\n", GetUniqueLoadID(), WorkTable != null, ShouldDoWork, ShouldStartBill, currentRecipe, WorkTableIsDisabled, WorkTableIsDormant, WorkTableIsPoweredOff, WorkTableisReservedByOther, !ingredients.Any(ingredient => ingredient.count > 0f));
            if (WorkTable != null)
            {
                builder.AppendFormat("BillStack != null: {0}\n", WorkTableBillStack != null);
                if (WorkTableBillStack != null)
                {
                    builder.AppendFormat("Bills [Count = {0}, AnyShouldDoNow: {1}]:\n", WorkTableBillStack.Bills.Count, WorkTableBillStack.AnyShouldDoNow);
                    for (int i = 0; i < WorkTableBillStack.Bills.Count; i++)
                    {
                        Bill bill = WorkTableBillStack.Bills[i];
                        builder.AppendFormat("Bill {0}: Name: {1} ShouldDoNow: {2}\n", i, bill.Label, bill.ShouldDoNow());
                    }
                }
            }
            builder.Append("End.");
            Log.Message(builder.ToString());
        }
    }
}

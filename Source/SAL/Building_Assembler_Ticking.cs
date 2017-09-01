using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;

namespace ProjectSAL
{
    public partial class Building_Assembler
    {
        public bool CheckIfShouldActivate()
        {
            if ((!ShouldDoWorkInCurrentTimeAssignment) || WorkTableIsDisabled || WorkTableIsDormant)
            {
                if (Map.reservationManager.IsReserved(new LocalTargetInfo(WorkTable), Faction)) ReleaseAll();
                var powerComp = GetComp<CompPowerTrader>();
                //Change to low power
                if (powerComp != null)
                {
                    powerComp.powerOutputInt = -Extension.powerUsageLowPower;
                }
                cachedShouldActivate = false;
                return false;
            }
            else if (!GetComp<CompPowerTrader>().PowerOn)
            {
                if (Map.reservationManager.IsReserved(new LocalTargetInfo(WorkTable), Faction)) ReleaseAll();
                cachedShouldActivate = false;
                return false;
            }
            else
            {
                //Restore high power
                var powerComp = GetComp<CompPowerTrader>();
                if (powerComp != null)
                {
                    powerComp.powerOutputInt = -def.GetCompProperties<CompProperties_Power>().basePowerConsumption;
                }
            }
            cachedShouldActivate = true;
            return true;
        }

        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % 30 == 0)
            {
                this.CheckForCoreDrillerSetting();
                if (!CheckIfShouldActivate()) return;
            }
            else
            {
                if (!cachedShouldActivate) return;
            }

            if (ShouldDoWork && SoundOfCurrentRecipe != null && !WorkTableIsDisabled)
                PlaySustainer();
            if (WorkTable != null && !Map.reservationManager.IsReserved(new LocalTargetInfo(WorkTable), Faction)) TryReserve();
            if (Find.TickManager.TicksGame % 35 == 0)
                AcceptItems();//once every 35 ticks
            if (Find.TickManager.TicksGame % 60 == 0)
                TickSecond();//once every 60 ticks
        }

        public virtual void TickSecond()
        {
            TryOutputItem();

            if (ResetIfWorkTableIsNull())
                return;
            if (ShouldStartBill)
                SetRecipe(BillStack.FirstShouldDoNow);
            if (ShouldDoWork)
            {
                if (workLeft <= 0)
                {
                    ThingDef mainIngDef = CalculateDominantIngredient(currentRecipe, thingRecord).def;
                    workLeft = currentRecipe.WorkAmountTotal(mainIngDef);
                }
                DoWork();
            }
            if (WorkDone)
                TryMakeProducts();
        }

        public void PlaySustainer()
        {
            if (sustainer == null || sustainer.Ended)
            {
                var soundInfo = SoundInfo.InMap(new TargetInfo(this), MaintenanceType.PerTick);
                sustainer = SoundOfCurrentRecipe.TrySpawnSustainer(soundInfo);
            }
            else
            {
                sustainer.Maintain();
            }
        }

        public virtual void DoWork(int interval = 60)
        {
            if (WorkTableIsDisabled)
            {
                ReleaseAll();
                return;
            }
            else
            {
                TryReserve();
            }
            if (workLeft > 0)
            {
                //Skill factors for each skill are calculated in CalculateCraftingSpeedFactor
                float skillFactor = currentRecipe.workSpeedStat.CalculateCraftingSpeedFactor(buildingPawn, Extension);

                //Factor from stuff, as well as extra work speed. The lighter the mass of the stuff it's made out of, the faster it crafts.
                //Steel is the base, so the factor of steel must equal 1
                float factorFromStuff = (Stuff.statBases.Find(s => s.stat == StatDefOf.MeleeWeapon_Cooldown)?.value ?? 0.5f) * 2;
                float extraFactor = Extension.globalFactor;
                workLeft -= (interval * skillFactor * extraFactor / factorFromStuff);
                if (workLeft <= 0f)
                {
                    workLeft = 0f;
                }
            }
        }
    }
}

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace ProjectSAL
{
    public partial class Building_Assembler
    {
        /// <summary>
        /// Displays skills in inspector
        /// </summary>
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats
        {
            get
            {
                var extensionSkills = Extension.skills.ListFullCopy();
                foreach (StatDrawEntry stat in base.SpecialDisplayStats)
                    yield return stat;
                foreach (var skill in DefDatabase<SkillDef>.AllDefs)
                {
                    foreach (SkillLevel skillLevel in extensionSkills)
                    {
                        if (skillLevel.skillDef == skill)
                        {
                            yield return new StatDrawEntry(StatCategoryDefOf.PawnMisc, skillLevel.skillDef.label, skillLevel.ToString());
                            extensionSkills.Remove(skillLevel);
                            goto SkillRecorded;
                        }
                    }
                    yield return new StatDrawEntry(StatCategoryDefOf.PawnMisc, skill.label, Extension.defaultSkillLevel.ToString());
                    SkillRecorded:;
                }
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (buildingPawn == null)
            {
                DoPawn();
            }
            else
            {
                var fieldInfo = typeof(Thing).GetField("mapIndexOrState", BindingFlags.NonPublic | BindingFlags.Instance);
                //Assign Pawn's mapIndexOrState to building's mapIndexOrState
                fieldInfo.SetValue(buildingPawn, fieldInfo.GetValue(this));
                //Assign Pawn's position without nasty errors
                buildingPawn.SetPositionDirect(Position);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref buildingPawn, "pawn");
            Scribe_Defs.Look(ref currentRecipe, "currentRecipe");
            Scribe_Values.Look(ref workLeft, "workLeft");
            Scribe_Values.Look(ref rotOutput, "rotOutput");
            Scribe_Values.Look(ref allowForbidden, "allowForbidden", true);
            Scribe_Collections.Look(ref ingredients, "ingredients", LookMode.Deep);
            Scribe_Collections.Look(ref thingRecord, "thingRecord", LookMode.Deep);
            Scribe_Collections.Look(ref thingPlacementQueue, "placementQueue", LookMode.Deep);
            if (buildingPawn == null)
            {
                DoPawn();
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
                yield return g;
            yield return new Command_Action
            {
                icon = ResourceBank.Texture.Compass,
                defaultLabel = ResourceBank.String.AdjustDirection_Output,
                defaultDesc = ResourceBank.String.AdjustDirection_Desc (rotOutput),
                activateSound = SoundDefOf.Click,
                action = () => rotOutput = rotOutput + 1 < OutputSlots ? rotOutput + 1 : 0
            };
            yield return new Command_Toggle
            {
                icon = ResourceBank.Texture.ForbiddenOverlay,
                defaultLabel = ResourceBank.String.SALToggleForbidden,
                defaultDesc = ResourceBank.String.SALToggleForbidden_Desc,
                isActive = () => allowForbidden,
                toggleAction = () => allowForbidden = !allowForbidden
            };
            yield return new Command_Action
            {
                icon = ResourceBank.Texture.DesignatorCancel,
                defaultLabel = ResourceBank.String.SALCancelBills,
                defaultDesc = ResourceBank.String.SALCancelBills_Desc,
                activateSound = SoundDefOf.Click,
                action = () => {
                    DropAllThings ();
                    ResetRecipe ();
                }
            };
            yield return new Command_Action
            {
                defaultLabel = ResourceBank.String.SALAssignTimeTable,
                defaultDesc = ResourceBank.String.SALAssignTimeTable_Desc,
                icon = ResourceBank.Texture.EditActiveHours,
                action = () => Find.WindowStack.Add (new Dialog_SALTimeTable (buildingPawn))
            };
            if (Prefs.DevMode)
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>
                {
                    new FloatMenuOption("Set workLeft to 1", () => workLeft = 1),
                    new FloatMenuOption("Drop everything", DropAllThings),
                    new FloatMenuOption("Do pawn skills analysis", DoSelfPawnAnalysis),
                    new FloatMenuOption("Reset pawn backstories/traits", () => SetBackstoryAndSkills(buildingPawn)),
                    new FloatMenuOption("Log work table debug info", ShowBillStack)
                };
                yield return new Command_Action
                {
                    defaultLabel = "Debug actions (click to show)",
                    action = () => Find.WindowStack.Add(new FloatMenu(list))
                };
            }
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            GenDraw.DrawFieldEdges(GenAdj.CellsAdjacent8Way(this).ToList());
            GenDraw.DrawFieldEdges(new List<IntVec3> { OutputSlot }, Color.green);
            if (def.hasInteractionCell)
                Graphics.DrawMesh(MeshPool.plane10, WorkTableCell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays), Quaternion.identity, GenDraw.InteractionCellMaterial, 0);
        }

        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.GetInspectString().TrimEndNewlines());
            stringBuilder.AppendLine("SALInspect_WorkLeft".Translate(workLeft.ToStringWorkAmount()));
            stringBuilder.AppendLine("SALInspect_PlacementQueue".Translate(thingPlacementQueue.Count));
            if (!GetComp<CompPowerTrader>().PowerOn)
            {
                stringBuilder.Append("SALInspect_PowerOff".Translate());
            }
            else
            {
                stringBuilder.Append("SALInspect_ResourcesNeeded".Translate());
                foreach (_IngredientCount ingredient in ingredients)
                {
                    var str = ingredient.ToString();
                    stringBuilder.Append(str + " ");//(75 x Steel) (63 x Wood) etc
                }
            }
            return stringBuilder.ToString();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            ReleaseAll();
            DropAllThings();
            base.Destroy(mode);
        }
    }
}

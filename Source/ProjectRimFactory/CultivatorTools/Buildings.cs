using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;
using UnityEngine;
using RimWorld;

namespace ProjectRimFactory.CultivatorTools
{
    public class Building_Cultivator : Building_SquareCellIterator
    {
        public Rot4 outputRotation = Rot4.North;
        
        public IntVec3 OutputSlot => Position + GenAdj.CardinalDirections[outputRotation.AsInt];

        #region Abstract stuff
        public override int TickRate => def.GetModExtension<CultivatorDefModExtension>()?.TickFrequencyDivisor ?? 200;

        public override bool CellValidator(IntVec3 c) => base.CellValidator(c) && (Utilities.GetIPlantToGrowSettable(c, Map)?.CanPlantRightNow() ?? false);

        public override bool DoIterationWork(IntVec3 c)
        {
            var zone = Utilities.GetIPlantToGrowSettable(c, Map);
            var plantDef = zone.GetPlantDefToGrow();
            foreach (var t in c.GetThingList(Map))
            {
                if (t is Plant p)
                {
                    if (t.def == plantDef)
                    {
                        if (p.Growth + 0.001f >= 1.00f)
                        {
                            //Harvests fully grown plants
                            CreatePlantProducts(p);
                            return false;
                        }
                        return true;
                    }
                    else
                    {
                        //Destroys foreign plants
                        CreatePlantProducts(p);
                        if (!p.Destroyed) p.Destroy();
                        return false;
                    }
                }
            }
            //If no plant of specified type, plants one
            TryPlantNew(c, plantDef);
            return true;
        }
        #endregion

        public void TryPlantNew(IntVec3 c, ThingDef plantDef)
        {
            if (plantDef.blueprintDef != null && Utilities.SeedsPleaseActive && plantDef.blueprintDef.category == ThingCategory.Item)
            {
                if (!TryPlantNewSeedsPleaseActive(plantDef))
                    return;
            }
            if (plantDef.CanEverPlantAt(c, Map) && GenPlant.AdjacentSowBlocker(plantDef, c, Map) == null)
                GenPlace.TryPlaceThing(ThingMaker.MakeThing(plantDef), c, Map, ThingPlaceMode.Direct);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref outputRotation, "outputRotation");
        }

        #region SeedsPlease activated stuff
        /// <summary>
        /// SeedsPlease activated code for trying to take seeds, credit to notfood for original mod
        /// </summary>
        private bool TryPlantNewSeedsPleaseActive(ThingDef plantDef)
        {
            var detectorCells = this.OccupiedRect().ExpandedBy(1);
            Thing seed = null;
            foreach (var cell in detectorCells)
            {
                var temp = cell.GetThingList(Map).Find(t => t.def == plantDef.blueprintDef);
                if (temp != null)
                {
                    seed = temp;
                    break;
                }
            }
            if (seed == null)
            {
                return false;
            }
            seed.stackCount--;
            if (seed.stackCount <= 0)
            {
                seed.Destroy();
            }
            return true;
        }

        /// <summary>
        /// SeedsPlease activated code for creating plant products, credit to notfood for original mod
        /// </summary>
        protected void CreatePlantProductsSeedsPleaseActive(Plant p)
        {
            var seed = p.def.blueprintDef;
            var type = seed.GetType();
            var props = type.GetField("seed").GetValue(seed);
            var propType = props.GetType();
            int count = 0;
            //This section of code adapted of notfood's original source
            float parameter = Mathf.Max(Mathf.InverseLerp(p.def.plant.harvestMinGrowth, 1.2f, p.Growth), 1f);
            if ((float)propType.GetField("seedFactor").GetValue(props) > 0f && Rand.Value < (float)propType.GetField("baseChance").GetValue(props) * parameter)
            {
                if (Rand.Value < (float)propType.GetField("extraChance").GetValue(props))
                {
                    count = 2;
                }
                else
                {
                    count = 1;
                }
                var thing = ThingMaker.MakeThing(seed);
                thing.stackCount = count;
                GenSpawn.Spawn(thing, OutputSlot, Map);
            }
        }
        #endregion

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo baseGizmo in base.GetGizmos())
            {
                yield return baseGizmo;
            }
            yield return new Command_Action
            {
                action = MakeMatchingGrowZone,
                hotKey = KeyBindingDefOf.Misc2,
                defaultDesc = "CommandSunLampMakeGrowingZoneDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneCreate_Growing"),
                defaultLabel = "CommandSunLampMakeGrowingZoneLabel".Translate()
            };
            yield return new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("UI/Misc/Compass"),
                defaultLabel = "CultivatorTools_AdjustDirection_Output".Translate(),
                defaultDesc = "CultivatorTools_AdjustDirection_Desc".Translate(outputRotation.AsCompassDirection()),
                activateSound = SoundDefOf.Click,
                action = () => outputRotation.Rotate(RotationDirection.Clockwise)
            };
        }

        protected void MakeMatchingGrowZone()
        {
            Designator_ZoneAdd_Growing designator = new Designator_ZoneAdd_Growing();
            designator.DesignateMultiCell(from tempCell in iter.cellPattern
                                          let pos = tempCell + Position
                                          where designator.CanDesignateCell(pos).Accepted
                                          select pos);
        }

        public virtual void CreatePlantProducts(Plant p)
        {
            int num2 = p.YieldNow();
            if (num2 > 0)
            {
                Thing thing = ThingMaker.MakeThing(p.def.plant.harvestedThingDef, null);
                thing.stackCount = num2;
                GenPlace.TryPlaceThing(thing, OutputSlot, Map, ThingPlaceMode.Near, null);
            }
            if (Utilities.SeedsPleaseActive && p.def.blueprintDef != null)
                CreatePlantProductsSeedsPleaseActive(p);

            p.PlantCollected();
        }

        public override string GetDescription() => base.GetDescription() + " " +
            ((Utilities.SeedsPleaseActive) ? "CultivatorTools_SeedsPleaseActiveDesc".Translate() : "CultivatorTools_SeedsPleaseInactiveDesc".Translate());

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            GenDraw.DrawFieldEdges(new List<IntVec3> { OutputSlot }, Color.cyan);
        }
    }
    public class Building_Sprinkler : Building_RadialCellIterator
    {
        public override int TickRate => 50;
        public override bool CellValidator(IntVec3 c)
        { 
            return base.CellValidator(c) && c.GetPlant(Map) != null;
        }

        public override bool DoIterationWork(IntVec3 c)
        {
            var plant = c.GetPlant(Map);
            if (plant != null && !Map.reservationManager.IsReserved(plant, Faction))
            {
                var rate = GetGrowthRatePerTickFor(plant);
                plant.Growth += rate * 2500;//Growth sped up by 1hr
            }
            return true;
        }
        public float GetGrowthRatePerTickFor(Plant p)
        {
            var num = 1f / (60000f * p.def.plant.growDays);
            return num * p.GrowthRate;
        }
    }
}

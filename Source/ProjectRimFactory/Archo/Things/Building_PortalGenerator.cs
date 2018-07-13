using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace ProjectRimFactory.Archo.Things
{
    public class Building_PortalGenerator : Building
    {
        public static readonly FieldInfo InnerContainerField = typeof(Building_Casket).GetField("innerContainer", BindingFlags.NonPublic | BindingFlags.Instance);
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos()) yield return g;
            if (Prefs.DevMode)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "DEBUG: Debug actions",
                    action = () => Find.WindowStack.Add(new FloatMenu(GetDebugActions()))
                };
            }
        }
        public List<FloatMenuOption> GetDebugActions()
        {
            return new List<FloatMenuOption>()
            {
                new FloatMenuOption("Liquidate room", LiquidateRoom)
            };
        }
        public void LiquidateRoom()
        {
            Room room = Position.GetRoom(Map, RegionType.Set_All);
            if (room != null && !room.PsychologicallyOutdoors)
            {
                float wealth = room.GetStat(RoomStatDefOf.Wealth);
                float roomSize = room.CellCount;
                float humanPawnCount = 0;
                float nonHumanPawnCount = 0;
                foreach (IntVec3 cell in room.Cells)
                {
                    if (Map.terrainGrid.CanRemoveTopLayerAt(cell))
                    {
                        Map.terrainGrid.RemoveTopLayer(cell, false);
                        FilthMaker.RemoveAllFilth(cell, Map);
                    }
                    foreach (Thing t in cell.GetThingList(Map).ToList())
                    {
                        if (t is Building_CryptosleepCasket)
                        {
                            foreach (Thing thing in ((IEnumerable<Thing>)InnerContainerField.GetValue(t)))
                            {
                                if (thing is Pawn p)
                                {
                                    if (p.RaceProps.Humanlike)
                                    {
                                        humanPawnCount++;
                                    }
                                    else
                                    {
                                        nonHumanPawnCount++;
                                    }
                                }
                            }
                        }
                        if (t.def.destroyable && t != this)
                        {
                            t.Destroy();
                        }
                    }
                }
                float points = 0.001f * wealth + roomSize + 10f * nonHumanPawnCount + 100f * humanPawnCount;
                if (Prefs.DevMode)
                {
                    Log.Message($"==SpdTec Room Liquidation Report==\nWealth: {wealth}\nRoom size: {roomSize}\nPawns: (non-human {nonHumanPawnCount}), (human {humanPawnCount})\nPoints: {points}");
                }
                this.Destroy();
            }
        }
    }
}

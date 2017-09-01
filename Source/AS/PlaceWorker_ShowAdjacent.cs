using System.Linq;
using Verse;

namespace AnimalStation
{
    class PlaceWorker_ShowAdjacent : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
        {
            GenDraw.DrawFieldEdges(GenAdj.OccupiedRect(center, rot, def.size).ExpandedBy(1).Cells.ToList().FindAll((IntVec3 c) => c.Standable(Map)));
        }
    }
}

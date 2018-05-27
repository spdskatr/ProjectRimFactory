using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace ProjectRimFactory.Drones.AI
{
    public class JobDriver_SelfTerminate : JobDriver
    {
        public override bool TryMakePreToilReservations()
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return new Toil()
            {
                initAction = () =>
                {
                    pawn.DeSpawn();
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}

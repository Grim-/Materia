using Verse;
using Verse.AI;

namespace Thor
{
    public class ThinkNode_ConditionalNearMaster : ThinkNode_Conditional
    {
        public float MaxDistanceToMaster = 5;

        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn != null && pawn.IsControlledSummon() && pawn.GetMaster() != null)
            {
                Pawn master = pawn.GetMaster();
                return pawn.Position.DistanceTo(master.Position) <= MaxDistanceToMaster;
            }
            return false;
        }
    }
}

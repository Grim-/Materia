using Verse;
using Verse.AI;

namespace Thor
{
    public class ThinkNode_ConditionalHasNoMaster : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn != null && pawn.IsControlledSummon() && pawn.GetMaster() == null;
        }
    }
}

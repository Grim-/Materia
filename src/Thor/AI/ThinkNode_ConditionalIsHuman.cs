using Verse;
using Verse.AI;

namespace Thor
{
    public class ThinkNode_ConditionalIsHuman : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn != null && pawn.RaceProps.Humanlike;
        }
    }
}

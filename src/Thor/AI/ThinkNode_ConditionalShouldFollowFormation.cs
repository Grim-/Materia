using Verse;
using Verse.AI;

namespace Thor
{
    public class ThinkNode_ConditionalShouldFollowFormation : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn == null || !pawn.IsControlledSummon())
            {
                return false;
            }

            Pawn MasterPawn = pawn.GetMaster();

            if (MasterPawn == null)
            {
                return false;
            }

            Hediff_UndeadMaster undeadMaster = (Hediff_UndeadMaster)MasterPawn.health.hediffSet.GetFirstHediffOfDef(ThorDefOf.DeathKnight_UndeadMaster);
            return undeadMaster != null && undeadMaster.InFormation;
        }
    }
}

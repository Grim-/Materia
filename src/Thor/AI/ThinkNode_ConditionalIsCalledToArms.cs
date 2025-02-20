using Verse;
using Verse.AI;

namespace Thor
{
    public class ThinkNode_ConditionalIsCalledToArms : ThinkNode_Conditional
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

            Hediff_Undead undeadMaster = (Hediff_Undead)pawn.health.hediffSet.GetFirstHediffOfDef(ThorDefOf.DeathKnight_Undead);
            return undeadMaster != null && undeadMaster.CalledToArms;
        }
    }
}

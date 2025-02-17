using RimWorld;
using Verse;
using Verse.AI;

namespace Thor
{
    public class ThinkNode_ConditionalSelfOrMasterHasTarget : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
     
            if (pawn == null)
            {
                Log.Message("ThinkNode_ConditionalSelfOrMasterHasTarget pawn is null");
                return false;
            }

            if (!pawn.Spawned)
            {
                Log.Message("ThinkNode_ConditionalSelfOrMasterHasTarget pawn is not spawned");
                return false;
            }

            Pawn master = pawn.GetMaster();
            if (master == null)
            {
                Log.Message("Master is null");
                return false;
            }

            if (pawn.mindState?.enemyTarget != null || master.mindState?.enemyTarget != null)
            {
                Log.Message("ThinkNode_ConditionalSelfOrMasterHasTarget pawn or master has target");
                return true;
            }

            if (pawn.Spawned && master.Spawned)
            {
                if (PawnUtility.EnemiesAreNearby(pawn, 10, true) ||
                    PawnUtility.EnemiesAreNearby(master, 10, true))
                {

                    Log.Message("ThinkNode_ConditionalSelfOrMasterHasTarget pawn or master has target nearby");
                    return true;
                }
            } 

            return false;
        }
    }
}

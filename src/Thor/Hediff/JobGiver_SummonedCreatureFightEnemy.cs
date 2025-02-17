using RimWorld;
using Verse;
using Verse.AI;

namespace Thor
{
    public class JobGiver_SummonedCreatureFightEnemy : JobGiver_AIDefendPawn
    {
        protected Pawn Master = null;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn == null)
            {
                Log.Error("TryGiveJob called with null pawn");
                return null;
            }

            this.chaseTarget = true;
            this.allowTurrets = true;
            this.ignoreNonCombatants = true;
            this.humanlikesOnly = false;

            Job job = base.TryGiveJob(pawn);

            if (job != null)
            {
                job.reportStringOverride = "Defending Summoner";
            }

            if (pawn.mindState != null)
            {
                pawn.mindState.canFleeIndividual = false;
            }

            return job;
        }

        protected override Pawn GetDefendee(Pawn pawn)
        {
            if (pawn.IsControlledSummon())
            {
                return pawn.GetMaster();
            }
            return null;
        }

        protected override float GetFlagRadius(Pawn pawn)
        {
            return 20f;
        }

        protected override IntVec3 GetFlagPosition(Pawn pawn)
        {
            return pawn.GetMaster().Position;
        }
    }
}

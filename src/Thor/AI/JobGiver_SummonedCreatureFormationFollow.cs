using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Thor
{
    public class JobGiver_SummonedCreatureFormationFollow : JobGiver_AIFollowMaster
    {
        protected override Pawn GetFollowee(Pawn pawn)
        {
            if (pawn.IsControlledSummon())
            {
                return pawn.GetMaster();
            }
            return null;
        }

        protected override float GetRadius(Pawn pawn)
        {
            return pawn.GetMaster().GetUndeadMaster().FollowDistance;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            Pawn followee = GetFollowee(pawn);
            if (followee == null)
            {
                Log.Error($"Followee is null for {pawn.LabelShort}");
                return null;
            }

            if (!followee.Spawned)
            {
                Log.Message($"Followee {followee.LabelShort} is not spawned");
                return null;
            }

            Hediff_UndeadMaster undeadMaster = (Hediff_UndeadMaster)followee.health.hediffSet.GetFirstHediffOfDef(ThorDefOf.DeathKnight_UndeadMaster);

            if (undeadMaster == null)
            {
                return null;
            }

            var activeUndead = undeadMaster.GetActiveCreatures();
            if (activeUndead == null || !activeUndead.Contains(pawn))
            {
                return null;
            }
            if (!JobDriver_FormationFollow.FarEnoughAndPossibleToStartJob(pawn, followee, undeadMaster, GetRadius(pawn)))
            {
                return null;
            }

            Job job = JobMaker.MakeJob(ThorDefOf.Thor_FormationFollow, followee);
            job.expiryInterval = 200;
            job.followRadius = undeadMaster.FollowDistance;
            job.SetTarget(TargetIndex.A, followee);
            job.reportStringOverride = $"Following {undeadMaster.pawn.LabelCap} in formation";
            return job;
        }
    }

    public class JobDriver_FormationFollow : JobDriver_FollowClose
    {
        private Hediff_UndeadMaster Hediff_UndeadMaster => (Hediff_UndeadMaster)this.TargetPawnA.health.GetOrAddHediff(ThorDefOf.DeathKnight_UndeadMaster);
   
        private List<Pawn> GetAllActiveShadows()
        {
            return Hediff_UndeadMaster.GetActiveCreatures();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            Toil formationToil = ToilMaker.MakeToil("FormationFollow");

            formationToil.tickAction = () =>
            {
                Pawn followee = this.TargetA.Pawn;
                float followRadius = this.job.followRadius;

                if (!this.pawn.pather.Moving || this.pawn.IsHashIntervalTick(30))
                {
                    List<Pawn> activeShadows = GetAllActiveShadows();
                    int formationIndex = activeShadows.IndexOf(this.pawn);

                    if (formationIndex == -1)
                    {
                        base.EndJobWith(JobCondition.Errored);
                        return;
                    }

                    IntVec3 targetCell = FormationUtils.GetFormationPosition(
                        Hediff_UndeadMaster.FormationType,
                        followee.Position.ToVector3(),
                        followee.Rotation,
                        formationIndex,
                        activeShadows.Count);

                    if (this.pawn.Position != targetCell)
                    {
                        if (!this.pawn.CanReach(targetCell, PathEndMode.OnCell, Danger.Deadly))
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                targetCell = CellFinder.StandableCellNear(targetCell, this.Map, 5);
                                if (!this.pawn.CanReach(targetCell, PathEndMode.OnCell, Danger.Deadly))
                                {
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        if (!this.pawn.CanReach(targetCell, PathEndMode.OnCell, Danger.Deadly))
                        {
                            base.EndJobWith(JobCondition.Incompletable);
                            return;
                        }

                        this.pawn.pather.StartPath(targetCell, PathEndMode.OnCell);
                        this.locomotionUrgencySameAs = followee;
                    }
                    else if (!followee.pather.Moving)
                    {
                        base.EndJobWith(JobCondition.Succeeded);
                        return;
                    }
                }
            };

            formationToil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return formationToil;
        }

        public override bool IsContinuation(Job j)
        {
            return this.job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
        }

        public static bool FarEnoughAndPossibleToStartJob(Pawn follower, Pawn followee, Hediff_UndeadMaster undeadMaster, float radius)
        {
            if (radius <= 0f)
            {
                Log.ErrorOnce($"Checking formation follow job with radius <= 0. pawn={follower.ToStringSafe<Pawn>()}",
                    follower.thingIDNumber ^ 843254009);
                return false;
            }

            var shadows = undeadMaster.GetActiveCreatures();
            int index = shadows.IndexOf(follower);

            if (index == -1)
                return false;

            IntVec3 targetCell = FormationUtils.GetFormationPosition(
                                undeadMaster.FormationType,
                                followee.Position.ToVector3(),
                                followee.Rotation,
                                index,
                                shadows.Count);

            return follower.CanReach(targetCell, PathEndMode.OnCell, Danger.Deadly);
        }
    }

}

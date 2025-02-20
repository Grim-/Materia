using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Thor
{
    public class JobGiver_BoidFormationFollow : JobGiver_AIFollowMaster
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
            if (followee == null || !followee.Spawned)
            {
                return null;
            }

            Hediff_UndeadMaster undeadMaster = followee.health.hediffSet.GetFirstHediffOfDef(ThorDefOf.DeathKnight_UndeadMaster) as Hediff_UndeadMaster;
            if (undeadMaster == null)
            {
                return null;
            }

            var activeUndead = undeadMaster.GetActiveCreatures();
            if (activeUndead == null || !activeUndead.Contains(pawn))
            {
                return null;
            }

            Job job = JobMaker.MakeJob(ThorDefOf.Thor_BoidFormationFollow, followee);
            job.expiryInterval = 100;
            job.followRadius = undeadMaster.FollowDistance;
            job.SetTarget(TargetIndex.A, followee);
            return job;
        }
    }

    public class JobDriver_BoidFormationFollow : JobDriver_FollowClose
    {
        private Hediff_UndeadMaster UndeadMaster =>
            (Hediff_UndeadMaster)this.TargetPawnA.health.hediffSet.GetFirstHediffOfDef(ThorDefOf.DeathKnight_UndeadMaster);

        private const float SEPARATION_WEIGHT = 1.5f;
        private const float COHESION_WEIGHT = 1.0f;
        private const float ALIGNMENT_WEIGHT = 1.0f;
        private const float NEIGHBOR_RADIUS = 10f;
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            Toil boidToil = ToilMaker.MakeToil("BoidFormationFollow");

            boidToil.tickAction = () =>
            {
                if (!this.pawn.pather.Moving || this.pawn.IsHashIntervalTick(15))
                {
                    UpdateBoidMovement();
                }
            };

            boidToil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return boidToil;
        }

        private void UpdateBoidMovement()
        {
            Pawn leader = this.TargetA.Pawn;
            List<Pawn> flock = UndeadMaster.GetActiveCreatures();

            if (!flock.Contains(this.pawn))
            {
                EndJobWith(JobCondition.Incompletable);
                return;
            }

            // Calculate boid forces
            IntVec3 separation = CalculateSeparation(flock);
            IntVec3 cohesion = CalculateCohesion(flock);
            IntVec3 alignment = CalculateAlignment(flock, leader);

            // Combine forces and get target cell
            IntVec3 targetCell = CalculateTargetCell(separation, cohesion, alignment);

            // Ensure the target is reachable
            if (!this.pawn.CanReach(targetCell, PathEndMode.OnCell, Danger.Deadly))
            {
                targetCell = CellFinder.StandableCellNear(targetCell, this.Map, 5f);
            }

            if (this.pawn.Position != targetCell &&
                this.pawn.CanReach(targetCell, PathEndMode.OnCell, Danger.Deadly))
            {
                this.pawn.pather.StartPath(targetCell, PathEndMode.OnCell);
                this.locomotionUrgencySameAs = leader;
            }
        }

        private IntVec3 CalculateSeparation(List<Pawn> flock)
        {
            IntVec3 separation = IntVec3.Zero;
            int count = 0;

            foreach (Pawn other in flock)
            {
                if (other == this.pawn) continue;

                float distance = this.pawn.Position.DistanceTo(other.Position);
                if (distance < NEIGHBOR_RADIUS)
                {
                    IntVec3 diff = this.pawn.Position - other.Position;
                    separation += diff;
                    count++;
                }
            }

            if (count > 0)
            {
                separation = new IntVec3(
                    separation.x / count,
                    separation.y / count,
                    separation.z / count
                );
            }

            return separation;
        }

        private IntVec3 CalculateCohesion(List<Pawn> flock)
        {
            if (flock.Count <= 1) return IntVec3.Zero;

            IntVec3 centerOfMass = IntVec3.Zero;
            int count = 0;

            foreach (Pawn other in flock)
            {
                if (other == this.pawn) continue;
                if (this.pawn.Position.DistanceTo(other.Position) < NEIGHBOR_RADIUS)
                {
                    centerOfMass += other.Position;
                    count++;
                }
            }

            if (count > 0)
            {
                centerOfMass = new IntVec3(
                    centerOfMass.x / count,
                    centerOfMass.y / count,
                    centerOfMass.z / count
                );
                return centerOfMass - this.pawn.Position;
            }

            return IntVec3.Zero;
        }

        private IntVec3 CalculateAlignment(List<Pawn> flock, Pawn leader)
        {
            // In RimWorld's case, alignment means following the leader's direction
            if (leader.pather.Moving)
            {
                return leader.pather.Destination.Cell - this.pawn.Position;
            }
            return IntVec3.Zero;
        }

        private IntVec3 CalculateTargetCell(IntVec3 separation, IntVec3 cohesion, IntVec3 alignment)
        {
            // Combine all forces with their weights
            IntVec3 combined = new IntVec3(
                (int)(separation.x * SEPARATION_WEIGHT +
                      cohesion.x * COHESION_WEIGHT +
                      alignment.x * ALIGNMENT_WEIGHT),
                0,
                (int)(separation.z * SEPARATION_WEIGHT +
                      cohesion.z * COHESION_WEIGHT +
                      alignment.z * ALIGNMENT_WEIGHT)
            );

            // Get the target cell relative to current position
            IntVec3 targetCell = this.pawn.Position + combined;

            // Ensure the target cell is within the map bounds
            targetCell.x = Mathf.Clamp(targetCell.x, 0, this.Map.Size.x - 1);
            targetCell.z = Mathf.Clamp(targetCell.z, 0, this.Map.Size.z - 1);

            return targetCell;
        }

        public override bool IsContinuation(Job j)
        {
            return this.job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
        }
    }
}

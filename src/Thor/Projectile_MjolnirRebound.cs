using RimWorld;
using UnityEngine;
using Verse;

namespace Thor
{
    public class Projectile_MjolnirRebound : Projectile_Delegate
    {
        private int maxRebounds = 3;
        private int currentRebounds = 0;
        private int reboundDistance = 10;

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (hitThing != null &&
                hitThing is Building &&
                currentRebounds < maxRebounds)
            {
                IntVec3 movementDirection = (Position - this.intendedTarget.Cell);

                IntVec3 reboundDir = GetReboundDirection(hitThing as Building, movementDirection);

                IntVec3 newDestination = Position + (reboundDir * reboundDistance);

                newDestination.x = Mathf.Clamp(newDestination.x, 0, Map.Size.x - 1);
                newDestination.z = Mathf.Clamp(newDestination.z, 0, Map.Size.z - 1);

                Projectile_MjolnirRebound reboundProjectile =
                    (Projectile_MjolnirRebound)ThingMaker.MakeThing(def);

                GenSpawn.Spawn(reboundProjectile, Position, Map);
                reboundProjectile.currentRebounds = this.currentRebounds + 1;
                reboundProjectile.OnImpact = this.OnImpact;

                reboundProjectile.Launch(
                    launcher,
                    new LocalTargetInfo(Position),
                    new LocalTargetInfo(newDestination),
                    ProjectileHitFlags.IntendedTarget
                );

                this.Destroy();
            }
            else
            {
                if (OnImpact != null)
                {
                    OnImpact(this, hitThing, blockedByShield);
                }
                Destroy();
            }
        }

        private IntVec3 GetReboundDirection(Building building, IntVec3 incomingDir)
        {
            IntVec3 hitOffset = Position - building.Position;

            if (Mathf.Abs(hitOffset.x) >= Mathf.Abs(hitOffset.z))
            {
                return new IntVec3(-incomingDir.x, 0, incomingDir.z);
            }
            else
            {
                return new IntVec3(incomingDir.x, 0, -incomingDir.z);
            }
        }
        private IntVec3 AddRandomness(IntVec3 direction)
        {
            if (Rand.Chance(0.3f))
            {
                if (Rand.Bool)
                {
                    direction.x += direction.x != 0 ? 0 : (Rand.Bool ? 1 : -1);
                }
                else
                {
                    direction.z += direction.z != 0 ? 0 : (Rand.Bool ? 1 : -1);
                }
            }
            return direction;
        }
    }
}

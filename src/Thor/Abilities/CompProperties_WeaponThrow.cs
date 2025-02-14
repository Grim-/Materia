using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Thor
{
    public class CompProperties_WeaponThrow : CompProperties_AbilityEffect
    {
        public float aoeRadius = 3f;

        public CompProperties_WeaponThrow()
        {
            compClass = typeof(Comp_WeaponThrow);
        }
    }

    public class Comp_WeaponThrow : CompAbilityEffect
    {
        new CompProperties_WeaponThrow Props => (CompProperties_WeaponThrow)props;
        private LightningRingBehavior lightningBehavior;
        private ThingWithComps thrownWeapon;

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            return thrownWeapon != null || parent.pawn.equipment?.Primary != null && base.Valid(target, throwMessages);
        }
        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return thrownWeapon != null || parent.pawn.equipment?.Primary != null && base.CanApplyOn(target, dest);
        }
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            Pawn pawn = parent.pawn;
            if (pawn == null || !pawn.Spawned)
                return;

            // If weapon is thrown and spawned, recall it
            if (thrownWeapon != null && thrownWeapon.Spawned)
            {
                RecallWeapon(pawn);
                this.parent.verb.verbProps.targetable = true;
            }
            // If weapon is thrown and equipped (means it was just summoned), throw it
            else if (thrownWeapon != null && pawn.equipment?.Primary == thrownWeapon)
            {
                ThrowWeapon(pawn, target);
                this.parent.verb.verbProps.targetable = false;
            }
            // If weapon isn't thrown at all but pawn has a weapon, throw it
            else if (thrownWeapon == null && pawn.equipment?.Primary != null)
            {
                ThrowWeapon(pawn, target);
                this.parent.verb.verbProps.targetable = false;
            }
        }

        //public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        //{
        //    base.Apply(target, dest);

        //    Pawn pawn = parent.pawn;
        //    if (pawn == null || !pawn.Spawned)
        //        return;

        //    if (thrownWeapon != null && thrownWeapon.Spawned)
        //    {
        //        RecallWeapon(pawn);
        //        this.parent.verb.verbProps.targetable = true;
        //    }
        //    else if(thrownWeapon != null && this.parent.pawn.equipment.Primary == thrownWeapon)
        //    {
        //        ThrowWeapon(pawn, target);
        //        this.parent.verb.verbProps.targetable = false;
        //    }
        //}

        private void ThrowWeapon(Pawn pawn, LocalTargetInfo target)
        {
            ThingWithComps weapon = pawn.equipment?.Primary;

            if (weapon == null)
                return;

            thrownWeapon = weapon;
            pawn.equipment.Remove(weapon);

            Projectile_MjolnirRebound throwProjectile = (Projectile_MjolnirRebound)ThingMaker.MakeThing(ThorDefOf.Thor_MjolnirProjectile);
            GenSpawn.Spawn(throwProjectile, pawn.Position, pawn.Map);
            throwProjectile.Launch(pawn, target, target, ProjectileHitFlags.IntendedTarget);
            throwProjectile.OnImpact = OnWeaponThrow_Land; 
        }
        private void OnWeaponThrow_Land(Projectile_Delegate projectile, Thing hitThing, bool blockedByshield)
        {
            if (projectile == null || !projectile.Spawned)
                return;

            lightningBehavior = new LightningRingBehavior(
                new List<LightningRingConfig>()
                {
                    {  new LightningRingConfig(2, 2) },
                    {  new LightningRingConfig(4, 4) }
                },
                projectile.Position,
                parent.pawn.Map,
                20
            );
            GenSpawn.Spawn(thrownWeapon, projectile.Position, projectile.Map);
        }
        public override void CompTick()
        {
            base.CompTick();

            if (lightningBehavior != null)
            {
                lightningBehavior?.Tick();

                if (lightningBehavior.IsFinished)
                {
                    lightningBehavior = null;
                }
            }
        }
        private void RecallWeapon(Pawn pawn)
        {
            if (thrownWeapon == null || !thrownWeapon.Spawned) 
                return;

            Projectile_Delegate throwProjectile = (Projectile_Delegate)ThingMaker.MakeThing(ThorDefOf.Thor_MjolnirProjectile);
            GenSpawn.Spawn(throwProjectile, thrownWeapon.Position, pawn.Map);
            throwProjectile.Launch(pawn, this.parent.pawn, this.parent.pawn, ProjectileHitFlags.IntendedTarget);
            thrownWeapon.DeSpawn();
            throwProjectile.OnImpact = OnWeaponThrow_Return;
        }
        private void OnWeaponThrow_Return(Projectile_Delegate projectile, Thing hitThing, bool blockedByshield)
        {
            if (thrownWeapon != null)
            {
                
                this.parent.pawn.equipment.AddEquipment(thrownWeapon);
                thrownWeapon = null;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_References.Look(ref thrownWeapon, "thrownWeapon");
        }
    }
}

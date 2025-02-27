using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Materia
{
    public class MateriaWorker : IExposable
    {
        public MateriaEffectDef def;
        public Pawn EquippingPawn;
        public ThingWithComps ParentEquipment;
        public Comp_Materia MateriaComp;
        public MateriaSlot MateriaSlot;

        private int CooldownTicks = 0;
        public int CooldownDurationTicks = 0;

        public MateriaWorker()
        {

        }

        public MateriaWorker(MateriaEffectDef def, Pawn equippingPawn, ThingWithComps parentEquipment)
        {
            this.def = def;
            EquippingPawn = equippingPawn;
            ParentEquipment = parentEquipment;
        }

        public virtual bool HasCooldown()
        {
            return CooldownDurationTicks > 0;
        }

        public virtual bool IsOnCooldown()
        {
            if (!HasCooldown())
            {
                return false;
            }

            return CooldownTicks > 0;
        }


        public virtual void Notify_MateriaEquipped()
        {

        }
        public virtual void Notify_MateriaUnequipped()
        {

        }

        public virtual void Notify_Equipped(Pawn pawn)
        {
            EquippingPawn = pawn;
        }
        public virtual void Notify_Unequipped(Pawn pawn)
        {
            EquippingPawn = null;
        }
        public virtual void Notify_ParentDestroyed()
        {
 
        }
        public virtual void Notify_ParentDespawned()
        {

        }
        public virtual void Notify_ProjectileImpact(Pawn Attacker, Thing Target, Projectile Projectile)
        {

        }

        public virtual bool Notify_PostPreApplyDamage(ref DamageInfo dinfo)
        {
            return false;
        }

        public virtual DamageInfo Notify_ProjectileApplyDamageToTarget(DamageInfo Damage, Pawn Attacker, Thing Target, Projectile Projectile)
        {
            return Damage;
        }

        public virtual void Notify_OwnerThoughtGained(Thought Thought, Pawn otherPawn)
        {

        }

        public virtual void Notify_OwnerThoughtLost(Thought Thought)
        {

        }
        public virtual void PostDraw()
        {

        }

        public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            return Enumerable.Empty<Gizmo>();
        }


        public virtual void Notify_KilledPawn(Pawn pawn)
        {

        }

        public virtual void Notify_OwnerKilled()
        {

        }

        public virtual void Notify_OwnerMood(Pawn pawn)
        {

        }

        public virtual void Notify_OwnerHediffGained(Hediff Hediff, BodyPartRecord partRecord, DamageInfo? dinfo, DamageWorker.DamageResult damageResult)
        {

        }
        public virtual void Notify_OwnerHediffRemoved(Hediff Hediff)
        {

        }

        public virtual void OnTick(Pawn pawn)
        {

        }

        public virtual void Notify_MateriaLinked(MateriaDef sourceMateria, MateriaDef linkedMateria)
        {
        
        }
        public virtual void Notify_MateriaUnlinked(MateriaDef sourceMateria, MateriaDef linkedMateria)
        {
        
        }

        public virtual DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult DamageWorkerResult)
        {
            return DamageWorkerResult;
        }
        public virtual float GetStatFactor(StatDef stat)
        {
            return 1f;
        }

        public virtual float GetStatOffset(StatDef stat)
        {
            return 0f;
        }
        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref CooldownTicks, "cooldownTicks");
        }


        public virtual void DestroyParentMateria()
        {
            if (this.MateriaComp != null && this.MateriaSlot != null)
            {

                if (this.EquippingPawn != null && this.EquippingPawn.Map != null)
                {
                    EffecterDefOf.Shield_Break.Spawn(this.EquippingPawn.Position, this.EquippingPawn.Map);
                }

                this.MateriaComp.UnequipMateria(this.MateriaSlot, false);
            }
        }

    }

}

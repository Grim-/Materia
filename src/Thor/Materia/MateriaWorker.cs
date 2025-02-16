using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Thor
{
    public class MateriaWorker : IExposable
    {
        public MateriaEffectDef def;
        public Pawn EquippingPawn;
        public ThingWithComps ParentEquipment;

        public MateriaWorker()
        {

        }

        public MateriaWorker(MateriaEffectDef def, Pawn equippingPawn, ThingWithComps parentEquipment)
        {
            this.def = def;
            EquippingPawn = equippingPawn;
            ParentEquipment = parentEquipment;
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

        public virtual DamageInfo Notify_ProjectileApplyDamageToTarget(DamageInfo Damage, Pawn Attacker, Thing Target, Projectile Projectile)
        {
            return Damage;
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

        public virtual void Notify_OwnerKilled(Map prevMap, DamageInfo? dinfo = null)
        {

        }

        public virtual void Notify_OwnerMood(Pawn pawn)
        {

        }

        public virtual void Notify_OwnerThought(Pawn pawn)
        {

        }
        public virtual void Notify_OwnerHediffAdded(Pawn pawn)
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
        }

    }

}

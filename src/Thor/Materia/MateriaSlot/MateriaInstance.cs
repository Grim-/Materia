using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Materia
{
    public class MateriaInstance : IExposable
    {
        public MateriaDef def;
        private List<MateriaWorker> activeEffects = new List<MateriaWorker>();

        public List<MateriaWorker> ActiveEffects => activeEffects.ToList();


        protected MateriaSlot ParentSlot;
        protected ThingWithComps ParentThing;
        protected Comp_Materia ParentMateriaComp;

        public MateriaInstance()
        {

        }

        public MateriaInstance(MateriaDef def, Comp_Materia materiaComp, MateriaSlot parentSlot)
        {
            this.def = def;
            this.ParentMateriaComp = materiaComp;
            this.ParentSlot = parentSlot;
            this.ParentThing = this.ParentSlot.ParentThing;
            InitializeEffects();
        }


        private void InitializeEffects()
        {
            if (def.effects != null)
            {
                foreach (var effectDef in def.effects)
                {
                    var worker = effectDef.CreateWorker(ParentThing, ParentMateriaComp, ParentSlot);
                    activeEffects.Add(worker);
                    worker.Notify_MateriaEquipped();
                }
            }
        }

        public List<T> GetEffectsOfType<T>() where T: MateriaWorker
        {
            List<T> newList = new List<T>();

            foreach (var effect in activeEffects)
            {
                if (effect is T asT)
                {
                    newList.Add(asT);
                }
            }
            return newList;
        }
        public List<T> GetEffectsOfDef<T>(MateriaEffectDef materiaEffectDef) where T : MateriaWorker
        {
            List<T> newList = new List<T>();

            foreach (var effectDef in activeEffects)
            {
                if (effectDef.def == materiaEffectDef && effectDef is T asT)
                {
                    newList.Add(asT);
                }
            }
            return newList;
        }


        public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var effect in activeEffects)
            {
                foreach (var item in effect.CompGetGizmosExtra())
                {
                    yield return item;

                }
            }
        }
        public virtual void Notify_MateriaEquipped()
        {
            foreach (var effect in activeEffects)
            {
                effect.Notify_MateriaEquipped();
            }
        }

        public virtual void Notify_MateriaUnequipped()
        {
            foreach (var effect in activeEffects)
            {
                effect.Notify_MateriaUnequipped();
            }
        }

        public void TickMateria(Pawn pawn)
        {
            foreach (var effect in activeEffects)
            {
                effect.OnTick(pawn);
            }
        }
        public virtual void Notify_KilledPawn(Pawn pawn)
        {
            foreach (var effect in activeEffects)
            {
                effect.Notify_KilledPawn(pawn);
            }
        }
        public virtual void Notify_ProjectileImpact(Pawn Attacker, Thing Target, Projectile Projectile)
        {
            foreach (var effect in activeEffects)
            {
                effect.Notify_ProjectileImpact(Attacker, Target, Projectile);
            }
        }

        public virtual bool Notify_PostPreApplyDamage(ref DamageInfo dinfo)
        {
            bool wasAbsorbed = false;
            foreach (var effect in activeEffects)
            {
                if (effect.Notify_PostPreApplyDamage(ref dinfo))
                {
                    wasAbsorbed = true;
                }       
            }
            return wasAbsorbed;
        }

        public virtual DamageInfo Notify_ProjectileApplyDamageToTarget(DamageInfo Damage, Pawn Attacker, Thing Target, Projectile Projectile)
        {
            DamageInfo damageInfo = new DamageInfo(Damage);
            foreach (var effect in activeEffects)
            {
                damageInfo = effect.Notify_ProjectileApplyDamageToTarget(damageInfo, Attacker, Target, Projectile);
            }
            return damageInfo;
        }

        public virtual void Notify_OwnerKilled()
        {
            foreach (var effect in activeEffects)
            {
                effect.Notify_OwnerKilled();
            }
        }

        public virtual void Notify_OwnerOnMoodChange(Pawn pawn)
        {

        }

        public virtual void Notify_OwnerThoughtGained(Thought Thought, Pawn otherPawn)
        {
            foreach (var effect in activeEffects)
            {
                effect.Notify_OwnerThoughtGained(Thought, otherPawn);
            }
        }

        public virtual void Notify_OwnerThoughtLost(Thought Thought)
        {
            foreach (var effect in activeEffects)
            {
                effect.Notify_OwnerThoughtLost(Thought);
            }
        }

        public virtual void Notify_OwnerHediffGained(Hediff Hediff, BodyPartRecord partRecord, DamageInfo? dinfo, DamageWorker.DamageResult damageResult)
        {
            foreach (var effect in activeEffects)
            {
                effect.Notify_OwnerHediffGained(Hediff, partRecord, dinfo, damageResult);
            }
        }
        public virtual void Notify_OwnerHediffRemoved(Hediff Hediff)
        {
            foreach (var effect in activeEffects)
            {
                effect.Notify_OwnerHediffRemoved(Hediff);
            }
        }
        public void Notify_Equipped(Pawn pawn)
        {
            foreach (var effect in activeEffects)
            {
                effect.Notify_Equipped(pawn);
            }
        }
        public void Notify_Unequipped(Pawn pawn)
        {
            foreach (var effect in activeEffects)
            {
                effect.Notify_Unequipped(pawn);
            }
        }

        public void Notify_ParentDestroyed()
        {
            foreach (var effect in activeEffects)
            {
                effect.Notify_ParentDestroyed();
            }
        }
        public void Notify_ParentDespawned()
        {
            foreach (var effect in activeEffects)
            {
                effect.Notify_ParentDespawned();
            }
        }

        public void PostDraw()
        {
            foreach (var effect in activeEffects)
            {
                effect.PostDraw();
            }
        }

        public virtual DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult DamageWorkerResult)
        {
            DamageWorker.DamageResult damageResult = DamageWorkerResult;
            foreach (var effect in activeEffects)
            {
                damageResult = effect.Notify_ApplyMeleeDamageToTarget(target, Attacker, DamageWorkerResult);
            }

            return damageResult;
        }



        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Collections.Look(ref activeEffects, "activeEffects", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (activeEffects == null)
                {
                    InitializeEffects();
                }
            }
        }
    }



}

using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Thor
{
    public class MateriaInstance : IExposable
    {
        public MateriaDef def;
        private List<MateriaWorker> activeEffects = new List<MateriaWorker>();

        public List<MateriaWorker> ActiveEffects => activeEffects.ToList();


        protected MateriaSlot ParentSlot;
        protected ThingWithComps ParentThing;

        public MateriaInstance()
        {

        }

        public MateriaInstance(MateriaDef def, MateriaSlot parentSlot)
        {
            this.def = def;
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
                    var worker = effectDef.CreateWorker();
                    worker.def = effectDef;
                    worker.ParentEquipment = ParentThing;
                    activeEffects.Add(worker);
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

        public virtual DamageInfo Notify_ProjectileApplyDamageToTarget(DamageInfo Damage, Pawn Attacker, Thing Target, Projectile Projectile)
        {
            DamageInfo damageInfo = new DamageInfo(Damage);
            foreach (var effect in activeEffects)
            {
                damageInfo = effect.Notify_ProjectileApplyDamageToTarget(damageInfo, Attacker, Target, Projectile);
            }
            return damageInfo;
        }

        public virtual void Notify_OwnerKilled(Map prevMap, DamageInfo? dinfo = null)
        {
            foreach (var effect in activeEffects)
            {
                effect.Notify_OwnerKilled(prevMap, dinfo);
            }
        }

        public virtual void Notify_OwnerOnMoodChange(Pawn pawn)
        {

        }

        public virtual void Notify_OwnerOnThoughtGained(Pawn pawn)
        {
      
        }


        public virtual void Notify_OwnerOnHediffAdded(Pawn pawn)
        {
  
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

        public void Notify_MateriaLinked(MateriaDef sourceMateria, MateriaDef linkedMateria)
        {
            var linkBonus = def.links?.FirstOrDefault(x => x.materia == linkedMateria);
            if (linkBonus?.effects != null)
            {
                foreach (var effectDef in linkBonus.effects)
                {
                    var worker = effectDef.CreateWorker();
                    worker.def = effectDef;
                    activeEffects.Add(worker);
                }
            }
        }
        public void Notify_MateriaUnlinked(MateriaDef sourceMateria, MateriaDef linkedMateria)
        {
            activeEffects.RemoveAll(effect =>
                def.links?.Any(link =>
                    link.materia == linkedMateria &&
                    link.effects?.Any(e => e == effect.def) == true) == true);
        }
    }



}

using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Thor
{
    public class MateriaInstance : IExposable
    {
        public MateriaDef def;
        private List<MateriaWorker> activeEffects = new List<MateriaWorker>();

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

        public void Notify_Equipped(Pawn pawn)
        {
            foreach (var effect in activeEffects)
            {
                effect.Notify_Equipped(pawn);
            }
        }

        public virtual DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, DamageWorker.DamageResult DamageWorkerResult)
        {
            DamageWorker.DamageResult damageResult = DamageWorkerResult;
            foreach (var effect in activeEffects)
            {
                damageResult = effect.Notify_ApplyMeleeDamageToTarget(target, DamageWorkerResult);
            }

            return damageResult;
        }

        public void Notify_Unequipped(Pawn pawn)
        {
            foreach (var effect in activeEffects)
            {
                effect.Notify_Unequipped(pawn);
            }
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

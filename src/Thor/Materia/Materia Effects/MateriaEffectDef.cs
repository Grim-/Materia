using RimWorld;
using System;
using Verse;

namespace Materia
{
    public class MateriaEffectDef : Def
    {
        public Type workerClass = typeof(MateriaWorker);

        public virtual string EffectDescription { get; private set; } = "";


        public MateriaWorker CreateWorker(MateriaEffectDef effectDef, ThingWithComps parentEquipment)
        {
            MateriaWorker newWorker = (MateriaWorker)Activator.CreateInstance(workerClass);
            newWorker.def = this;
            newWorker.ParentEquipment = parentEquipment;
            return newWorker;
        }
    }
}

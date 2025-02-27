using RimWorld;
using System;
using Verse;

namespace Materia
{
    public class MateriaEffectDef : Def
    {
        public Type workerClass = typeof(MateriaWorker);

        public virtual string EffectDescription { get; private set; } = "";


        public MateriaWorker CreateWorker(ThingWithComps parentEquipment, Comp_Materia materiaComp, MateriaSlot materiaSlot)
        {
            MateriaWorker newWorker = (MateriaWorker)Activator.CreateInstance(workerClass);
            newWorker.def = this;
            newWorker.ParentEquipment = parentEquipment;
            newWorker.MateriaComp = materiaComp;
            newWorker.MateriaSlot = materiaSlot;
            return newWorker;
        }
    }
}

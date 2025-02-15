using RimWorld;
using System;
using Verse;

namespace Thor
{
    public class MateriaEffectDef : Def
    {
        public Type workerClass = typeof(MateriaWorker);


        public MateriaWorker CreateWorker()
        {
            MateriaWorker newWorker = (MateriaWorker)Activator.CreateInstance(workerClass);
            return newWorker;
        }
    }
}

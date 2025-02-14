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

    //public class MateriaEffect
    //{
    //    public string label;
    //    public string description;
    //    public Type workerClass;
    //}

    //public class MateriaEffect_Gene : MateriaEffect
    //{
    //    public GeneDef gene;
    //    public bool isXenogene = true;
    //}

    //public class MateriaEffect_Ability : MateriaEffect
    //{
    //    public AbilityDef ability;
    //}

    //public class MateriaEffect_DamageOnHit : MateriaEffect
    //{
    //    public float damageMultiplier = 3f;
    //    public DamageDef damageType;
    //    public EffecterDef effecter;
    //    public EffecterDef onhitEffecter;
    //}

}

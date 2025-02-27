using System.Collections.Generic;
using Verse;

namespace Materia
{
    public class MateriaEffectDef_OnDamageTaken : MateriaEffectDef
    {

        public float absorbAmount = 0;
        public DamageDef damageType;
        public bool isModifier = false;


        public MateriaEffectDef_OnDamageTaken()
        {
            workerClass = typeof(MateriaEffect_OnDamageTaken);
        }
    }

    public class MateriaEffect_OnDamageTaken : MateriaWorker
    {
        MateriaEffectDef_OnDamageTaken Def => (MateriaEffectDef_OnDamageTaken)def;

        public override bool Notify_PostPreApplyDamage(ref DamageInfo dinfo)
        {
            bool wasAbsorbed = base.Notify_PostPreApplyDamage(ref dinfo);
            Log.Message($"damage {dinfo.Amount} {dinfo.Def}");

            if (CalculateAbsorbption(ref dinfo))
            {
                wasAbsorbed = true;
            }

            return wasAbsorbed;
        }


        private bool CalculateAbsorbption(ref DamageInfo originalDamage)
        {
            float amountAbsorbed = 0;
            if (Def.isModifier)
            {
                amountAbsorbed = originalDamage.Amount * Def.absorbAmount;
            }
            else
            {
                amountAbsorbed = originalDamage.Amount - Def.absorbAmount;
            }

            float damageAfter = originalDamage.Amount - amountAbsorbed;
            originalDamage.SetAmount(damageAfter);

            if (damageAfter <= 0)
            {
                damageAfter = 0;
                return true;
            }

            return false;
        }

    }


}
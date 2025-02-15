using RimWorld;
using UnityEngine;
using Verse;

namespace Thor
{
    public class MateriaEffectDef_HealOnHit : MateriaEffectDef
    {
        public float healFactor = 0.2f;
        public float minimumDamageToHeal = 5f;
    }

    public class MateriaEffect_HealOnHit : MateriaWorker
    {
        MateriaEffectDef_HealOnHit Def => (MateriaEffectDef_HealOnHit)def;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult damageResult)
        {
            if (damageResult?.totalDamageDealt >= Def.minimumDamageToHeal && Attacker != null)
            {
                float healAmount = damageResult.totalDamageDealt * Def.healFactor;
                if (HealthUtility.TryGetWorstHealthCondition(Attacker, out Hediff hediff, out BodyPartRecord part, null))
                {
                    HealthUtility.AdjustSeverity(Attacker, hediff.def, healAmount);
                }

                MoteMaker.ThrowText(Attacker.DrawPos, Attacker.Map, "+" + healAmount.ToString("F1"), Color.green);
            }
            return damageResult;
        }
    }

}
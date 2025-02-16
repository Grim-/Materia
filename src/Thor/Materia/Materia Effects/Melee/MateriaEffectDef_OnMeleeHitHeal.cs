using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Thor
{
    public class MateriaEffectDef_OnMeleeHitHeal : MateriaEffectDef
    {
        public float healAmount = 3f;
        public bool isPercentage = false;
        public float minimumDamageToHeal = 5f;

        public MateriaEffectDef_OnMeleeHitHeal()
        {
            workerClass = typeof(MateriaEffect_OnMeleeHitHeal);
        }

        public override string EffectDescription
        {
            get
            {
                string healText = isPercentage ? healAmount + "%" : healAmount.ToString();
                return string.Format("Heals the Attacker for {0} {1} while equipped",
                    healText,
                    isPercentage ? "of melee damage dealt" : "health");
            }
        }


    }

    public class MateriaEffect_OnMeleeHitHeal : MateriaWorker
    {
        MateriaEffectDef_OnMeleeHitHeal Def => (MateriaEffectDef_OnMeleeHitHeal)def;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult damageResult)
        {
            if (damageResult?.totalDamageDealt > 0 && Attacker != null)
            {
                float healAmount = Def.isPercentage
                    ? damageResult.totalDamageDealt * (Def.healAmount / 100f)
                    : Def.healAmount;

                healAmount = Mathf.Clamp(healAmount, Def.minimumDamageToHeal, 1000f);

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
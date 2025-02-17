using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Thor
{
    public class MateriaEffectDef_OnHitHeal : MateriaEffectDef
    {
        public float healAmount = 3f;
        public bool isPercentage = false;
        public float minimumDamageToHeal = 5f;
        public OnHitHealMode healMode = OnHitHealMode.ON_MELEE;

        public MateriaEffectDef_OnHitHeal()
        {
            workerClass = typeof(MateriaEffect_OnHitHeal);
        }

        public override string EffectDescription
        {
            get
            {
                string healText = isPercentage ? healAmount * 100f + "%" : healAmount.ToString();
                return string.Format("Heals the Attacker for {0} {1} while equipped",
                    healText,
                    isPercentage ? "of melee damage dealt" : "health");
            }
        }


    }

    public enum OnHitHealMode
    {
        ON_MELEE,
        ON_RANGE,
        BOTH
    }

    public class MateriaEffect_OnHitHeal : MateriaWorker
    {
        MateriaEffectDef_OnHitHeal Def => (MateriaEffectDef_OnHitHeal)def;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult damageResult)
        {
            if (damageResult?.totalDamageDealt > 0 && Attacker != null && Def.healMode == OnHitHealMode.ON_MELEE || Def.healMode == OnHitHealMode.BOTH)
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


        public override DamageInfo Notify_ProjectileApplyDamageToTarget(DamageInfo Damage, Pawn Attacker, Thing Target, Projectile Projectile)
        {
            DamageInfo damage = base.Notify_ProjectileApplyDamageToTarget(Damage, Attacker, Target, Projectile);


            if (damage.Amount > 0 && Attacker != null && Def.healMode == OnHitHealMode.ON_RANGE || Def.healMode == OnHitHealMode.BOTH)
            {
                float healAmount = Def.isPercentage
                ? damage.Amount * (Def.healAmount / 100f)
                : Def.healAmount;

                healAmount = Mathf.Clamp(healAmount, Def.minimumDamageToHeal, 1000f);

                if (HealthUtility.TryGetWorstHealthCondition(Attacker, out Hediff hediff, out BodyPartRecord part, null))
                {
                    HealthUtility.AdjustSeverity(Attacker, hediff.def, healAmount);
                }
                MoteMaker.ThrowText(Attacker.DrawPos, Attacker.Map, "+" + healAmount.ToString("F1"), Color.green);
            }

            return damage;
        }
    }


}
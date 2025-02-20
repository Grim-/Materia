using RimWorld;
using UnityEngine;
using Verse;

namespace Materia
{
    public class MateriaEffectDef_OnProjectileHitHeal : MateriaEffectDef
    {
        public float healAmount = 3f;
        public bool isPercentage = false;
        public float minimumDamageToHeal = 5f;

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

        public MateriaEffectDef_OnProjectileHitHeal()
        {
            workerClass = typeof(MateriaEffect_OnProjectileHitHeal);
        }
    }

    public class MateriaEffect_OnProjectileHitHeal : MateriaWorker
    {
        MateriaEffectDef_OnProjectileHitHeal Def => (MateriaEffectDef_OnProjectileHitHeal)def;

        public override DamageInfo Notify_ProjectileApplyDamageToTarget(DamageInfo Damage, Pawn Attacker, Thing Target, Projectile Projectile)
        {
            DamageInfo damage = base.Notify_ProjectileApplyDamageToTarget(Damage, Attacker, Target, Projectile);

            if (Attacker != null && Target != null)
            {
                float healAmount = Def.isPercentage
                ? Damage.Amount * (Def.healAmount / 100f)
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
using RimWorld;
using UnityEngine;
using Verse;

namespace Materia
{
    public class MateriaEffectDef_OnHitBane : MateriaEffectDef_OnHitDamage
    {
        public ThingDef race;
        public override string EffectDescription
        {
            get
            {
                string damageText = isMultiplicative ? damageValue + "x" : damageValue.ToString();
                return string.Format("Deal {0} ({1}) damage on a successful {3} attack against {2} targets.", damageText, damageType.LabelCap, race.LabelCap, attackType);
            }
        }

        public MateriaEffectDef_OnHitBane()
        {
            workerClass = typeof(MateriaEffect_OnHitBane);
        }
    }


    public class MateriaEffect_OnHitBane : MateriaWorker
    {
        MateriaEffectDef_OnHitBane Def => (MateriaEffectDef_OnHitBane)def;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult damageResult)
        {

            if (Def.hitMode == OnHitMode.Melee)
            {
                if (target.Pawn != null && target.Pawn.def == Def.race)
                {
                    float extraDamage = Def.isMultiplicative
                               ? damageResult.totalDamageDealt * (Def.damageValue - 1f)
                               : Def.damageValue;
                    target.Thing.TakeDamage(new DamageInfo(Def.damageType, extraDamage));
                    MoteMaker.ThrowText(Attacker.DrawPos, Attacker.Map, $"Bane! {Def.race.LabelCap} " + extraDamage.ToString("F1"), Color.red);
                }
            }


            return damageResult;
        }

        public override DamageInfo Notify_ProjectileApplyDamageToTarget(DamageInfo Damage, Pawn Attacker, Thing Target, Projectile Projectile)
        {
            DamageInfo damage = base.Notify_ProjectileApplyDamageToTarget(Damage, Attacker, Target, Projectile);

            if (Def.hitMode == OnHitMode.Range)
            {
                if (Attacker != null && Target != null && Target.def == Def.race)
                {
                    float extraDamage = Def.isMultiplicative
                        ? Damage.Amount * (Def.damageValue - 1f)
                        : Def.damageValue;

                    Target.TakeDamage(new DamageInfo(Def.damageType, extraDamage));
                    MoteMaker.ThrowText(Attacker.DrawPos, Attacker.Map, $"Bane! {Def.race.LabelCap} " + extraDamage.ToString("F1"), Color.red);
                }
            }
            return damage;
        }
    }


}
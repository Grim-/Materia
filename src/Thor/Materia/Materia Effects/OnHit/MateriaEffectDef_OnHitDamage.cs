using RimWorld;
using Verse;

namespace Materia
{
    public class MateriaEffectDef_OnHitDamage : MateriaEffectDef_OnHitBase
    {
        public float damageValue = 3f;
        public bool isMultiplicative = false;
        public DamageDef damageType;
        public EffecterDef effecter;
        public EffecterDef onhitEffecter;

        public override string EffectDescription
        {
            get
            {
                string damageText = isMultiplicative ? damageValue + "x" : damageValue.ToString();
                return string.Format("Deal {0} ({1}) damage on a successful melee attack.", damageText, damageType.LabelCap);
            }
        }

        public MateriaEffectDef_OnHitDamage()
        {
            workerClass = typeof(MateriaEffect_OnHitDamage);
        }
    }

    public class MateriaEffect_OnHitDamage : MateriaWorker
    {
        MateriaEffectDef_OnHitDamage Def => (MateriaEffectDef_OnHitDamage)def;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult damageResult)
        {

            if (Def.hitMode  == OnHitMode.Melee)
            {
                if (damageResult?.totalDamageDealt > 0 && target.Thing != null)
                {
                    Thing targetThing = target.Thing;
                    IntVec3 hitPosition = targetThing.Position;


                    float extraDamage = Def.isMultiplicative
                        ? damageResult.totalDamageDealt * (Def.damageValue - 1f)
                        : Def.damageValue;

                    Log.Message($"Notify Apply Melee Damage To Target amount {extraDamage} type {Def.damageType}");
                    target.Thing.TakeDamage(new DamageInfo(Def.damageType, extraDamage));

                    if (Def.onhitEffecter != null)
                    {
                        var effect = Def.onhitEffecter.Spawn(target.Thing, target.Thing.Map);
                        effect.Trigger(new TargetInfo(hitPosition, Attacker.Map), new TargetInfo(hitPosition, Attacker.Map));
                    }
                }
            }


            return damageResult;
        }


        public override DamageInfo Notify_ProjectileApplyDamageToTarget(DamageInfo Damage, Pawn Attacker, Thing Target, Projectile Projectile)
        {
            DamageInfo damage = base.Notify_ProjectileApplyDamageToTarget(Damage, Attacker, Target, Projectile);

            if (Def.hitMode == OnHitMode.Range)
            {
                if (Damage.Amount > 0 && Target != null)
                {
                    float extraDamage = Def.isMultiplicative
                        ? Damage.Amount * (Def.damageValue - 1f)
                        : Def.damageValue;

                    Log.Message($"Notify Apply Projectile Damage To Target amount {extraDamage} type {Def.damageType}");
                    Target.TakeDamage(new DamageInfo(Def.damageType, extraDamage));

                    if (Def.onhitEffecter != null)
                    {
                        var effect = Def.onhitEffecter.Spawn(Target, Target.Map);
                        effect.Trigger(Target, Target);
                    }
                }
            }
            return damage;
        }
    }
}
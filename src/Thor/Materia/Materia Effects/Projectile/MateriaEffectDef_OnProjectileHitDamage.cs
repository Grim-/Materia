using Verse;

namespace Materia
{
    public class MateriaEffectDef_OnProjectileHitDamage : MateriaEffectDef
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

        public MateriaEffectDef_OnProjectileHitDamage()
        {
            workerClass = typeof(MateriaEffect_OnProjectileHitDamage);
        }
    }

    public class MateriaEffect_OnProjectileHitDamage : MateriaWorker
    {
        MateriaEffectDef_OnProjectileHitDamage Def => (MateriaEffectDef_OnProjectileHitDamage)def;

        public override DamageInfo Notify_ProjectileApplyDamageToTarget(DamageInfo Damage, Pawn Attacker, Thing Target, Projectile Projectile)
        {
            DamageInfo damage = base.Notify_ProjectileApplyDamageToTarget(Damage, Attacker, Target, Projectile);

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

            return damage;
        }
    }
}
using RimWorld;
using UnityEngine;
using Verse;

namespace Thor
{
    public class MateriaEffectDef_OnProjectileHitBane : MateriaEffectDef
    {
        public float damage = 1f;
        public DamageDef damageType;
        public ThingDef race;

        public MateriaEffectDef_OnProjectileHitBane()
        {
            workerClass = typeof(MateriaEffect_OnProjectileHitBane);
        }

        public override string EffectDescription => $"Deal {damage} ({damageType.LabelCap}) to targets of {race.LabelCap} race.";


    }

    public class MateriaEffect_OnProjectileHitBane : MateriaWorker
    {
        MateriaEffectDef_OnProjectileHitBane Def => (MateriaEffectDef_OnProjectileHitBane)def;

        public override DamageInfo Notify_ProjectileApplyDamageToTarget(DamageInfo Damage, Pawn Attacker, Thing Target, Projectile Projectile)
        {
            DamageInfo damage = base.Notify_ProjectileApplyDamageToTarget(Damage, Attacker, Target, Projectile);

            if (Attacker != null & Target != null)
            {
                float extraDamage = Def.damage;
                Target.TakeDamage(new DamageInfo(Def.damageType, extraDamage));
                MoteMaker.ThrowText(Attacker.DrawPos, Attacker.Map, $"Bane {Def.race.LabelCap} " + extraDamage.ToString("F1"), Color.red);
            }


            return damage;
        }
    }

}
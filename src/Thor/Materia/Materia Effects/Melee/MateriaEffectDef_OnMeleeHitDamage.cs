using UnityEngine;
using Verse;

namespace Thor
{
    public class MateriaEffectDef_OnMeleeHitDamage : MateriaEffectDef
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

        public MateriaEffectDef_OnMeleeHitDamage()
        {
            workerClass = typeof(MateriaEffect_OnMeleeHitDamage);
        }
    }

    public class MateriaEffect_OnMeleeHitDamage : MateriaWorker
    {
        MateriaEffectDef_OnMeleeHitDamage Def => (MateriaEffectDef_OnMeleeHitDamage)def;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult damageResult)
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
            return damageResult;
        }
    }

}
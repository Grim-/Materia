using UnityEngine;
using Verse;

namespace Thor
{
    public class MateriaEffectDef_ExtraDamage : MateriaEffectDef
    {
        public float damageMultiplier = 3f;
        public DamageDef damageType;

        public EffecterDef effecter;
        public EffecterDef onhitEffecter;
    }

    public class MateriaEffect_ExtraDamage : MateriaWorker
    {
        MateriaEffectDef_ExtraDamage Def => (MateriaEffectDef_ExtraDamage)def;
        private Effecter effect;

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            Log.Message("MateriaEffect_Notify_Equipped");
            //if (effect == null && ParentEquipment != null && Def.effecter != null)
            //{
            //    // Using a known working effect for testing
            //    effect = Def.effecter.Spawn();
            //    Log.Message("Spawning effect on weapon");
            //    effect.Trigger(new TargetInfo(ParentEquipment), new TargetInfo(ParentEquipment));
            //}
        }

        public override void OnTick(Pawn pawn)
        {
            base.OnTick(pawn);

            //if (effect != null && ParentEquipment != null && Find.TickManager.TicksGame % 10 == 0)
            //{
            //    effect.EffectTick(ParentEquipment, ParentEquipment);
            //}
        }

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult damageResult)
        {
            if (damageResult?.totalDamageDealt > 0 && target.Thing != null)
            {
                float extraDamage = damageResult.totalDamageDealt * (Def.damageMultiplier - 1f);
                Log.Message($"Notify Apply Melee Damage To Target amount {extraDamage} type {Def.damageType}");
                target.Thing.TakeDamage(new DamageInfo(Def.damageType, extraDamage));

                if (Def.onhitEffecter != null)
                {
                   var effect = Def.onhitEffecter.Spawn(target.Thing, target.Thing.Map);
                    effect.Trigger(target.Thing, target.Thing);
                }
            }
            return damageResult;
        }
    }
}
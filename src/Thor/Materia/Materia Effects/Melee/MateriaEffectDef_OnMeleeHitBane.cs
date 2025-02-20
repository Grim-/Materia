using RimWorld;
using UnityEngine;
using Verse;

namespace Materia
{
    public class MateriaEffectDef_OnMeleeHitBane : MateriaEffectDef
    {
        public float damage =1f;
        public DamageDef damageType;
        public ThingDef race;
        public MateriaEffectDef_OnMeleeHitBane()
        {
            workerClass = typeof(MateriaEffect_OnMeleeHitBane);
        }
        public override string EffectDescription => $"Deal {damage} ({damageType.LabelCap}) to targets of {race.LabelCap} race.";
    }


    public class MateriaEffect_OnMeleeHitBane : MateriaWorker
    {
        MateriaEffectDef_OnMeleeHitBane Def => (MateriaEffectDef_OnMeleeHitBane)def;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult damageResult)
        {
            if (target.Pawn != null && target.Pawn.def == Def.race)
            {
                float extraDamage = Def.damage;
                target.Thing.TakeDamage(new DamageInfo(Def.damageType, extraDamage));
                MoteMaker.ThrowText(Attacker.DrawPos, Attacker.Map, $"Bane {Def.race.LabelCap} " + extraDamage.ToString("F1"), Color.red);
            }
            return damageResult;
        }
    }



}
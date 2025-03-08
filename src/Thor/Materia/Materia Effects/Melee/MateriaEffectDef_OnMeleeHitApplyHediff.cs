using RimWorld;
using System.Linq;
using Verse;

namespace Materia
{
    public class MateriaEffectDef_OnMeleeHitApplyHediff : MateriaEffectDef
    {
        public HediffDef hediff;
        public float chance = 1f;
        public bool applyToSelf = false;
        public bool hostileOnly = true;


        public bool aoe = false;
        public float aoeRadius = 4f;

        public override string EffectDescription
        {
            get
            {
                string targetString = "";
                if (aoe)
                {
                    targetString = applyToSelf ? "yourself and nearby allies" : "enemies within a radius of " + aoeRadius;
                }
                else
                {
                    targetString = applyToSelf ? "yourself" : "the target";
                }

                string conditionString = "";
                if (hostileOnly)
                {
                    conditionString = " only on hostile targets";
                }

                return $"{chance * 100}% chance on successful melee attack{conditionString} to apply {hediff.LabelCap} to {targetString}";
            }
        }

        public MateriaEffectDef_OnMeleeHitApplyHediff()
        {
            workerClass = typeof(MateriaEffect_OnMeleeHitApplyHediff);
        }
    }


    public class MateriaEffect_OnMeleeHitApplyHediff : MateriaWorker
    {
        MateriaEffectDef_OnMeleeHitApplyHediff Def => (MateriaEffectDef_OnMeleeHitApplyHediff)def;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn attacker, DamageWorker.DamageResult damageResult)
        {
            if (attacker != null && target.Pawn != null && Def.hediff != null && Rand.Value <= Def.chance)
            {
                if (Def.aoe)
                {
                    var targets = GenRadial.RadialDistinctThingsAround(target.Pawn.Position, target.Pawn.Map, Def.aoeRadius, true);
                    foreach (Thing thing in targets)
                    {
                        if (thing is Pawn pawn && (!Def.hostileOnly || pawn.Faction.HostileTo(attacker.Faction)))
                        {
                            pawn.health.GetOrAddHediff(Def.hediff);
                        }
                    }
                }
                else if (Def.applyToSelf)
                {
                    attacker.health.GetOrAddHediff(Def.hediff);
                }
                else if (!Def.hostileOnly || target.Pawn.Faction.HostileTo(attacker.Faction))
                {
                    target.Pawn.health.GetOrAddHediff(Def.hediff);
                }
            }
            return damageResult;
        }

    }
}
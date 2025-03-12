using RimWorld;
using System.Linq;
using Verse;

namespace Materia
{
    public class MateriaEffectDef_OnHitApplyHediff : MateriaEffectDef_OnHitBase
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



                return $"{chance * 100}% chance on successful {attackType} attack{conditionString} to apply {hediff.LabelCap} to {targetString}";
            }
        }

        public MateriaEffectDef_OnHitApplyHediff()
        {
            workerClass = typeof(MateriaEffect_OnHitApplyHediff);
        }
    }


    public class MateriaEffect_OnHitApplyHediff : MateriaWorker
    {
        MateriaEffectDef_OnHitApplyHediff Def => (MateriaEffectDef_OnHitApplyHediff)def;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn attacker, DamageWorker.DamageResult damageResult)
        {
            if (Def.hitMode == OnHitMode.Melee)
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
            }


            return damageResult;
        }


        public override DamageInfo Notify_ProjectileApplyDamageToTarget(DamageInfo Damage, Pawn Attacker, Thing Target, Projectile Projectile)
        {
            DamageInfo damage = base.Notify_ProjectileApplyDamageToTarget(Damage, Attacker, Target, Projectile);

            if (Def.hitMode == OnHitMode.Range && Attacker != null && Target != null && Target is Pawn TargetPawn)
            {
                if (Def.aoe)
                {
                    var targets = GenRadial.RadialDistinctThingsAround(TargetPawn.Position, TargetPawn.Map, Def.aoeRadius, true);
                    foreach (Thing thing in targets)
                    {
                        if (thing is Pawn pawn && (!Def.hostileOnly || pawn.Faction.HostileTo(Attacker.Faction)))
                        {
                            pawn.health.GetOrAddHediff(Def.hediff);
                        }
                    }
                }
                else if (Def.applyToSelf)
                {
                    Attacker.health.GetOrAddHediff(Def.hediff);
                }
                else if (!Def.hostileOnly || TargetPawn.Faction.HostileTo(Attacker.Faction))
                {
                    TargetPawn.health.GetOrAddHediff(Def.hediff);
                }
            }

            return damage;
        }
    }
}
using RimWorld;
using Verse;

namespace Materia
{
    public class MateriaEffectDef_OnProjectileHitApplyHediff : MateriaEffectDef
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

                return $"{chance * 100}% chance on successful ranged attack{conditionString} to apply {hediff.LabelCap} to {targetString}";
            }
        }

        public MateriaEffectDef_OnProjectileHitApplyHediff()
        {
            workerClass = typeof(MateriaEffect_OnProjectileHitApplyHediff);
        }

    }

    public class MateriaEffect_OnProjectileHitApplyHediff : MateriaWorker
    {
        MateriaEffectDef_OnMeleeHitApplyHediff Def => (MateriaEffectDef_OnMeleeHitApplyHediff)def;

        public override DamageInfo Notify_ProjectileApplyDamageToTarget(DamageInfo Damage, Pawn Attacker, Thing Target, Projectile Projectile)
        {
            DamageInfo damage = base.Notify_ProjectileApplyDamageToTarget(Damage, Attacker, Target, Projectile);

            if (Attacker != null && Target != null && Target is Pawn TargetPawn)
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
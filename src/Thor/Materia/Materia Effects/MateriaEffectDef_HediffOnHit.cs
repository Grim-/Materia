using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Thor
{
    public class MateriaEffectDef_HediffOnHit : MateriaEffectDef
    {
        public HediffDef hediff;
        public float chance = 1f;
        public bool applyToSelf = false;
        public bool hostileOnly = true;


        public bool aoe = false;
        public float aoeRadius = 4f;
    }

    public class MateriaEffect_HediffOnHit : MateriaWorker
    {
        MateriaEffectDef_HediffOnHit Def => (MateriaEffectDef_HediffOnHit)def;

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
using RimWorld;
using Verse;

namespace Thor
{
    public class MateriaEffectDef_HediffOnHit : MateriaEffectDef
    {
        public HediffDef hediff;
        public float chance = 1f;
        public bool applyToSelf = false;
    }

    public class MateriaEffect_HediffOnHit : MateriaWorker
    {
        MateriaEffectDef_HediffOnHit Def => (MateriaEffectDef_HediffOnHit)def;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult damageResult)
        {
            if (Attacker != null && target.Pawn != null && Def.hediff != null && Rand.Value <= Def.chance)
            {
                if (Def.applyToSelf)
                {
                    Attacker.health.GetOrAddHediff(Def.hediff);
                }
                else
                {
                    target.Pawn.health.GetOrAddHediff(Def.hediff);
                }
            }
            return damageResult;
        }
    }

    public class HediffCompProperties_DamageOverTime : HediffCompProperties
    {
        public int damagePerInterval = 1;
        public int intervalTicks = 300;
        public DamageDef damageType;
        public float severityPerDamage = -0.1f;

        public HediffCompProperties_DamageOverTime()
        {
            compClass = typeof(HediffComp_DamageOverTime);
        }
    }

    public class HediffComp_DamageOverTime : HediffComp
    {
        private int ticksUntilDamage;

        public HediffCompProperties_DamageOverTime Props => (HediffCompProperties_DamageOverTime)props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            ResetInterval();
        }

        private void ResetInterval()
        {
            ticksUntilDamage = Props.intervalTicks;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            ticksUntilDamage--;
            if (ticksUntilDamage <= 0)
            {
                DealDamage();
                ResetInterval();
            }
        }

        private void DealDamage()
        {
            if (parent.pawn.Dead) return;

            DamageInfo damageInfo = new DamageInfo(
                Props.damageType != null? Props.damageType : DamageDefOf.AcidBurn, 
                Props.damagePerInterval,
                0f,
                -1f,
                null,
                parent.Part
            );

            parent.pawn.TakeDamage(damageInfo);
            parent.Severity += Props.severityPerDamage;
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref ticksUntilDamage, "ticksUntilDamage");
        }
    }
}
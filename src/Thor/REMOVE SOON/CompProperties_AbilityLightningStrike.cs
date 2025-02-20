using RimWorld;
using Verse;

namespace Materia
{
    public class CompProperties_AbilityLightningStrike : CompProperties_AbilityEffect
    {
        public bool lightning = true;
        public float explosionRadius = 3f;
        public int explosionDamage = 50;
        public SoundDef soundOnImpact;

        public CompProperties_AbilityLightningStrike()
        {
            compClass = typeof(CompAbilityEffect_LightningStrike);
        }
    }

    public class CompAbilityEffect_LightningStrike : CompAbilityEffect
    {
        private LightningRingBehavior lightningBehavior;
        new CompProperties_LightningRing Props => (CompProperties_LightningRing)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (parent.pawn?.Map == null) 
                return;

            lightningBehavior = new LightningRingBehavior(
                Props.rings,
                target.Cell,
                parent.pawn.Map,
                Props.delayTicks
            );
        }

        public override void CompTick()
        {
            base.CompTick();

            if (lightningBehavior != null)
            {
                lightningBehavior?.Tick();

                if (lightningBehavior.IsFinished)
                {
                    lightningBehavior = null;
                }
            }      
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            return parent.pawn?.Map != null && base.Valid(target, throwMessages);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Deep.Look(ref lightningBehavior, "lightningTicker");
        }
    }


}

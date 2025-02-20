using RimWorld;
using Verse;

namespace Materia
{
    public class CompProperties_ChainLightning : CompProperties_AbilityEffect
    {
        public bool lightning = true;
        public float explosionRadius = 3f;
        public int explosionDamage = 50;
        public SoundDef soundOnImpact;

        public CompProperties_ChainLightning()
        {
            compClass = typeof(CompAbilityEffect_ChainLightning);
        }
    }

    public class CompAbilityEffect_ChainLightning : CompAbilityEffect
    {
        new CompProperties_ChainLightning Props => (CompProperties_ChainLightning)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (parent.pawn?.Map == null)
                return;

            //StaggeredChainLightning chainLightning = new StaggeredChainLightning(parent.pawn?.Map, parent.pawn, 500, 30, 30, 15, DamageDefOf.ElectricalBurn, (Thing) =>
            //{
            //    return Thing != this.parent.pawn;
            //});


            //chainLightning.StartChain(target.Pawn);
        }

        public override void CompTick()
        {
            base.CompTick();
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            return base.Valid(target, throwMessages);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
        }
    }
}

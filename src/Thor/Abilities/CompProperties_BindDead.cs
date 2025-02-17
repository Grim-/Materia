using RimWorld;
using Verse;

namespace Thor
{
    public class CompProperties_BindDead : CompProperties_AbilityEffect
    {
        public CompProperties_BindDead()
        {
            compClass = typeof(CompAbilityEffect_BindDead);
        }
    }

    public class CompAbilityEffect_BindDead : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (target.Thing == null)
                return;

            if (target.Thing is Corpse corpse)
            {
                Pawn deadPawn = corpse.InnerPawn;
                ResurrectionUtility.TryResurrect(deadPawn);
                Hediff_Undead undeadHediff = (Hediff_Undead)deadPawn.health.GetOrAddHediff(ThorDefOf.DeathKnight_Undead);

                deadPawn.TryMakeUndeadSummon(this.parent.pawn);
                //corpse.Destroy();
            }
        }
    }
}

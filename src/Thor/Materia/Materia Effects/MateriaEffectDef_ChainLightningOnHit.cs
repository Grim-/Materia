using RimWorld;
using Verse;

namespace Thor
{
    public class MateriaEffectDef_ChainLightningOnHit : MateriaEffectDef
    {

    }

    public class MateriaEffect_ChainLightningOnHit : MateriaWorker
    {
        MateriaEffectDef_ChainLightningOnHit Def => (MateriaEffectDef_ChainLightningOnHit)def;

        private StaggeredChainLightning chainLightning;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult damageResult)
        {
            if (damageResult?.totalDamageDealt >= 0 && Attacker != null && target.Pawn != null)
            {
                chainLightning = new StaggeredChainLightning(Attacker.Map, Attacker, 20, 60, 15, DamageDefOf.ElectricalBurn, (Thing) =>
                {
                    return Thing != Attacker && Thing is Pawn targetPawn && targetPawn.Faction.HostileTo(Attacker.Faction);
                });

                chainLightning.StartChain(target.Pawn);
            }
            return damageResult;
        }

        public override void OnTick(Pawn pawn)
        {
            base.OnTick(pawn);

            if (chainLightning != null)
            {
                chainLightning.Tick();

                if (chainLightning.IsFinished)
                {
                    chainLightning.Stop();
                    chainLightning = null;
                }
            }
        }
    }
}
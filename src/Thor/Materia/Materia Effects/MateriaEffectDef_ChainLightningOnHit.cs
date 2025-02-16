using RimWorld;
using Verse;

namespace Thor
{
    public class MateriaEffectDef_ChainLightningOnHit : MateriaEffectDef
    {
        public int maxJumps = 6;
        public float targetRadius = 5f;
        public int damage = 1;
        public DamageDef damageType;

        public override string EffectDescription
        {
            get
            {
                string damageTypestring = damageType != null ? damageType.LabelCap : DamageDefOf.ElectricalBurn.LabelCap;
                return $"On Successful melee attack you deal {damage} ({damageTypestring}) damage to the target, which will then attempt to jump to upto {maxJumps} other targets in {targetRadius} radius around the target.";
            }
        }
    }

    public class MateriaEffect_ChainLightningOnHit : MateriaWorker
    {
        MateriaEffectDef_ChainLightningOnHit Def => (MateriaEffectDef_ChainLightningOnHit)def;

        private StaggeredChainLightning chainLightning;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult damageResult)
        {
            if (damageResult?.totalDamageDealt >= 0 && Attacker != null && target.Pawn != null)
            {
                if (chainLightning != null)
                {
                    chainLightning.Stop();
                    chainLightning = null;
                }

                chainLightning = new StaggeredChainLightning(Attacker.Map, Attacker, Def.maxJumps, Def.targetRadius, Def.damage, Def.damageType != null ? Def.damageType : DamageDefOf.ElectricalBurn, (Thing) =>
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
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Materia
{
    public class MateriaEffectDef_ChainLightningOnHit : MateriaEffectDef
    {
        public int maxJumps = 14;
        public float targetRadius = 15f;
        public int damage = 34;
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

        private List<StaggeredChainLightning> chainLightningInstances = new List<StaggeredChainLightning>();

        private int LastTriggerTick = -1;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult damageResult)
        {
            if (damageResult?.totalDamageDealt >= 0 && Attacker != null && target.Pawn != null)
            {
                StaggeredChainLightning chainLightning = new StaggeredChainLightning(Attacker.Map, Attacker, 1000, Def.maxJumps, Def.targetRadius, Def.damage, Def.damageType != null ? Def.damageType : DamageDefOf.ElectricalBurn, (Thing) =>
                {
                    return Thing != Attacker && Thing is Pawn targetPawn;
                });


                //Log.Message($"Starting chain on {target.Pawn}");
                chainLightning.StartChain(target.Pawn);

                chainLightningInstances.Add(chainLightning);
                LastTriggerTick = Current.Game.tickManager.TicksGame;
            }
            return damageResult;
        }



        public override void OnTick(Pawn pawn)
        {
            base.OnTick(pawn);

            if (chainLightningInstances != null)
            {
                for (int i = chainLightningInstances.Count - 1; i >= 0; i--)
                {
                    chainLightningInstances[i].Tick();

                    if (chainLightningInstances[i].IsFinished)
                    {
                        chainLightningInstances[i].Stop();
                        chainLightningInstances.RemoveAt(i);
                    }
                }
            }
        }
    }
}
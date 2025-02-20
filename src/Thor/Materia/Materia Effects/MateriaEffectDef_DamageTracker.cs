using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Materia
{
    public class MateriaEffectDef_DamageTracker : MateriaEffectDef
    {
        public float damageToHealthRatio = 0.001f; 
        public float maxStored = 1000f;      
        public float lossPerDay = 50f;

        public override string EffectDescription => $"Upto {damageToHealthRatio * 100} of your damage dealt is stored, upto a value of {maxStored}, you lose {lossPerDay} per day from this. \nYou gain {1f + (1 * damageToHealthRatio)} max health per damage stored.";
    }


    public class MateriaEffect_DamageTracker : MateriaWorker
    {
        private float storedDamage = 0f;
        private const int TicksPerProgressUpdate = 250;

        MateriaEffectDef_DamageTracker Def => (MateriaEffectDef_DamageTracker)def;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref storedDamage, "storedDamage", 0f);
        }

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult damageResult)
        {
            if (damageResult?.totalDamageDealt > 0f)
            {
                float newDamage = Mathf.Min(storedDamage + damageResult.totalDamageDealt, Def.maxStored);
                if (newDamage != storedDamage)
                {
                    storedDamage = newDamage;
                    MoteMaker.ThrowText(Attacker.DrawPos, Attacker.Map,
                        "Damage Stored: " + storedDamage.ToString("F1"),
                        Color.yellow);
                }
            }
            return damageResult;
        }

        public override void OnTick(Pawn pawn)
        {
            if (Find.TickManager.TicksGame % TicksPerProgressUpdate == 0)
            {
                float damageDecay = Def.lossPerDay * (TicksPerProgressUpdate / GenDate.TicksPerDay);
                storedDamage = Mathf.Max(0f, storedDamage - damageDecay);
            }
        }

        public override float GetStatFactor(StatDef stat)
        {
            if (stat == StatDefOf.MaxHitPoints)
            {
                return 1f + (storedDamage * Def.damageToHealthRatio);
            }
            return base.GetStatFactor(stat);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (EquippingPawn?.Faction == Faction.OfPlayer)
            {
                yield return new Gizmo_DamageTrackerStatus
                {
                    damageTracker = this,
                    maxDamage = Def.maxStored,
                    currentDamage = storedDamage
                };
            }
        }
    }
}
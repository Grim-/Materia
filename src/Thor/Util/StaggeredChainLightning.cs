using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Thor
{
    [StaticConstructorOnStartup]
    public class StaggeredChainLightning : Ticker
    {
        private readonly Map map;
        private readonly Thing instigator;
        private readonly int maxJumps;
        private readonly float jumpRadius;
        private readonly int damageAmount;
        private readonly DamageDef damageDef;
        private readonly Func<Thing, bool> targetValidator;
        private readonly HashSet<Thing> struckTargets = new HashSet<Thing>();

        private Thing firstTarget;
        private List<Thing> chainTargets;
        private int currentIndex = 0;
        private Thing currentTarget;
        private List<ChainLightningVisualEffect> activeEffects = new List<ChainLightningVisualEffect>();

        public StaggeredChainLightning(Map map, Thing instigator, int maxJumps, float jumpRadius,
            int damageAmount, DamageDef damageDef, Func<Thing, bool> targetValidator, int ticksBetweenJumps = 15)
            : base(ticksBetweenJumps, null, false)
        {
            this.map = map;
            this.instigator = instigator;
            this.maxJumps = maxJumps;
            this.jumpRadius = jumpRadius;
            this.damageAmount = damageAmount;
            this.damageDef = damageDef;
            this.targetValidator = targetValidator;

            _OnTick = DoNextChainStep;
        }

        public void StartChain(LocalTargetInfo firstTargetInfo)
        {
            if (!firstTargetInfo.HasThing || !targetValidator(firstTargetInfo.Thing) || !firstTargetInfo.Thing.Spawned)
                return;

            firstTarget = firstTargetInfo.Thing;
            chainTargets = FindAllTargets(firstTarget);

            if (chainTargets == null)
                return;

            currentIndex = 0;
            currentTarget = firstTarget;

            DamageTarget(firstTarget);

            if (chainTargets.Any())
            {
                Start();
            }
        }

        private List<Thing> FindAllTargets(Thing firstTarget)
        {
            List<Thing> chainTargets = new List<Thing>();
            Thing currentTarget = firstTarget;
            int remainingJumps = maxJumps - 1;

            struckTargets.Clear();
            struckTargets.Add(firstTarget);

            while (remainingJumps > 0)
            {
                IEnumerable<Thing> potentialTargets = map.listerThings.AllThings
                    .Where(t =>
                        t != currentTarget &&
                        !struckTargets.Contains(t) &&
                        targetValidator(t) &&
                        t.Position.InBounds(map) &&
                        t.Position.InHorDistOf(currentTarget.Position, jumpRadius));

                if (!potentialTargets.Any())
                    break;

                Thing nextTarget = potentialTargets.MinBy(t =>
                    t.Position.DistanceTo(currentTarget.Position));

                chainTargets.Add(nextTarget);
                struckTargets.Add(nextTarget);
                currentTarget = nextTarget;
                remainingJumps--;
            }

            return chainTargets;
        }

        private void DoNextChainStep()
        {
            if (chainTargets == null)
            {
                Stop();
                return;
            }

            if (currentIndex >= chainTargets.Count)
            {
                Stop();
                return;
            }

            Thing nextTarget = chainTargets[currentIndex];

            if (currentTarget == null || nextTarget == null || !currentTarget.Spawned || !nextTarget.Spawned)
            {
                Stop();
                return;
            }

            CreateLightningBetween(currentTarget.Position, nextTarget.Position);
            DamageTarget(nextTarget);

            currentTarget = nextTarget;
            currentIndex++;
        }

        private void CreateLightningBetween(IntVec3 start, IntVec3 end)
        {
            Vector3 startVec = start.ToVector3Shifted();
            Vector3 endVec = end.ToVector3Shifted();

            ChainLightningVisualEffect effect = (ChainLightningVisualEffect)ThingMaker.MakeThing(
                ThingDef.Named("ChainLightningVisual"));

            GenSpawn.Spawn(effect, start, map);
            effect.Initialize(startVec, endVec, 100 + 8); // Extra ticks for fade out
            activeEffects.Add(effect);

            // Clean up destroyed effects
            activeEffects.RemoveAll(e => e.Destroyed);
        }

        private void DamageTarget(Thing target)
        {
            DamageInfo dinfo = new DamageInfo(
                damageDef,
                damageAmount,
                instigator: instigator,
                hitPart: null,
                weapon: null
            );
            target.TakeDamage(dinfo);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref currentIndex, "currentIndex", 0);
            Scribe_References.Look(ref firstTarget, "firstTarget");
            Scribe_References.Look(ref currentTarget, "currentTarget");
            Scribe_Collections.Look(ref chainTargets, "chainTargets", LookMode.Reference);
            Scribe_Collections.Look(ref activeEffects, "activeEffects", LookMode.Reference);
        }
    }
}

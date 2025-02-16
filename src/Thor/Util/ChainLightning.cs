using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Thor
{
    [StaticConstructorOnStartup]
    public class ChainLightning
    {
        private readonly Map map;
        private readonly Thing instigator;
        private readonly int maxJumps;
        private readonly float jumpRadius;
        private readonly int damageAmount;
        private readonly DamageDef damageDef;
        private readonly Func<Thing, bool> targetValidator;
        private readonly HashSet<Thing> struckTargets = new HashSet<Thing>();
        private static readonly Material LightningMat = MaterialPool.MatFrom("Weather/LightningBolt", ShaderDatabase.MoteGlow);
        private readonly Mesh boltMesh;

        public ChainLightning(Map map, Thing instigator, int maxJumps, float jumpRadius,
            int damageAmount, DamageDef damageDef, Func<Thing, bool> targetValidator)
        {
            this.map = map;
            this.instigator = instigator;
            this.maxJumps = maxJumps;
            this.jumpRadius = jumpRadius;
            this.damageAmount = damageAmount;
            this.damageDef = damageDef;
            this.targetValidator = targetValidator;
            this.boltMesh = LightningBoltMeshPool.RandomBoltMesh;
        }

        public void StartChain(LocalTargetInfo firstTarget)
        {
            if (!firstTarget.HasThing || !targetValidator(firstTarget.Thing))
                return;

            List<Thing> chainTargets = FindAllTargets(firstTarget.Thing);
            ExecuteChainLightning(firstTarget.Thing, chainTargets);
        }

        private List<Thing> FindAllTargets(Thing firstTarget)
        {
            List<Thing> chainTargets = new List<Thing>();
            Thing currentTarget = firstTarget;
            int remainingJumps = maxJumps - 1;  // -1 because we already have first target

            struckTargets.Add(firstTarget);  // Mark first target as struck immediately

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

                // Get closest valid target to CURRENT target
                Thing nextTarget = potentialTargets.MinBy(t =>
                    t.Position.DistanceTo(currentTarget.Position));

                chainTargets.Add(nextTarget);
                struckTargets.Add(nextTarget);
                currentTarget = nextTarget;  
                remainingJumps--;
            }

            return chainTargets;
        }

        private void ExecuteChainLightning(Thing firstTarget, List<Thing> chainTargets)
        {
            Thing currentTarget = firstTarget;
            DamageTarget(currentTarget);

            foreach (Thing nextTarget in chainTargets)
            {
                DrawLightningBetween(currentTarget.Position, nextTarget.Position);
                DamageTarget(nextTarget);
                currentTarget = nextTarget;  // Update for next iteration
            }
        }

        private void DrawLightningBetween(IntVec3 start, IntVec3 end)
        {
            Vector3 startVec = start.ToVector3Shifted();
            Vector3 endVec = end.ToVector3Shifted();

            Graphics.DrawMesh(
                boltMesh,
                 startVec,
                Quaternion.FromToRotation(startVec, endVec),
                FadedMaterialPool.FadedVersionOf(LightningMat, 0.8f),
                0
            );
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
    }
}


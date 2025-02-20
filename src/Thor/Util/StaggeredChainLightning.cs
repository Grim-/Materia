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
        #region Fields
        private readonly Map map;
        private readonly Thing instigator;
        private readonly int maxJumps;
        private readonly int wantedLifetimeTicks;
        private readonly int ticksBetweenJumps;
        private readonly float jumpRadius;
        private readonly int damageAmount;
        private readonly DamageDef damageDef;
        private readonly Func<Thing, bool> targetValidator;
        private readonly HashSet<Thing> struckTargets = new HashSet<Thing>();

        private Thing firstTarget;
        private List<Thing> chainTargets;
        private int currentIndex = 0;
        private Thing currentTarget;
        private int jumpWaitCounter = 0;
        private const int LightningEffectLifetime = 399;

        // Lingering effect settings
        private int effectLingerTicks = 60; // How long effects should linger after last jump (1 second at 60 TPS)
        private int lingerCounter = 0;
        private bool inLingeringPhase = false;

        private List<TrackedMote> activeEffects = new List<TrackedMote>();
        private static readonly ThingDef LightningMoteDef = DefDatabase<ThingDef>.GetNamed("Mote_GraserBeamBase");

        private Mesh Mesh = null;
        public event Action<StaggeredChainLightning, Thing, Map> OnTargetHit;

        #endregion

        #region Initialization
        public StaggeredChainLightning(Map map, Thing instigator, int lifeTimeTicks, int maxJumps, float jumpRadius,
            int damageAmount, DamageDef damageDef, Func<Thing, bool> targetValidator, int ticksBetweenJumps = 7, int effectLingerTicks = 120)
            : base(lifeTimeTicks, null, false)
        {
            this.map = map;
            this.instigator = instigator;
            this.maxJumps = maxJumps;
            this.jumpRadius = jumpRadius;
            this.damageAmount = damageAmount;
            this.damageDef = damageDef;
            this.targetValidator = targetValidator;
            this.ticksBetweenJumps = ticksBetweenJumps;
            this.wantedLifetimeTicks = lifeTimeTicks;
            this.effectLingerTicks = effectLingerTicks;
        }

        public void StartChain(LocalTargetInfo firstTargetInfo)
        {
            if (!ValidateFirstTarget(firstTargetInfo))
                return;

            SetupChain(firstTargetInfo);
            DamageTarget(firstTarget);
            Start();
        }

        private bool ValidateFirstTarget(LocalTargetInfo firstTargetInfo)
        {
            return firstTargetInfo.HasThing &&
                   targetValidator(firstTargetInfo.Thing) &&
                   firstTargetInfo.Thing.Spawned;
        }

        private void SetupChain(LocalTargetInfo firstTargetInfo)
        {
            firstTarget = firstTargetInfo.Thing;
            chainTargets = FindAllTargets(firstTarget);

            // Adjusting lifetime to include lingering phase
            this.Ticks = ticksBetweenJumps * (chainTargets?.Count ?? 0) + effectLingerTicks;

            currentIndex = 0;
            currentTarget = firstTarget;
        }

        public override void Start()
        {
            base.Start();
        }
        #endregion

        #region Target Finding
        private List<Thing> FindAllTargets(Thing firstTarget)
        {
            List<Thing> targets = new List<Thing>();
            Thing current = firstTarget;
            int remainingJumps = maxJumps - 1;

            InitializeTargetTracking(firstTarget);

            while (remainingJumps > 0 && TryFindNextTarget(current, out Thing nextTarget))
            {
                targets.Add(nextTarget);
                struckTargets.Add(nextTarget);
                current = nextTarget;
                remainingJumps--;
            }

            return targets;
        }

        private void InitializeTargetTracking(Thing firstTarget)
        {
            struckTargets.Clear();
            struckTargets.Add(firstTarget);
        }

        private bool TryFindNextTarget(Thing currentTarget, out Thing nextTarget)
        {
            nextTarget = null;

            IEnumerable<Thing> potentialTargets = GetPotentialTargets(currentTarget);

            if (!potentialTargets.Any())
                return false;

            nextTarget = potentialTargets.MinBy(t => t.Position.DistanceTo(currentTarget.Position));
            return nextTarget != null;
        }

        private IEnumerable<Thing> GetPotentialTargets(Thing currentTarget)
        {
            return GenRadial.RadialDistinctThingsAround(currentTarget.Position, map, jumpRadius, true)
                .Where(t =>
                    t != currentTarget &&
                    !struckTargets.Contains(t) &&
                    targetValidator(t) &&
                    t.Position.InBounds(map));
        }
        #endregion

        #region Main Loop
        public override void Tick()
        {
            base.Tick();
            ProcessActiveEffects();

            if (ShouldStop())
                return;

            if (inLingeringPhase)
            {
                ProcessLingeringPhase();
            }
            else
            {
                ProcessChainStep();
            }
        }

        private bool ShouldStop()
        {
            if (IsFinished)
                return true;

            if (chainTargets == null || chainTargets.Count == 0)
            {
                Stop();
                return true;
            }

            return false;
        }

        private void ProcessLingeringPhase()
        {
            lingerCounter++;
            if (lingerCounter >= effectLingerTicks)
            {
                Stop();
            }
        }

        private void ProcessChainStep()
        {
            jumpWaitCounter++;

            if (jumpWaitCounter >= ticksBetweenJumps)
            {
                if (currentIndex < chainTargets.Count)
                {
                    DoNextChainStep();
                }
                else
                {
                    EnterLingeringPhase();
                }

                jumpWaitCounter = 0;
            }
        }

        private void EnterLingeringPhase()
        {
            inLingeringPhase = true;
            lingerCounter = 0;
        }
        #endregion

        #region Effect Processing
        private void ProcessActiveEffects()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                TrackedMote trackedMote = activeEffects[i];

                if (!ValidateActiveEffect(trackedMote))
                {
                    CleanupEffect(trackedMote);
                    activeEffects.RemoveAt(i);
                }
                else
                {
                    UpdateEffect(trackedMote);
                }
            }
        }
        private void LightningOnTarget(Thing arg2, Map arg3)
        {
            if (Mesh == null)
            {
                Mesh = LightningBoltMeshPool.RandomBoltMesh;
            }

            WeatherEvent_LightningStrike.DoStrike(arg2.Position, arg3, ref Mesh);
        }
        private void CleanupEffect(TrackedMote trackedMote)
        {
            if (trackedMote.Mote != null && !trackedMote.Mote.Destroyed)
            {
                trackedMote.Mote.Destroy();
            }
        }

        private void UpdateEffect(TrackedMote trackedMote)
        {
            trackedMote.Mote.Maintain();
            trackedMote.Mote.UpdateTargets(
                trackedMote.SourceThing,
                trackedMote.TargetThing,
                Vector3.zero,
                Vector3.zero
            );
        }

        private bool ValidateActiveEffect(TrackedMote trackedMote)
        {
            return trackedMote.RemainingTicks > 0 &&
                   trackedMote.SourceThing != null &&
                   trackedMote.SourceThing.Spawned &&
                   trackedMote.TargetThing != null &&
                   trackedMote.TargetThing.Spawned;
        }
        #endregion

        #region Chain Stepping
        private void DoNextChainStep()
        {
            if (currentIndex >= chainTargets.Count)
            {
                EnterLingeringPhase();
                return;
            }

            Thing nextTarget = chainTargets[currentIndex];

            if (!ValidateChainStep(nextTarget))
            {
                Stop();
                return;
            }

            CreateLightningBetween(currentTarget, nextTarget);
            DamageTarget(nextTarget);

            currentTarget = nextTarget;
            currentIndex++;
        }

        private bool ValidateChainStep(Thing nextTarget)
        {
            return currentTarget != null &&
                   nextTarget != null &&
                   currentTarget.Spawned &&
                   nextTarget.Spawned;
        }

        private void CreateLightningBetween(Thing source, Thing target)
        {
            if (LightningMoteDef == null)
            {
                Log.Error("Thor: MoteChainLightning def not found");
                return;
            }

            MoteDualAttached mote = MoteMaker.MakeInteractionOverlay(
                LightningMoteDef,
                source,
                new TargetInfo(target.Position, map, false));

            if (mote == null)
                return;

            TrackedMote trackedMote = new TrackedMote(mote, source, target, LightningEffectLifetime);
            activeEffects.Add(trackedMote);

            FleckMaker.ThrowLightningGlow(source.DrawPos, map, 1.5f);
        }

        private void DamageTarget(Thing target)
        {
            OnTargetHit?.Invoke(this, target, map);
            LightningOnTarget(target, map);
            DamageInfo dinfo = new DamageInfo(
                damageDef,
                damageAmount,
                instigator: instigator,
                hitPart: null,
                weapon: null
            );
            target.TakeDamage(dinfo);

            FleckMaker.Static(target.Position, map, FleckDefOf.ExplosionFlash, 1f);
            FleckMaker.ThrowLightningGlow(target.DrawPos, map, 1f);
        }
        #endregion

        #region Cleanup
        public override void Stop(bool reset = false)
        {
            base.Stop();
            CleanupAllEffects();
        }

        private void CleanupAllEffects()
        {
            foreach (TrackedMote trackedMote in activeEffects)
            {
                CleanupEffect(trackedMote);
            }
            activeEffects.Clear();
        }
        #endregion

        #region Save/Load
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref currentIndex, "currentIndex", 0);
            Scribe_Values.Look(ref inLingeringPhase, "inLingeringPhase", false);
            Scribe_Values.Look(ref lingerCounter, "lingerCounter", 0);
            Scribe_References.Look(ref firstTarget, "firstTarget");
            Scribe_References.Look(ref currentTarget, "currentTarget");
            Scribe_Collections.Look(ref chainTargets, "chainTargets", LookMode.Reference);
        }
        #endregion
    }
}
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Thor
{
    //[StaticConstructorOnStartup]
    //public class StaggeredChainLightning : Ticker
    //{
    //    private readonly Map map;
    //    private readonly Thing instigator;
    //    private readonly int maxJumps;
    //    private readonly float jumpRadius;
    //    private readonly int damageAmount;
    //    private readonly DamageDef damageDef;
    //    private readonly Func<Thing, bool> targetValidator;
    //    private readonly HashSet<Thing> struckTargets = new HashSet<Thing>();
    //    private static readonly Material LightningMat = MaterialPool.MatFrom("Weather/LightningBolt", ShaderDatabase.MoteGlow);
    //    private readonly Mesh boltMesh;

    //    private Thing firstTarget;
    //    private List<Thing> chainTargets;
    //    private int currentIndex = 0;
    //    private Thing currentTarget;

    //    public StaggeredChainLightning(Map map, Thing instigator, int maxJumps, float jumpRadius,
    //        int damageAmount, DamageDef damageDef, Func<Thing, bool> targetValidator, int ticksBetweenJumps = 15)
    //        : base(ticksBetweenJumps, null, false)
    //    {
    //        this.map = map;
    //        this.instigator = instigator;
    //        this.maxJumps = maxJumps;
    //        this.jumpRadius = jumpRadius;
    //        this.damageAmount = damageAmount;
    //        this.damageDef = damageDef;
    //        this.targetValidator = targetValidator;
    //        this.boltMesh = LightningBoltMeshPool.RandomBoltMesh;

    //        _OnTick = DoNextChainStep;
    //    }

    //    public void StartChain(LocalTargetInfo firstTargetInfo)
    //    {
    //        if (!firstTargetInfo.HasThing || !targetValidator(firstTargetInfo.Thing) || !firstTargetInfo.Thing.Spawned)
    //            return;

    //        firstTarget = firstTargetInfo.Thing;
    //        chainTargets = FindAllTargets(firstTarget);

    //        if (chainTargets == null)
    //            return;

    //        currentIndex = 0;
    //        currentTarget = firstTarget;

    //        DamageTarget(firstTarget);

    //        if (chainTargets.Any())
    //        {
    //            Start();
    //        }
    //    }

    //    private List<Thing> FindAllTargets(Thing firstTarget)
    //    {
    //        List<Thing> chainTargets = new List<Thing>();
    //        Thing currentTarget = firstTarget;
    //        int remainingJumps = maxJumps - 1;  

    //        struckTargets.Clear();
    //        struckTargets.Add(firstTarget);

    //        while (remainingJumps > 0)
    //        {
    //            IEnumerable<Thing> potentialTargets = map.listerThings.AllThings
    //                .Where(t =>
    //                    t != currentTarget &&
    //                    !struckTargets.Contains(t) &&
    //                    targetValidator(t) &&
    //                    t.Position.InBounds(map) &&
    //                    t.Position.InHorDistOf(currentTarget.Position, jumpRadius));

    //            if (!potentialTargets.Any())
    //                break;

    //            Thing nextTarget = potentialTargets.MinBy(t =>
    //                t.Position.DistanceTo(currentTarget.Position));

    //            chainTargets.Add(nextTarget);
    //            struckTargets.Add(nextTarget);
    //            currentTarget = nextTarget;
    //            remainingJumps--;
    //        }

    //        return chainTargets;
    //    }

    //    private void DoNextChainStep()
    //    {
    //        if (chainTargets == null)
    //        {
    //            Stop();
    //            return;
    //        }

    //        if (currentIndex >= chainTargets.Count)
    //        {
    //            Stop();
    //            return;
    //        }

    //        Thing nextTarget = chainTargets[currentIndex];

    //        if (currentTarget == null || nextTarget == null || !currentTarget.Spawned || !nextTarget.Spawned)
    //        {
    //            Stop();
    //            return;
    //        }

    //        DrawLightningBetween(currentTarget.Position, nextTarget.Position);

    //        DamageTarget(nextTarget);

    //        currentTarget = nextTarget;
    //        currentIndex++;
    //    }

    //    private void DrawLightningBetween(IntVec3 start, IntVec3 end)
    //    {
    //        Vector3 startVec = start.ToVector3Shifted();
    //        Vector3 endVec = end.ToVector3Shifted();

    //        Graphics.DrawMesh(
    //            boltMesh,
    //            startVec.SetToAltitude(AltitudeLayer.MoteOverhead),
    //            Vector3Utility.ToAngleFlat(endVec - startVec).ToQuat(),
    //            FadedMaterialPool.FadedVersionOf(LightningMat, 0.8f),
    //            0
    //        );
    //    }

    //    private void DamageTarget(Thing target)
    //    {
    //        DamageInfo dinfo = new DamageInfo(
    //            damageDef,
    //            damageAmount,
    //            instigator: instigator,
    //            hitPart: null,
    //            weapon: null
    //        );
    //        target.TakeDamage(dinfo);
    //    }

    //    public override void ExposeData()
    //    {
    //        base.ExposeData();
    //        Scribe_Values.Look(ref currentIndex, "currentIndex", 0);
    //        Scribe_References.Look(ref firstTarget, "firstTarget");
    //        Scribe_References.Look(ref currentTarget, "currentTarget");
    //        Scribe_Collections.Look(ref chainTargets, "chainTargets", LookMode.Reference);
    //    }
    //}
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
    public class ChainLightningVisualEffect : Thing
    {
        private static readonly Material LightningMat = MaterialPool.MatFrom("Weather/LightningBolt", ShaderDatabase.MoteGlow);
        private const float BoltWidth = 1.5f;
        private const int SegmentsPerBolt = 6;
        private const float JitterAmount = 0.3f;

        private List<LightningSegment> segments = new List<LightningSegment>();
        private int ticksRemaining;
        private float fadeOutProgress = 0f;
        private readonly int fadeOutTicks = 8;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 0);
            Scribe_Values.Look(ref fadeOutProgress, "fadeOutProgress", 0f);
        }

        public void Initialize(Vector3 start, Vector3 end, int duration)
        {
            Position = start.ToIntVec3();
            ticksRemaining = duration;
            GenerateLightningSegments(start, end);
        }

        private void GenerateLightningSegments(Vector3 start, Vector3 end)
        {
            segments.Clear();
            Vector3 direction = end - start;
            float totalDistance = direction.magnitude;
            Vector3 normalizedDir = direction.normalized;

            List<Vector3> controlPoints = new List<Vector3> { start };

            for (int i = 1; i < SegmentsPerBolt; i++)
            {
                float progress = i / (float)(SegmentsPerBolt - 1);
                Vector3 idealPoint = Vector3.Lerp(start, end, progress);

                Vector3 perpendicular = Vector3.Cross(normalizedDir, Vector3.up).normalized;
                float jitter = Rand.Range(-JitterAmount, JitterAmount) * totalDistance;
                Vector3 offset = perpendicular * jitter;
                offset.y = Rand.Range(-JitterAmount * 0.3f, JitterAmount * 0.3f) * totalDistance;

                controlPoints.Add(idealPoint + offset);
            }

            controlPoints.Add(end);

            for (int i = 0; i < controlPoints.Count - 1; i++)
            {
                segments.Add(new LightningSegment(
                    controlPoints[i],
                    controlPoints[i + 1],
                    BoltWidth * Rand.Range(0.8f, 1.2f)));
            }
        }

        public override void Tick()
        {
            if (ticksRemaining <= 0)
            {
                if (fadeOutProgress >= 1f)
                {
                    Destroy();
                    return;
                }
                fadeOutProgress += 1f / fadeOutTicks;
            }
            else
            {
                ticksRemaining--;
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            if (segments == null) 
                return;

            float alpha = 1f - fadeOutProgress;
            Material fadedMat = FadedMaterialPool.FadedVersionOf(LightningMat, alpha);

            foreach (var segment in segments)
            {
                Vector3 center = (segment.Start + segment.End) * 0.5f;
                Vector3 scale = new Vector3(segment.Width, 1f, (segment.End - segment.Start).magnitude);
                Quaternion rotation = Quaternion.LookRotation(segment.End - segment.Start);

                Matrix4x4 matrix = Matrix4x4.TRS(
                    center,
                    rotation,
                    scale);

                Graphics.DrawMesh(MeshPool.plane10, matrix, fadedMat, 0);
            }
        }

        private class LightningSegment
        {
            public Vector3 Start { get; }
            public Vector3 End { get; }
            public float Width { get; }

            public LightningSegment(Vector3 start, Vector3 end, float width)
            {
                Start = start;
                End = end;
                Width = width;
            }
        }
    }

    // Helper class to manage the creation and lifecycle of chain lightning effects
    public class ChainLightningManager
    {
        private readonly Map map;

        public ChainLightningManager(Map map)
        {
            this.map = map;
        }

        public void CreateChainLightning(Vector3 start, Vector3 end, int durationTicks)
        {
            ChainLightningVisualEffect lightning = (ChainLightningVisualEffect)ThingMaker.MakeThing(ThingDef.Named("ChainLightningVisual"));
            GenSpawn.Spawn(lightning, start.ToIntVec3(), map);
            lightning.Initialize(start, end, durationTicks);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Thor
{
    [StaticConstructorOnStartup]
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
}

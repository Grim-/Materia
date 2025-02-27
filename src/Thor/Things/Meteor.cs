using RimWorld;
using UnityEngine;
using Verse;

namespace Materia
{
    [StaticConstructorOnStartup]
    public class Meteor : Thing
    {
        // Textures
        private static readonly Texture2D meteorTexture = ContentFinder<Texture2D>.Get("Meteor");
        private static readonly Texture2D shadowTexture = ContentFinder<Texture2D>.Get("MeteorShadow");

        // Animation properties
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private Vector3 currentPosition;
        private int currentTick = 0;
        private int totalDurationTicks = 1000;
        private bool hasImpacted = false;
        private float scale = 1.5f;
        private float startingHeight = 200f;

        // Explosion properties
        private int explosionRadius = 25;
        private int damageAmount = 150;
        private DamageDef damageDef = DamageDefOf.Bomb;
        protected bool Launched = false;



        private Vector2 startSize = new Vector2(15, 15);
        private Vector2 finalSize = new Vector2(50f, 50f);

        public override Vector2 DrawSize
        {
            get
            {
                float progress = Mathf.Clamp01((float)currentTick / totalDurationTicks);
                return Vector2.Lerp(finalSize, startSize, progress);
            }
        }


        private float initialShadowSize = 0.8f;
        private float finalShadowSize = 300f;
        private float ShadowSize
        {
            get
            {
                float progress = Mathf.Clamp01((float)currentTick / totalDurationTicks);
                return Mathf.Lerp(initialShadowSize, finalShadowSize, Mathf.Pow(progress, 0.5f));
            }
        }

        private float shadowOpacityMin = 0.1f;
        private float shadowOpacityMax = 0.8f;
        private float ShadowOpacity
        {
            get
            {
                float progress = Mathf.Clamp01((float)currentTick / totalDurationTicks);
                return Mathf.InverseLerp(shadowOpacityMin, shadowOpacityMax, Mathf.Pow(progress, 0.7f));
            }
        }
        public void Launch(IntVec3 target, int ticksToImpact = 1000)
        {
            targetPosition = target.ToVector3Shifted();
            startPosition = new Vector3(targetPosition.x, startingHeight, targetPosition.z - startingHeight);
            currentPosition = startPosition;
            currentTick = 0;
            this.totalDurationTicks = ticksToImpact;
            Launched = true;
        }

        public override void Tick()
        {
            if (hasImpacted || !Launched)
                return;

            currentTick++;

            float progress = Mathf.Clamp01((float)currentTick / totalDurationTicks);

            currentPosition = Vector3.Lerp(startPosition, targetPosition, progress);
            currentPosition.y = Mathf.Lerp(startPosition.y, 0f, progress);

            // Check for impact
            if (currentTick >= totalDurationTicks && !hasImpacted)
            {
                Impact();
            }
        }

        private void Impact()
        {
            hasImpacted = true;
            GenExplosion.DoExplosion(
                Position,
                Map,
                explosionRadius,
                damageDef,
                this,
                damageAmount);
            this.DeSpawn();
        }


        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Vector3 shadowPos = new Vector3(targetPosition.x, 0, targetPosition.z);
            float shadowSize = ShadowSize;

            Graphics.DrawMesh(
                MeshPool.plane10,
                Matrix4x4.TRS(
                    shadowPos,
                    Quaternion.identity,
                    new Vector3(shadowSize, 1f, shadowSize)),
                FadedMaterialPool.FadedVersionOf(MaterialPool.MatFrom(shadowTexture, ShaderTypeDefOf.Transparent.Shader, Color.black), 1f),
                0);

            float progress = Mathf.Clamp01((float)currentTick / totalDurationTicks);
            if (progress > 0.7f)
            {
                Vector2 size = DrawSize;
                Graphics.DrawMesh(
                    MeshPool.plane10,
                    Matrix4x4.TRS(
                        currentPosition,
                        Quaternion.identity,
                        new Vector3(size.x * scale, 1f, size.y * scale)),
                    FadedMaterialPool.FadedVersionOf(MaterialPool.MatFrom(meteorTexture), 1f),
                    0);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref startPosition, "startPosition");
            Scribe_Values.Look(ref targetPosition, "targetPosition");
            Scribe_Values.Look(ref currentPosition, "currentPosition");
            Scribe_Values.Look(ref currentTick, "currentTick");
            Scribe_Values.Look(ref totalDurationTicks, "totalDurationTicks");
            Scribe_Values.Look(ref hasImpacted, "hasImpacted");
            Scribe_Values.Look(ref Launched, "isLaunched");
        }
    }
}

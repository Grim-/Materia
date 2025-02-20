using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Materia
{
    public class CompProperties_LightningRing : CompProperties_AbilityEffect
    {
        public int delayTicks = 15;
        public List<LightningRingConfig> rings =new List<LightningRingConfig>
        {
            new LightningRingConfig(4, 3f),
            new LightningRingConfig(5, 5f),
            new LightningRingConfig(7, 7f)
        };

        public CompProperties_LightningRing()
        {
            compClass = typeof(Comp_LightningRing);
        }
    }


    public class LightningRingConfig
    {
        public int Strikes { get; set; }
        public float Radius { get; set; }

        public LightningRingConfig(int strikes, float radius)
        {
            Strikes = strikes;
            Radius = radius;
        }
    }

    public class Comp_LightningRing : CompAbilityEffect
    {
        private int currentRing = 0;
        private int ticksUntilNext = 0;
        new CompProperties_LightningRing Props => (CompProperties_LightningRing)props;

        private IntVec3 origin;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (parent.pawn?.Map == null)
                return;

            origin = target.Cell;

            currentRing = 0;
            ticksUntilNext = Props.delayTicks;
            DoLightningRing();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (currentRing >= Props.rings.Count) return;

            if (ticksUntilNext > 0)
            {
                ticksUntilNext--;
                return;
            }

            currentRing++;
            if (currentRing < Props.rings.Count)
            {
                DoLightningRing();
                ticksUntilNext = Props.delayTicks;
            }
        }

        private void DoLightningRing()
        {
            Map map = parent.pawn.Map;
            if (map == null)
                return;
        
            var ring = Props.rings[currentRing];
            float angleStep = 360f / ring.Strikes;

            for (int i = 0; i < ring.Strikes; i++)
            {
                float angle = i * angleStep + Rand.Range(-10f, 10f);
                float rad = ring.Radius + Rand.Range(-0.5f, 0.5f);

                IntVec3 strikePos = origin + GetStrikeOffset(angle, rad);
                if (strikePos.InBounds(map))
                {
                    var weather = new WeatherEvent_LightningStrike(map, strikePos);
                    weather.FireEvent();
                    map.weatherManager.eventHandler.AddEvent(weather);
                }
            }
        }

        private IntVec3 GetStrikeOffset(float angle, float radius)
        {
            int x = Mathf.RoundToInt(radius * Mathf.Cos(angle * Mathf.Deg2Rad));
            int z = Mathf.RoundToInt(radius * Mathf.Sin(angle * Mathf.Deg2Rad));
            return new IntVec3(x, 0, z);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            return parent.pawn?.Map != null && base.Valid(target, throwMessages);
        }
    }

    [StaticConstructorOnStartup]
    public class VisualLightningStrike
    {
        private IntVec3 strikeLoc;
        private Mesh boltMesh;
        private float fadeOut = 1f;
        private static readonly Material LightningMat = MaterialPool.MatFrom("Weather/LightningBolt", ShaderDatabase.MoteGlow);

        public bool Expired => fadeOut <= 0f;

        public VisualLightningStrike(IntVec3 location)
        {
            strikeLoc = location;
            boltMesh = LightningBoltMeshPool.RandomBoltMesh;
        }

        public void Draw()
        {
            if (!Expired)
            {
                Graphics.DrawMesh(boltMesh, strikeLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather),
                    Quaternion.identity,
                    FadedMaterialPool.FadedVersionOf(LightningMat, fadeOut), 0);
                fadeOut -= 0.1f;
            }
        }
    }
}

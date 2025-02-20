using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Materia
{
    public static class LightningStrike
    {
        public static void GenerateLightningStrike(Map map, IntVec3 Position, ref Mesh boltMesh, ref Material LightningMat, float Radius, int Damage = 0, float ArmourPen = 1f, DamageDef OverrideDamage = null, SoundDef OverrideSoundToPlay = null, int repeatVisualCount = 4)
        {
            if (Position.InBounds(map))
            {

                if (!Position.IsValid)
                {
                    Position = CellFinderLoose.RandomCellWith((IntVec3 sq) => sq.Standable(map) && !map.roofGrid.Roofed(sq), map, 1000);
                }
                boltMesh = LightningBoltMeshPool.RandomBoltMesh;
                if (!Position.Fogged(map))
                {
                    GenExplosion.DoExplosion(Position, map, Radius,
                        OverrideDamage != null ? OverrideDamage : DamageDefOf.Flame, null,
                        Damage > 0 ? Damage : -1,
                        ArmourPen,
                        null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false, null, null, null, true, 1f, 0f, true, null, 1f, null, null);

                    Vector3 loc = Position.ToVector3Shifted();
                    for (int x = 0; x < repeatVisualCount; x++)
                    {
                        FleckMaker.ThrowSmoke(loc, map, 1.5f);
                        FleckMaker.ThrowMicroSparks(loc, map);
                        FleckMaker.ThrowLightningGlow(loc, map, 1.5f);
                    }
                }
                SoundInfo info = SoundInfo.InMap(new TargetInfo(Position, map, false), MaintenanceType.None);
                SoundDefOf.Thunder_OnMap.PlayOneShot(info);

                Graphics.DrawMesh(boltMesh, Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather),
                    Quaternion.identity, LightningMat, 0);
            }
        }
    }
}

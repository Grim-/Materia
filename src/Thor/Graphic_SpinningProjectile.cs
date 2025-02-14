using UnityEngine;
using Verse;

namespace Thor
{
    public class Graphic_SpinningProjectile : Graphic_Single
    {
        private const float ROTATION_SPEED = 720f; // Increased speed for testing visibility

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            float currentRotation = Find.TickManager.TicksGame * 1f;
            currentRotation = (currentRotation * ROTATION_SPEED / 60f) % 360f; // Convert ticks to degrees

            Mesh mesh = this.MeshAt(rot);
            Quaternion quat = Quaternion.identity;

            // Apply the rotations in order
            if (extraRotation != 0f)
            {
                quat *= Quaternion.Euler(0f, extraRotation, 0f);
            }

            // Add our spin rotation
            quat *= Quaternion.Euler(0f, currentRotation, 0f);

            loc += this.DrawOffset(rot);
            Graphics.DrawMesh(mesh, loc, quat, this.MatSingle, 0);


            if (this.ShadowGraphic != null)
            {
                this.ShadowGraphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
            }
        }

        public override bool ShouldDrawRotated
        {
            get
            {
                return true;
            }
        }

        public override Material MatSingleFor(Thing thing)
        {
            return this.MatSingle;
        }
    }
}

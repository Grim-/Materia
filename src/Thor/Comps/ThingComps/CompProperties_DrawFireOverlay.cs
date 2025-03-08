using RimWorld;
using UnityEngine;
using Verse;

namespace Materia
{
    public class CompProperties_DrawFireOverlay : CompProperties
    {
        public Vector3 drawOffset = new Vector3(0,1,1f);


        public CompProperties_DrawFireOverlay()
        {
            compClass = typeof(CompDrawFireOverlay);
        }
    }

    public class CompDrawFireOverlay : ThingComp
    {
        CompProperties_DrawFireOverlay Props => (CompProperties_DrawFireOverlay)props;
        private Vector3 drawPosition;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.drawPosition = this.parent.DrawPos + Props.drawOffset;
        }

        public override void PostDraw()
        {
            CompFireOverlay.FireGraphic.Draw(this.drawPosition, Rot4.North, this.parent, 0f);
        }
    }
}

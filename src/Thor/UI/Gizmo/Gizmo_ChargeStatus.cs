using UnityEngine;
using Verse;

namespace Materia
{
    [StaticConstructorOnStartup]
    public class Gizmo_ChargeStatus : Gizmo
    {
        private Comp_BoundPawn comp;
        private static readonly Vector2 BarSize = new Vector2(32f, 4f);
        private static readonly Color BarColor = new Color(0.5f, 0.8f, 1f);

        public Gizmo_ChargeStatus(Comp_BoundPawn comp)
        {
            this.comp = comp;
            Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return 100f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, BarSize.x, BarSize.y);
            float fillPercent = comp.CurrentCharge / comp.MaxCharge;

            Widgets.FillableBar(rect, fillPercent);

            // Show charge percentage on mouseover
            if (Mouse.IsOver(rect))
            {
                TooltipHandler.TipRegion(rect, $"Charge: {comp.CurrentCharge:F1} / {comp.MaxCharge:F1}");
            }

            return new GizmoResult(GizmoState.Clear);
        }
    }
}

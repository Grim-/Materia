using UnityEngine;
using Verse;

namespace Materia
{
    [StaticConstructorOnStartup]
    public class Gizmo_MateriaShieldStatus : Gizmo
    {
        public MateriaEffect_DamageShield shield;
        private static readonly Texture2D ShieldBarFilledTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.9f));
        private static readonly Texture2D ShieldBarEmptyTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public Gizmo_MateriaShieldStatus()
        {
            Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return maxWidth;
        }


        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, 140f, 26f);
            Rect innerRect = rect.ContractedBy(3f);
            Widgets.DrawWindowBackground(rect);

            MateriaEffectDef_DamageShield def = shield.Def;
            float fillPercent = shield.currentShieldHP / def.shieldMaxCapacity;

            Widgets.FillableBar(innerRect, fillPercent, ShieldBarFilledTex, ShieldBarEmptyTex, false);
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(innerRect, shield.GetShieldStatus());
            Text.Anchor = TextAnchor.UpperLeft;

            return new GizmoResult(GizmoState.Clear);
        }
    }
}
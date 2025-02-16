using UnityEngine;
using Verse;

namespace Thor
{
    public class Gizmo_DamageTrackerStatus : Gizmo
    {
        public MateriaEffect_DamageTracker damageTracker;
        public float maxDamage;
        public float currentDamage;

        private static readonly Vector2 BarSize = new Vector2(140f, 12f);
        private static readonly Color BarColor = new Color(0.9f, 0.85f, 0.2f);
        private static readonly Color BarBackgroundColor = new Color(0.15f, 0.15f, 0.15f);

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect barRect = new Rect(rect.x, rect.y + 32f, BarSize.x, BarSize.y);

            // Draw the background
            Widgets.DrawBox(rect);
            GUI.color = Color.white;
            Text.Font = GameFont.Tiny;

            // Draw label
            Widgets.Label(new Rect(rect.x + 5f, rect.y + 5f, rect.width - 10f, rect.height),
                "Stored Damage: " + currentDamage.ToString("F1") + " / " + maxDamage.ToString("F0"));

            // Draw the bar
            Widgets.FillableBar(barRect, currentDamage / maxDamage);

            // Draw health bonus
            float healthBonus = ((MateriaEffectDef_DamageTracker)damageTracker.def).damageToHealthRatio * currentDamage * 100f;
            Widgets.Label(new Rect(rect.x + 5f, rect.y + 50f, rect.width - 10f, rect.height),
                "Health Bonus: +" + healthBonus.ToString("F1") + "%");

            return new GizmoResult(GizmoState.Clear);
        }
    }
}
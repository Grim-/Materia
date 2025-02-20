using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Thor
{
    public class Gizmo_BowModeSelector : Gizmo
    {
        public Verb verbShoot;
        public CompEquippable_BowModeSwitcher comp;

        public float ButtonSize = 32f;
        public float Margin = 8f;
        public float WorkerRowHeight = 32f;
        public Gizmo_BowModeSelector(CompEquippable_BowModeSwitcher comp, Verb verbShoot)
        {
            this.comp = comp;
            this.verbShoot = verbShoot;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f + Margin * 2 + TryGetWorkerExtraWidth();
        }

        private float TryGetWorkerExtraWidth()
        {
            return comp.CurrentModeDef != null && comp.CurrentWorker != null ? comp.CurrentWorker.GetExtraWidth() : 0;
        }

        private float TryGetWorkerExtraHeight()
        {
            return comp.CurrentModeDef != null && comp.CurrentWorker != null ? comp.CurrentWorker.GetExtraHeight() : 0;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            float gizmoWidth = GetWidth(maxWidth);
            Rect gizmoRect = new Rect(topLeft.x, topLeft.y, gizmoWidth, ButtonSize + WorkerRowHeight + Margin * 2 + TryGetWorkerExtraHeight());
            Widgets.DrawWindowBackground(gizmoRect);

            float curX = topLeft.x + Margin;
            float curY = topLeft.y + Margin;

            Rect buttonRect = new Rect(curX, curY, gizmoWidth - Margin * 2, ButtonSize);

            if (comp.CurrentModeDef != null)
            {
                Widgets.DrawWindowBackground(buttonRect);

                float xPos = buttonRect.x + 4f;

                Rect dropdownIconRect = new Rect(
                    xPos,
                    buttonRect.y + (buttonRect.height - 16f) / 2f,
                    16f,
                    16f);
                Widgets.DrawTextureFitted(dropdownIconRect, TexButton.ReorderDown, 1f);
                xPos += 16f + 4f;

                float remainingWidth = buttonRect.xMax - xPos - 4f;
                Rect labelRect = new Rect(xPos, buttonRect.y, remainingWidth, buttonRect.height);

                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, comp.CurrentModeDef.LabelCap);
                Text.Anchor = TextAnchor.UpperLeft;

                if (Widgets.ButtonInvisible(buttonRect))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();

                    foreach (var item in comp.Props.allowedModeDefs)
                    {
                        if (item == comp.CurrentModeDef)
                        {
                            continue;
                        }

                        options.Add(new FloatMenuOption($"{item.LabelCap}", () =>
                        {
                            comp.ApplyBowMode(item);
                        }));
                    }

                    Find.WindowStack.Add(new FloatMenu(options));
                }

                TooltipHandler.TipRegion(buttonRect, $"{comp.CurrentModeDef.LabelCap} \n{comp.CurrentModeDef.description}");
            }

            curY += ButtonSize + Margin;
            Rect workerRect = new Rect(curX, curY, gizmoWidth - Margin * 2, WorkerRowHeight);


            if (comp.CurrentModeDef != null)
            {
                BowModeWorker worker = comp.CurrentWorker;
                if (worker != null)
                {
                    worker.OnGUI(this, gizmoWidth, workerRect);
                }
            }

            return new GizmoResult(GizmoState.Clear);
        }
    }
}
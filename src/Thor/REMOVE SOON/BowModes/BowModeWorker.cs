using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Materia
{

    /// <summary>
    /// Handle drawing and in some cases executing specific actions
    /// </summary>
    public class BowModeWorker : IExposable
    {
        public BowModeDef modeDef;
        public CompEquippable_BowModeSwitcher parentComp;
        private const float SliderHeight = 24f;
        private const float CheckboxSize = 24f;
        private int currentBurstCount;
        private bool burstEnabled;

        public virtual void OnGUI(Gizmo_BowModeSelector parentGizmo, float parentWidth, Rect slotRect)
        {
            float checkboxWidth = CheckboxSize + 4f;
            float sliderWidth = parentWidth - parentGizmo.Margin * 2 - checkboxWidth - 4f;
            float curY = slotRect.y;

            // Checkbox - aligned with label
            Rect checkboxRect = new Rect(
                slotRect.x + parentGizmo.Margin,
                curY + (SliderHeight - CheckboxSize) / 2,
                CheckboxSize,
                CheckboxSize);

            bool prevBurstEnabled = burstEnabled;
            Widgets.Checkbox(checkboxRect.position, ref burstEnabled);
            TooltipHandler.TipRegion(checkboxRect, burstEnabled ? "Burst" : "Single");

            // Handle burst mode change
            if (burstEnabled != prevBurstEnabled)
            {
                if (burstEnabled && parentGizmo.verbShoot.verbProps.burstShotCount <= 1)
                {
                    parentGizmo.verbShoot.verbProps.burstShotCount = 3;
                    currentBurstCount = 3;
                }
                else if (!burstEnabled && parentGizmo.verbShoot.verbProps.burstShotCount > 1)
                {
                    parentGizmo.verbShoot.verbProps.burstShotCount = 1;
                    currentBurstCount = 1;
                }
            }

            Rect sliderRect = new Rect(
                checkboxRect.xMax + 4f,
                curY,
                sliderWidth,
                SliderHeight);

            if (burstEnabled)
            {
                int newBurstCount = (int)Widgets.HorizontalSlider(
                    sliderRect,
                    currentBurstCount,
                    1f,
                    50f,
                    false,
                    currentBurstCount.ToString(),
                    "2",
                    "10",
                    1f);

                if (newBurstCount != currentBurstCount)
                {
                    currentBurstCount = newBurstCount;
                    parentGizmo.verbShoot.verbProps.burstShotCount = currentBurstCount;
                }
            }
        }

        public virtual void OnDraw()
        {

        }

        public virtual float GetExtraHeight()
        {
            return 0f;
        }

        public virtual float GetExtraWidth()
        {
            return 0;
        }

        public virtual IEnumerable<Gizmo> GetGizmos()
        {
            yield break;
        }

        public virtual void Tick() 
        { 

        }

        public virtual void ExposeData()
        { 

        }
    }
}

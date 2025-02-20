using System.Collections.Generic;
using UnityEngine;

namespace Thor
{
    public class CircularMateriaDisplay : MateriaDisplayWorker
    {
        public CircularMateriaDisplay()
        {
        }

        public override Dictionary<MateriaSlot, Rect> PositionSlots(Window_MateriaSelection MateriaWindow, 
            List<MateriaSlot> slots,
            Rect availableSpace,
            float slotSize,
            float spacing)
        {
            var slotPositions = new Dictionary<MateriaSlot, Rect>();
            if (slots == null || slots.Count == 0) return slotPositions;

            // Find center of available space
            Vector2 center = new Vector2(
                availableSpace.x + (availableSpace.width / 2f),
                availableSpace.y + (availableSpace.height / 2f)
            );

            float effectiveSpacing = spacing * MateriaWindow.DisplaySettings.margin;
            float totalCircumference = slots.Count * (slotSize + effectiveSpacing);

            float minRequiredRadius = totalCircumference / (2f * Mathf.PI);

            float maxAllowedRadius = Mathf.Min(availableSpace.width, availableSpace.height) / 2f - slotSize;

            float radius = Mathf.Max(minRequiredRadius, maxAllowedRadius / 2f) * MateriaWindow.DisplaySettings.circleRadiusMultiplier;

            for (int i = 0; i < slots.Count; i++)
            {
                float angle = ((float)i / slots.Count) * Mathf.PI * 2f;

                float x = center.x + Mathf.Cos(angle) * radius - (slotSize / 2f);
                float y = center.y + Mathf.Sin(angle) * radius - (slotSize / 2f);

                slotPositions[slots[i]] = new Rect(x, y, slotSize, slotSize);
            }

            return slotPositions;
        }

        public override float GetLayoutSuitability(List<MateriaSlot> slots)
        {
            if (slots.Count >= 5)
            {
                return 0.8f;
            }
            else if (slots.Count >= 3)
            {
                return 0.5f;
            }
            return 0.2f;
        }
    }
}

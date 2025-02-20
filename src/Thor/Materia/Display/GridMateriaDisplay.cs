using System.Collections.Generic;
using UnityEngine;

namespace Materia
{
    public class GridMateriaDisplay : MateriaDisplayWorker
    {
        private const float HORIZONTAL_PADDING = 10f;
        private const float VERTICAL_PADDING = 10f;

        public override Dictionary<MateriaSlot, Rect> PositionSlots(Window_MateriaSelection MateriaWindow,
            List<MateriaSlot> slots,
            Rect availableSpace,
            float slotSize,
            float spacing)
        {
            var slotPositions = new Dictionary<MateriaSlot, Rect>();
            if (slots == null || slots.Count == 0) return slotPositions;

            float startX = availableSpace.x + HORIZONTAL_PADDING;
            float startY = availableSpace.y + VERTICAL_PADDING;

            // Calculate maximum slots per row
            float effectiveWidth = availableSpace.width - (HORIZONTAL_PADDING * 2);
            int slotsPerRow = Mathf.Max(1, Mathf.FloorToInt(effectiveWidth / (slotSize + spacing)));

            // Position slots in grid layout
            for (int i = 0; i < slots.Count; i++)
            {
                int row = i / slotsPerRow;
                int col = i % slotsPerRow;

                float x = startX + (col * (slotSize + spacing));
                float y = startY + (row * (slotSize + spacing));

                slotPositions[slots[i]] = new Rect(x, y, slotSize, slotSize);
            }

            return slotPositions;
        }

        public override float GetLayoutSuitability(List<MateriaSlot> slots)
        {
            return 0.6f;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Materia
{
    public class GridMateriaDisplay : MateriaDisplayWorker
    {
        public override Dictionary<MateriaSlot, Rect> PositionSlots(
            Window_MateriaSelection MateriaWindow,
            List<MateriaSlot> slots,
            Rect availableSpace,
            Vector2 slotSize,
            Vector2 spacing)
        {
            var slotPositions = new Dictionary<MateriaSlot, Rect>();
            if (slots == null || slots.Count == 0) return slotPositions;

            // Calculate grid dimensions
            float totalSlotAndSpacingWidth = slotSize.x + spacing.x;
            int slotsPerRow = Mathf.Max(1, Mathf.FloorToInt((availableSpace.width + spacing.x) / totalSlotAndSpacingWidth));
            int numberOfRows = Mathf.CeilToInt((float)slots.Count / slotsPerRow);

            // Calculate total size of all slots and spacing
            float totalWidth = (slotsPerRow * slotSize.x) + ((slotsPerRow - 1) * spacing.x);
            float totalHeight = (numberOfRows * slotSize.y) + ((numberOfRows - 1) * spacing.y);

            // Center horizontally
            float startX = availableSpace.x + (availableSpace.width - totalWidth) * 0.5f;


            float usableHeight = availableSpace.height;
            float startY = (usableHeight - totalHeight) * 0.5f;

            // Position each slot
            for (int i = 0; i < slots.Count; i++)
            {
                int row = i / slotsPerRow;
                int col = i % slotsPerRow;

                float x = startX + (col * (slotSize.x + spacing.x));
                float y = startY + (row * (slotSize.y + spacing.y));

                slotPositions[slots[i]] = new Rect(x, y, slotSize.x, slotSize.y);
            }

            return slotPositions;
        }

        public override float GetLayoutSuitability(List<MateriaSlot> slots)
        {
            return 0.6f;
        }
    }
}
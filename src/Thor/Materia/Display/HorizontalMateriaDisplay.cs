using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Materia
{
    public class HorizontalMateriaDisplay : MateriaDisplayWorker
    {
        private const int DEFAULT_SLOTS_PER_ROW = 4;
        private const int DEFAULT_MAX_LINK_LENGTH = 2;

        private int slotsPerRow = DEFAULT_SLOTS_PER_ROW;
        private int maxLinkLength = DEFAULT_MAX_LINK_LENGTH;
        private float rowMargin = 150f;
        private float columnMargin = 20f;

        public int SlotsPerRow
        {
            get => slotsPerRow;
            set => slotsPerRow = Math.Max(1, value);
        }

        public int MaxLinkLength
        {
            get => maxLinkLength;
            set => maxLinkLength = Math.Max(1, value);
        }

        public float RowMargin
        {
            get => rowMargin;
            set => rowMargin = Math.Max(0f, value);
        }

        public float ColumnMargin
        {
            get => columnMargin;
            set => columnMargin = Math.Max(0f, value);
        }

        public override Dictionary<MateriaSlot, Rect> PositionSlots(Window_MateriaSelection MateriaWindow,
            List<MateriaSlot> slots,
            Rect containerRect,
            Vector2 slotSize,
            Vector2 padding)
        {
            Dictionary<MateriaSlot, Rect> result = new Dictionary<MateriaSlot, Rect>();
            List<MateriaSlot> slotsList = slots.ToList();

            if (slotsList.Count == 0)
                return result;

            float labelHeight = MateriaWindow.DisplaySettings.labelHeight;
            float verticalUnitSize = slotSize.x + labelHeight + rowMargin;

            float startX = containerRect.x + padding.x;
            float startY = containerRect.y + padding.y;
            float totalWidth = slotSize.x * slotsPerRow + (padding.x + columnMargin) * (slotsPerRow - 1);

            // Center the grid horizontally if needed
            if (totalWidth < containerRect.width)
            {
                startX += (containerRect.width - totalWidth) / 2f;
            }

            for (int i = 0; i < slotsList.Count; i++)
            {
                int row = i / slotsPerRow;
                int col = i % slotsPerRow;

                float posX = startX + col * (slotSize.x + padding.x + columnMargin);
                float posY = startY + row * verticalUnitSize;

                Rect slotRect = new Rect(posX, posY, slotSize.x, slotSize.x);
                result[slotsList[i]] = slotRect;
            }

            return result;
        }

        public override List<MateriaSlotLinkConfig> GetLinkDefinitions(
            List<MateriaSlot> slots,
            Dictionary<MateriaSlot, Rect> positions)
        {
            List<MateriaSlotLinkConfig> result = new List<MateriaSlotLinkConfig>();

            if (slots.Count < 2)
                return result;

            // Sort slots by their position (left to right, top to bottom)
            List<MateriaSlot> sortedSlots = slots
                .OrderBy(s => positions[s].y)
                .ThenBy(s => positions[s].x)
                .ToList();

            // Create links between slots in each row, up to the maximum link length
            for (int rowIndex = 0; rowIndex < (sortedSlots.Count + slotsPerRow - 1) / slotsPerRow; rowIndex++)
            {
                // Get slots for this row
                int startIndex = rowIndex * slotsPerRow;
                int endIndex = Math.Min(startIndex + slotsPerRow, sortedSlots.Count);

                // For each slot in the row
                for (int i = startIndex; i < endIndex; i++)
                {
                    // Link with subsequent slots up to max link length
                    for (int j = i + 1; j < endIndex && j <= i + maxLinkLength; j++)
                    {
                        result.Add(new MateriaSlotLinkConfig
                        {
                            FirstSlot = sortedSlots[i],
                            SecondSlot = sortedSlots[j],
                            IsVisual = true
                        });
                    }
                }
            }

            return result;
        }

        public override void DrawToolBar(Rect toolbarRect)
        {
   
        }
    }
}
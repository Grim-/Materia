using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Materia
{
    public class StarMateriaDisplay : MateriaDisplayWorker
    {
        public override Dictionary<MateriaSlot, Rect> PositionSlots(
            Window_MateriaSelection MateriaWindow,
            List<MateriaSlot> slots,
            Rect availableSpace,
            Vector2 slotSize,
            Vector2 spacing)
        {
            var slotPositions = new Dictionary<MateriaSlot, Rect>();
            if (slots == null || slots.Count == 0) 
                return slotPositions;

            Vector2 center = new Vector2(
                availableSpace.x + (availableSpace.width / 2f),
                availableSpace.y + (availableSpace.height / 2f)
            );

            if (slots.Count > 0)
            {
                slotPositions[slots[0]] = new Rect(
                    center.x - (slotSize.x / 2f),
                    center.y - (slotSize.y / 2f),
                    slotSize.x,
                    slotSize.y
                );
            }
            if (slots.Count > 1)
            {
                float maxSlotDimension = Mathf.Max(slotSize.x, slotSize.y);
                float maxSpacingDimension = Mathf.Max(spacing.x, spacing.y);
                float itemSpace = maxSlotDimension + maxSpacingDimension;

                float radius = Mathf.Min(
                    (availableSpace.width - itemSpace) / 3f,
                    (availableSpace.height - itemSpace) / 3f
                );
                for (int i = 1; i < slots.Count; i++)
                {
                    float angle = ((float)(i - 1) / (slots.Count - 1)) * Mathf.PI * 2f;
                    float x = center.x + Mathf.Cos(angle) * radius - (slotSize.x / 2f);
                    float y = center.y + Mathf.Sin(angle) * radius - (slotSize.y / 2f);
                    slotPositions[slots[i]] = new Rect(x, y, slotSize.x, slotSize.y);
                }
            }

            return slotPositions;
        }
    }
}

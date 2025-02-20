using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Thor
{
    public class StarMateriaDisplay : MateriaDisplayWorker
    {
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

            // First slot in center
            if (slots.Count > 0)
            {
                slotPositions[slots[0]] = new Rect(
                    center.x - (slotSize / 2f),
                    center.y - (slotSize / 2f),
                    slotSize,
                    slotSize
                );
            }

            // Remaining slots in circle around center
            if (slots.Count > 1)
            {
                float radius = Mathf.Min(availableSpace.width, availableSpace.height) / 3f;
                for (int i = 1; i < slots.Count; i++)
                {
                    float angle = ((float)(i - 1) / (slots.Count - 1)) * Mathf.PI * 2f;

                    float x = center.x + Mathf.Cos(angle) * radius - (slotSize / 2f);
                    float y = center.y + Mathf.Sin(angle) * radius - (slotSize / 2f);

                    slotPositions[slots[i]] = new Rect(x, y, slotSize, slotSize);
                }
            }

            return slotPositions;
        }

        public override float GetLayoutSuitability(List<MateriaSlot> slots)
        {
            if (slots.Count > 0)
            {
                bool hasCentralSlot = false;
                bool hasValidSatellites = false;

                // Check for a different colored/sized first slot
                if (slots.Count > 1 && slots[0].SlotType != null)
                {
                    var firstType = slots[0].SlotType;
                    var satelliteTypes = slots.Skip(1).Select(s => s.SlotType).Distinct().ToList();

                    hasCentralSlot = !satelliteTypes.Contains(firstType);
                    hasValidSatellites = satelliteTypes.Count <= 3;
                }

                if (hasCentralSlot && hasValidSatellites && slots.Count >= 3 && slots.Count <= 9)
                {
                    return 0.9f;
                }
                else if (slots.Count >= 3 && slots.Count <= 6)
                {
                    return 0.7f;
                }
            }

            return 0.3f;
        }
    }
}

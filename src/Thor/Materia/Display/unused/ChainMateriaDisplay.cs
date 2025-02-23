//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using Verse;

//namespace Materia
//{
//    public class ChainMateriaDisplay : MateriaDisplayWorker
//    {
//        public override Dictionary<MateriaSlot, Rect> PositionSlots(Window_MateriaSelection MateriaWindow,
//            List<MateriaSlot> slots,
//            Rect availableSpace,
//            float slotSize,
//            float spacing)
//        {
//            var slotPositions = new Dictionary<MateriaSlot, Rect>();
//            if (slots == null || slots.Count == 0) return slotPositions;

//            float startX = availableSpace.x + (availableSpace.width - (slots.Count * (slotSize + spacing) - spacing)) / 2;
//            float y = availableSpace.y + (availableSpace.height - slotSize) / 2;

//            for (int i = 0; i < slots.Count; i++)
//            {
//                float x = startX + (i * (slotSize + spacing));
//                slotPositions[slots[i]] = new Rect(x, y, slotSize, slotSize);
//            }

//            return slotPositions;
//        }

//        public override float GetLayoutSuitability(List<MateriaSlot> slots)
//        {
//            if (slots.Count <= 5)
//            {
//                bool hasChainLinks = false;
//                foreach (var slot in slots)
//                {
//                    if (slot.LinkingRules != null && slot.LinkingRules.Any())
//                    {
//                        hasChainLinks = true;
//                        break;
//                    }
//                }

//                if (hasChainLinks)
//                {
//                    return 0.75f;
//                }

//                return 0.7f; 
//            }

//            return 0.4f; 
//        }
//    }
//}

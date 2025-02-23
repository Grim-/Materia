using System.Collections.Generic;
using UnityEngine;

namespace Materia
{
    public abstract class MateriaDisplayWorker
    {
        public abstract Dictionary<MateriaSlot, Rect> PositionSlots(Window_MateriaSelection MateriaWindow,
            List<MateriaSlot> slots,
            Rect availableSpace,
            Vector2 slotSize,
            Vector2 spacing);

        public virtual void DrawToolBar(Rect toolbarRect) { }

        public virtual void DrawSlot(Rect slotRect, MateriaSlot slot) { }

        public virtual float GetLayoutSuitability(List<MateriaSlot> slots)
        {
            return 0.5f; 
        }

        public virtual List<MateriaSlotLinkConfig> GetLinkDefinitions(List<MateriaSlot> slots, Dictionary<MateriaSlot, Rect> positions)
        {
            return new List<MateriaSlotLinkConfig>();
        }

    }

    public class MateriaSlotLinkConfig
    {
        public MateriaSlot FirstSlot;
        public MateriaSlot SecondSlot;
        public bool IsVisual;
    }
}

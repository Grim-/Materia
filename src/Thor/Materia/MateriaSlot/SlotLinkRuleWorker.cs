using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Materia
{
    //handles verifying a valid link and creating that link in Comp_Materia
    public abstract class SlotLinkRuleWorker
    {
        public Comp_MateriaComp MateriaComp;
        public MateriaSlot MateriaSlot;
        public SlotLinkRuleDef Def;

        public virtual bool ValidateSlots(List<MateriaSlot> potentialSlots, List<MateriaSlotLink> activeLinks)
        {
            if (potentialSlots == null || potentialSlots.Count < 2)
                return false;

            // Verify all slots exist and are occupied
            if (potentialSlots.Any(s => s == null || !s.IsOccupied)) return false;

            return true;
        }

        // Base class - override in specific rule defs
        public abstract void CreateLinks(List<MateriaSlot> slots, List<MateriaSlotLink> activeLinks);
    }

    //only pairs with the next available node
    public class SlotLinkRuleWorker_PairForward : SlotLinkRuleWorker
    {
        public override bool ValidateSlots(List<MateriaSlot> potentialSlots, List<MateriaSlotLink> activeLinks)
        {
            if (!base.ValidateSlots(potentialSlots, activeLinks)) return false;

            if (potentialSlots.Count != 2) return false;

            var origin = potentialSlots[0];
            var destination = potentialSlots[1];

            if (!origin.SlottedMateria.def.CanLinkWith(destination.SlottedMateria.def)) return false;

            if (activeLinks.Any(link =>
                link.OriginSlot == origin ||
                link.OriginSlot == destination ||
                link.DestinationSlot == origin ||
                link.DestinationSlot == destination)) return false;

            return true;
        }

        public override void CreateLinks(List<MateriaSlot> slots, List<MateriaSlotLink> activeLinks)
        {
            if (!ValidateSlots(slots, activeLinks)) return;

            activeLinks.Add(new MateriaSlotLink
            {
                OriginSlot = slots[0],
                DestinationSlot = slots[1]
            });
        }
    }

    //can pair with the previous and next nodes
    public class SlotLinkRuleWorker_DoubleLink : SlotLinkRuleWorker
    {
        public override bool ValidateSlots(List<MateriaSlot> potentialSlots, List<MateriaSlotLink> activeLinks)
        {
            if (!base.ValidateSlots(potentialSlots, activeLinks)) return false;

            if (potentialSlots.Count != 2) return false;

            var first = potentialSlots[0];
            var second = potentialSlots[1];

            if (!first.SlottedMateria.def.CanLinkWith(second.SlottedMateria.def)) return false;
            if (!second.SlottedMateria.def.CanLinkWith(first.SlottedMateria.def)) return false;

            if (activeLinks.Any(link =>
                link.OriginSlot == first ||
                link.OriginSlot == second ||
                link.DestinationSlot == first ||
                link.DestinationSlot == second)) return false;

            return true;
        }

        public override void CreateLinks(List<MateriaSlot> slots, List<MateriaSlotLink> activeLinks)
        {
            if (!ValidateSlots(slots, activeLinks)) return;

            activeLinks.Add(new MateriaSlotLink { OriginSlot = slots[0], DestinationSlot = slots[1], Worker = this });
            activeLinks.Add(new MateriaSlotLink { OriginSlot = slots[1], DestinationSlot = slots[0], Worker = this });
        }
    }
}

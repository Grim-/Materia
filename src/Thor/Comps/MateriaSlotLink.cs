using Verse;

namespace Thor
{
    public class MateriaSlotLink : IExposable
    {
        public MateriaSlot OriginSlot;
        public MateriaSlot DestinationSlot;

        public SlotLinkRuleWorker Worker;

        public bool IsValidLink()
        {
            return OriginSlot.SlottedMateria != null &&
                   DestinationSlot.SlottedMateria != null &&
                   OriginSlot.SlottedMateria.def.CanLinkWith(DestinationSlot.SlottedMateria.def);
        }

        public void ExposeData()
        {
          
        }
    }
}

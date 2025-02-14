using Verse;

namespace Thor
{
    public class MateriaSlot : IExposable
    {
        public MateriaTypeDef SlotType;
        public MateriaInstance SlottedMateria;

        protected ThingWithComps _ParentThing;
        public ThingWithComps ParentThing => _ParentThing;

        protected Comp_MateriaComp _MateriaComp;
        public Comp_MateriaComp MateriaComp => _MateriaComp;

        public bool IsOccupied => SlottedMateria != null;

        public MateriaSlot()
        {
        }

        public MateriaSlot(ThingWithComps parentThing, Comp_MateriaComp materiaComp, MateriaTypeDef type)
        {
            _MateriaComp = materiaComp;
            _ParentThing = parentThing;
            SlotType = type;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref SlotType, "SlotType");
            Scribe_Deep.Look(ref SlottedMateria, "SlottedMateria");
            Scribe_References.Look(ref _ParentThing, "ParentThing");
        }

        public void EquipMateria(MateriaDef materiaDef)
        {
            if (!CanEquipMateria(materiaDef))
                return;

            SlottedMateria = new MateriaInstance(materiaDef, this);
        }

        public bool CanAccept(MateriaDef Materia)
        {
            return SlotType != null ? Materia.SlotType == SlotType : true;
        }

        public void UnequipMateria()
        {
            if (SlottedMateria == null)
                return;

            SlottedMateria = null;
        }

        public bool CanEquipMateria(MateriaDef materia)
        {
            return !IsOccupied && CanAccept(materia);
        }
    }

}

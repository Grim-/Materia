using System.Linq;
using UnityEngine;
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



        public string EffectDescription => SlottedMateria != null ? string.Join("\n", SlottedMateria.ActiveEffects.Select(x => x.def.EffectDescription)) : null;


        protected int _SlotLevel = 1;
        public int SlotLevel => _SlotLevel;

        public MateriaSlot()
        {
        }

        public MateriaSlot(ThingWithComps parentThing, Comp_MateriaComp materiaComp, MateriaTypeDef type, int slotLevel = 1)
        {
            _MateriaComp = materiaComp;
            _ParentThing = parentThing;
            SlotType = type;
            
        }


        public void EquipMateria(MateriaDef materiaDef)
        {
            if (!CanEquipMateria(materiaDef))
                return;

            SlottedMateria = new MateriaInstance(materiaDef, this);
            
            if (MateriaComp.HasEquipOwner)
            {
                SlottedMateria.Notify_Equipped(MateriaComp.EquippedPawn);
            }
        }

        public bool CanAccept(MateriaDef Materia)
        {
            return SlotType != null ? Materia.SlotType == SlotType : true;
        }

        public void UnequipMateria()
        {
            if (SlottedMateria == null)
                return;


            if (MateriaComp.HasEquipOwner)
            {
                SlottedMateria.Notify_Unequipped(MateriaComp.EquippedPawn);
            }

            SlottedMateria = null;
        }


        public void AddLevel(int levelsToAdd)
        {
            this._SlotLevel += levelsToAdd;
        }

        public void SetLevel(int level)
        {
            this._SlotLevel = level;
        }

        public void RemoveLevel(int levelsToRemove)
        {
            this._SlotLevel -= levelsToRemove;
            this._SlotLevel = Mathf.Clamp(_SlotLevel, 1, 1000);
        }

        public void Reset()
        {
            this._SlotLevel = 1;
        }

        public bool CanEquipMateria(MateriaDef materia)
        {
            return !IsOccupied && CanAccept(materia);
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref SlotType, "SlotType");
            Scribe_Deep.Look(ref SlottedMateria, "SlottedMateria");
            Scribe_References.Look(ref _ParentThing, "ParentThing");
            Scribe_Values.Look(ref _SlotLevel, "SlotLevel");
        }

    }

}

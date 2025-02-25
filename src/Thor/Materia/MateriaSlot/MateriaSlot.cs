using LudeonTK;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Materia
{
    public class MateriaSlot : IExposable
    {
        protected MateriaInstance _SlottedMateria;
        public MateriaInstance SlottedMateria
        {
            get
            {
                return _SlottedMateria;
            }
        }

        protected ThingWithComps _ParentThing;
        public ThingWithComps ParentThing
        {
            get
            {
                return _ParentThing;
            }
        }

        protected Comp_MateriaComp _MateriaComp;
        public Comp_MateriaComp MateriaComp => _MateriaComp;
        public bool IsOccupied => _SlottedMateria != null;

        protected List<SlotLinkRuleWorker> _linkRules = new List<SlotLinkRuleWorker>();
        public List<SlotLinkRuleWorker> LinkingRules
        {
            get
            {
                return _linkRules;
            }
        }

        public string EffectDescription => _SlottedMateria != null ? string.Join("\n", _SlottedMateria.ActiveEffects.Select(x => x.def.EffectDescription)) : null;

        //public MateriaTypeDef SlotType;
        protected MateriaTypeDef _SlotType;
        public MateriaTypeDef SlotType
        {
            get
            {
                return _SlotType;
            }
        }


        protected int _SlotLevel = 1;
        public int SlotLevel
        {
            get
            {
                return _SlotLevel;
            }
        }

        protected int _SlotIndex = -1;
        public int SlotIndex
        {
            get
            {
                if (MateriaComp == null)
                {
                    return -1;
                }

                return MateriaComp.MateriaSlots.IndexOf(this);
            }
        }

        protected bool _IsSlotLocked = false;
        public bool IsSlotLocked
        {
            get
            {
                return _IsSlotLocked;
            }
        }

        public MateriaSlot()
        {
        }

        public MateriaSlot(ThingWithComps parentThing, Comp_MateriaComp materiaComp, MateriaTypeDef type, int slotLevel = 1)
        {
            _MateriaComp = materiaComp;
            _ParentThing = parentThing;
            _SlotType = type;
            _SlotLevel = slotLevel;
        }



        public void EquipMateria(MateriaDef materiaDef, bool force = false)
        {
            if (!CanEquipMateria(materiaDef) && !force)
                return;

            _SlottedMateria = new MateriaInstance(materiaDef, this);

            if (MateriaComp.HasEquipOwner)
            {
                _SlottedMateria.Notify_Equipped(MateriaComp.EquippedPawn);
            }
        }

        public void UnequipMateria()
        {
            if (_SlottedMateria == null)
                return;

            _SlottedMateria = null;
        }

        public bool CanAccept(MateriaDef Materia)
        {
            if (Materia == null)
                return false;

            return AcceptsType(Materia.SlotType) && IsValidLevel(Materia) && IsValidEquipmentType(Materia, this.ParentThing) && !IsSlotLocked;
        }


        public bool AcceptsType(MateriaTypeDef materiaTypeDef)
        {
            if (SlotType != null)
            {
                return materiaTypeDef == SlotType;
            }

            return true;
        }

        public bool IsValidLevel(MateriaDef Materia)
        {
            return this.SlotLevel >= Materia.slotLevel;
        }

        public bool IsValidEquipmentType(MateriaDef Materia, Thing Thing)
        {
            return Materia.materiaCategories == null ||
            (Thing.def.IsMeleeWeapon && Materia.materiaCategories.Contains(MateriaDefOf.MateriaCategory_Melee)) ||
            (Thing.def.IsRangedWeapon && Materia.materiaCategories.Contains(MateriaDefOf.MateriaCategory_Ranged)) ||
            (Thing.def.IsApparel && Materia.materiaCategories.Contains(MateriaDefOf.MateriaCategory_Armor));
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

        public void SetLockStatus(bool IsLocked)
        {
            this._IsSlotLocked = IsLocked;
        }

        public bool CanEquipMateria(MateriaDef materia)
        {
            return !IsOccupied && CanAccept(materia);
        }


        public bool AddLinkRule(SlotLinkRuleDef linkRuleDef)
        {
            this._linkRules.Add(linkRuleDef.CreateWorker(MateriaComp, this));
            return true;
        }

        public bool CanLinkTo(MateriaSlot other, List<MateriaSlotLink> activeLinks)
        {
            return _linkRules.Any(rule =>
                rule.ValidateSlots(new List<MateriaSlot> { this, other }, activeLinks));
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref _SlotType, "SlotType");
            Scribe_Deep.Look(ref _SlottedMateria, "SlottedMateria");
            Scribe_References.Look(ref _ParentThing, "ParentThing");
            Scribe_Values.Look(ref _SlotLevel, "SlotLevel");
            Scribe_Values.Look(ref _SlotIndex, "SlotIndex");
            Scribe_Values.Look(ref _IsSlotLocked, "SlotLocked");
        }
    }
}

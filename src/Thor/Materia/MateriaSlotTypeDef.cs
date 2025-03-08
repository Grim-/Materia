using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Materia
{
    public class MateriaSlotTypeDef : Def
    {
        public Color materiaTextColor;
        public List<MateriaTypeDef> acceptableMateriaTypes;
        public List<SlotLimit> slotLimits;
        public float baseWeight = 1f;

        public List<MateriaWeight> weaponMateriaWeights;
        public List<MateriaWeight> armourMateriaWeights;


        public bool IsValidForSlotCreation(ThingWithComps parent, Comp_Materia comp)
        {
            if (slotLimits == null)
                return true;

            foreach (var limit in slotLimits)
            {
                bool matchesCategory = parent.HasThingCategoryOrParent(limit.ThingCategory);
                if (matchesCategory)
                {
                    if (limit.maxSlots == 0)
                    {
                        return false;
                    }

                    if (comp.OccupiedSlotsCount >= limit.maxSlots)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool IsAcceptableMateriaType(MateriaTypeDef materiaDef)
        {
            if (acceptableMateriaTypes == null)
            {
                return true;
            }

            return acceptableMateriaTypes != null && acceptableMateriaTypes.Contains(materiaDef);
        }
        public static MateriaSlotTypeDef SelectMateriaType(Dictionary<MateriaSlotTypeDef, float> weightedTypes, List<MateriaSlotTypeDef> disallowedTypes = null)
        {
            if (weightedTypes == null || weightedTypes.Count == 0)
                return null;

            var validTypes = weightedTypes.Keys.ToList();
            if (disallowedTypes != null && disallowedTypes.Count > 0)
                validTypes.RemoveAll(t => disallowedTypes.Contains(t));

            if (validTypes.Count == 0)
                return null;

            float roll = Rand.Value;
            float cumulative = 0f;

            foreach (var type in validTypes)
            {
                if (roll <= cumulative)
                {
                    return type;
                }
            }
            return validTypes.RandomElement();
        }
    }

    public class MateriaWeight
    {
        public MateriaTypeDef materiaType;
        public float weight = 1;
    }
}

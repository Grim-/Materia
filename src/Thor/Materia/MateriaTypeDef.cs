using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Materia
{
    public class MateriaTypeDef : Def
    {
        public Color materiaTextColor;
        public float baseWeight = 1f;
        public List<SlotLimit> slotLimits;


        public List<MateriaTypeDef> countsAsMateria;

        public bool IsValidForSlotCreation(ThingWithComps parent, Comp_Materia comp)
        {
            if (slotLimits == null)
                return true;

            foreach (var limit in slotLimits)
            {
                bool matchesCategory = HasThingCategoryOrParent(parent, limit.ThingCategory);
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

        // Helper method to check if a thing has a specific ThingCategory or any of its parent categories
        private bool HasThingCategoryOrParent(Thing thing, ThingCategoryDef categoryToCheck)
        {
            if (thing.HasThingCategory(categoryToCheck))
                return true;

            foreach (ThingCategoryDef parentCategory in categoryToCheck.Parents)
            {
                if (thing.HasThingCategory(parentCategory))
                    return true;
            }

            foreach (ThingCategoryDef childCategory in categoryToCheck.ThisAndChildCategoryDefs)
            {
                if (thing.HasThingCategory(childCategory))
                    return true;
            }

            return false;
        }

        public static MateriaTypeDef SelectMateriaType(Dictionary<MateriaTypeDef, float> weightedTypes, List<MateriaTypeDef> disallowedTypes = null)
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
}

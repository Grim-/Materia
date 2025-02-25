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

        public bool IsValidForSlotCreation(ThingWithComps parent, Comp_MateriaComp comp)
        {
            if (slotLimits == null)
                return true;

            foreach (var limit in slotLimits)
            {
                if (parent.HasThingCategory(limit.ThingCategory))
                {
                    if (limit.maxSlots == 0)
                    {
                        return false;
                    }
                    var currentCount = comp.GetMateriaCountOfType(this);
                    if (currentCount >= limit.maxSlots)
                        return false;
                }
            }

            return true;
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

            float totalWeight = 0f;
            foreach (var type in validTypes)
            {
                totalWeight += weightedTypes[type];
            }

            Dictionary<MateriaTypeDef, float> normalizedWeights = new Dictionary<MateriaTypeDef, float>();
            foreach (var type in validTypes)
            {
                normalizedWeights[type] = weightedTypes[type] / totalWeight;
            }

            float roll = Rand.Value;
            float cumulative = 0f;

            foreach (var type in validTypes)
            {
                cumulative += normalizedWeights[type];
                if (roll <= cumulative)
                {
                    return type;
                }
            }
            return validTypes.RandomElement();
        }
    }
}

using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Thor
{
    public static class MateriaSlotGenerator
    {
        private const float TECH_LEVEL_MULTIPLIER = 0.5f;
        private const float MARKET_VALUE_MULTIPLIER = 0.0008f;
        private const float QUALITY_MULTIPLIER = 0.3f;

        private const int MIN_SLOTS = 1;
        private const int MAX_SLOTS = 6;

        private static readonly Dictionary<TechLevel, int> BASE_TECH_SLOTS = new Dictionary<TechLevel, int>
        {
            { TechLevel.Animal, 0 },
            { TechLevel.Neolithic, 1 },
            { TechLevel.Medieval, 2 },
            { TechLevel.Industrial, 2 },
            { TechLevel.Spacer, 3 },
            { TechLevel.Ultra, 3 },
            { TechLevel.Archotech, 4 }
        };

        private static readonly Dictionary<string, float> WEAPON_TAG_MODIFIERS = new Dictionary<string, float>
        {
            { "Bladelink", 2.0f },
            { "Noble", 1.5f },
            { "ExoticMiitary", 1.3f },
            { "IndustrialGunAdvanced", 1.2f },
            { "SpacerGun", 1.4f },
            { "UltratechMelee", 1.5f },
            { "SimpleGun", 0.8f },
            { "NeolithicMelee", 0.7f },
            { "MedievalMelee", 0.9f }
        };

        public static List<MateriaSlot> GenerateSlots(ThingWithComps parent, Comp_MateriaComp comp, QualityCategory? quality = null)
        {
            if (!parent.def.IsWeapon)
                return new List<MateriaSlot>();

            float baseScore = CalculateBaseScore(parent.def);

            if (quality.HasValue)
            {
                baseScore *= 1f + (((int)quality.Value - 2) * QUALITY_MULTIPLIER);
            }

            int totalSlots = DetermineSlotCount(baseScore);
            var slots = new List<MateriaSlot>();
            DistributeSlotTypes(slots, totalSlots, parent.def, parent, comp);

            return slots;
        }

        private static float CalculateBaseScore(ThingDef weaponDef)
        {
            float score = BASE_TECH_SLOTS[weaponDef.techLevel];
            score += weaponDef.BaseMarketValue * MARKET_VALUE_MULTIPLIER;

            if (weaponDef.weaponTags != null)
            {
                foreach (string tag in weaponDef.weaponTags)
                {
                    if (WEAPON_TAG_MODIFIERS.TryGetValue(tag, out float modifier))
                    {
                        score *= modifier;
                    }
                }
            }

            if (weaponDef.IsRangedWeapon)
            {
                score *= 1.1f;
                if (weaponDef.Verbs?.Any(v => v.warmupTime >= 1f) == true)
                {
                    score *= 0.9f;
                }
            }
            else
            {
                var meleeDamageAmount = weaponDef.tools?.Average(t => t.power) ?? 1f;
                if (meleeDamageAmount > 20f)
                {
                    score *= 1.2f;
                }
            }

            if (weaponDef.techHediffsTags != null)
            {
                score *= 1.5f;
            }

            if (weaponDef.recipeMaker?.researchPrerequisite != null)
            {
                score *= 1.2f;
            }

            return score;
        }

        private static int DetermineSlotCount(float baseScore)
        {
            float slotCount = Mathf.Log(baseScore + 1, 2) + MIN_SLOTS;
            return Mathf.Clamp(Mathf.RoundToInt(slotCount), MIN_SLOTS, MAX_SLOTS);
        }

        private static void DistributeSlotTypes(List<MateriaSlot> slots, int totalSlots, ThingDef weaponDef, ThingWithComps parent, Comp_MateriaComp comp)
        {
            var damageType = DefDatabase<MateriaTypeDef>.GetNamed("RedMateria");
            var specialType = DefDatabase<MateriaTypeDef>.GetNamed("PurpleMateria");
            var statType = DefDatabase<MateriaTypeDef>.GetNamed("BlueMateria");
            var lifeType = DefDatabase<MateriaTypeDef>.GetNamed("GreenMateria");

            var distribution = new Dictionary<MateriaTypeDef, float>();

            if (weaponDef.IsRangedWeapon)
            {
                distribution.Add(damageType, 0.45f);
                distribution.Add(statType, 0.35f);
                distribution.Add(specialType, 0.10f);
                distribution.Add(lifeType, 0.10f);

                if (weaponDef.weaponTags?.Contains("SpacerGun") == true)
                {
                    distribution[specialType] = 0.30f;
                    distribution[damageType] = 0.40f;
                    distribution[statType] = 0.30f;
                    distribution[lifeType]= 0.20f;
                }
            }
            else
            {
                distribution.Add(specialType, 0.40f);
                distribution.Add(damageType, 0.40f);
                distribution.Add(statType, 0.10f);
                distribution.Add(lifeType, 0.10f);

                if (weaponDef.weaponTags?.Contains("UltratechMelee") == true)
                {
                    distribution[specialType] = 0.45f;
                    distribution[damageType] = 0.35f;
                    distribution[statType] = 0.10f;
                    distribution[lifeType] = 0.10f;
                }
            }

            for (int i = 0; i < totalSlots; i++)
            {
                float roll = Rand.Value;
                float cumulative = 0f;

                foreach (var kvp in distribution)
                {
                    cumulative += kvp.Value;
                    if (roll <= cumulative)
                    {
                        slots.Add(new MateriaSlot(parent, comp, kvp.Key, slots.Count + 1));
                        break;
                    }
                }
            }

            // Ensure at least one damage slot for higher tech weapons
            if (totalSlots >= 2 && !slots.Any(s => s.SlotType == damageType) &&
                weaponDef.techLevel >= TechLevel.Industrial)
            {
                slots[slots.Count - 1] = new MateriaSlot(parent, comp, damageType, slots.Count - 1);
            }
        }
    }
}

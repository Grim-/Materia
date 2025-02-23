using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Random = UnityEngine.Random;

namespace Materia
{
    public static class MateriaSlotGenerator
    {
        public static List<MateriaSlot> GenerateSlots(ThingWithComps parent, Comp_MateriaComp comp, QualityCategory? quality = null)
        {
            if (parent == null)
                return null;
            if (comp == null)
                return null;

            //Log.Message($"Gen Materia Slots for {parent.Label}");
            var baseScore = CalculateBaseScore(parent, parent.def, quality);
            //Log.Message($"  BaseScore: {baseScore}");
            var maxSlots = GetMaxSlotsForCategory(parent.def, quality, baseScore);
            //Log.Message($"  MaximumSlots: {maxSlots}");
            List<MateriaSlot> generatedSlots = GenerateSlotDistribution(maxSlots, parent, comp);
            return generatedSlots;
        }

        public static int GetMaxSlotsForCategory(ThingDef thingDef, QualityCategory? quality, float score)
        {
            if (thingDef == null)
                return 0;

            int slotCap = GetSlotCap(thingDef);

            int maxSlots = 0;

            if (score >= 90)
            {
                maxSlots = slotCap;
            }
            else if (score >= 70)
            {
                maxSlots = Random.Range(slotCap - 1, slotCap);
            }
            else if (score >= 50)
            {
                maxSlots = Random.Range(0, 2);
            }
            else
            {
                maxSlots = Random.Range(0, 1);
            }
            return maxSlots;
        }

        public static int GetSlotCap(ThingDef def)
        {
            var config = MateriaDefOf.MateriaGenerationConfig;
            if (def.IsApparel && def.apparel != null)
            {
                if (def.apparel.bodyPartGroups != null && def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) || def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs))
                {
                    return config.maxArmourSlots;
                }
            }
            else if (def.IsApparel && def.apparel != null && def.apparel.layers.Contains(ApparelLayerDefOf.Belt))
            {
                return config.maxUtilityArmourSlots;
            }
            else if (def.IsWeapon)
            {
                return config.maxWeaponSlots;
            }

            return 0;
        }


        public static float GetTechLevelScore(ThingDef def)
        {
            if ((int)def.techLevel > (int)TechLevel.Industrial)
            {
                return (int)def.techLevel * 10;
            }


            return 0;
        }

        public static float GetMarketValueScore(float baseMarketValue, float valueMulti = 0)
        {
            return Mathf.Clamp(baseMarketValue * (valueMulti != 0 ? valueMulti : MateriaDefOf.MateriaGenerationConfig.marketValueMultiplier), 0, 20f);
        }

        public static float GetStuffScore(Thing thing)
        {
            if (!thing.def.MadeFromStuff || thing.Stuff == null || thing.Stuff.stuffProps == null)
                return 0;
            return GetMarketValueScore(thing.Stuff.BaseMarketValue, 0.09f);
        }


        //Every category level above normal +5

        public static float GetQualityScore(QualityCategory? quality)
        {
            if (quality.HasValue && quality.Value > QualityCategory.Excellent)
            {
                return (int)quality.Value * 10f;
            }
            else if (quality.HasValue && quality.Value >= QualityCategory.Normal)
            {
                return (int)quality.Value * 5f;
            }

            return 0;
        }

        public static float GetResearchScore(ThingDef def, float clampedMax = 15)
        {
            if (def.recipeMaker?.researchPrerequisite != null && def.recipeMaker.researchPrerequisite.techLevel >= TechLevel.Industrial)
            {
                return Mathf.Clamp((int)def.recipeMaker.researchPrerequisite.techLevel * 3, 0, clampedMax);
            }
            else
            {
                return 0;
            }
        }

        public static float GetArmorScore(ThingDef def)
        {
            if (!def.IsApparel || def.apparel == null) return 0;
            return (def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) ||
                    def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs)) ? 10f : -20f;
        }

        public static float CalculateBaseScore(Thing thing, ThingDef def, QualityCategory? quality)
        {
            if (def == null) return 1f;

            float score = GetTechLevelScore(def) +
                         GetMarketValueScore(def.BaseMarketValue) +
                         GetStuffScore(thing) +
                         GetQualityScore(quality) +
                         GetResearchScore(def) +
                         GetArmorScore(def);

            float finalScore = Mathf.Clamp(score, 0, 100f);
            return finalScore;
        }


        public static int GetGeneratedSlotLevel(ThingDef def, QualityCategory? quality, MateriaTypeDef materiaType)
        {
            int maxPossibleSlotLevel = GetMaximumPossibleSlotLevel(def, quality);
            int minimumLevel = 1;

            float SlotLevelScore = GetSlotLevelScore(def, quality);

            if (SlotLevelScore >= 90) minimumLevel = 4;
            else if (SlotLevelScore >= 70) minimumLevel = 3;
            else if (SlotLevelScore >= 40) minimumLevel = 2;

            minimumLevel = Mathf.Clamp(minimumLevel, 1, maxPossibleSlotLevel);
            int chosenLevel = Random.Range(minimumLevel, maxPossibleSlotLevel + 1);

            Log.Message($"SlotLevel: {def.LabelCap} Chosen Level:{chosenLevel} (min:{minimumLevel}, max:{maxPossibleSlotLevel})");

            return chosenLevel;
        }

        public static float GetSlotLevelScore(ThingDef def, QualityCategory? quality)
        {
            float score = 0f;
            float techLevelScore = 0;
            if (def.techLevel != TechLevel.Animal && def.techLevel != TechLevel.Undefined)
            {
                techLevelScore = (int)def.techLevel * 15;
            }
            score += techLevelScore;

            float qualityScore = 0;
            if (quality.HasValue)
            {
                qualityScore = (int)quality.Value * 3f;
            }
            score += qualityScore;

            score = Mathf.Clamp(score, 0, 100);

            Log.Message($"SlotLevel: {def.LabelCap} Score  \nTech:{def.techLevel}({techLevelScore}) \nQuality:{quality}({qualityScore}) \nScore:{score}");
            return score;
        }

        public static int GetMaximumPossibleSlotLevel(ThingDef def, QualityCategory? quality)
        {


            return def.techLevel != TechLevel.Archotech && def.techLevel != TechLevel.Ultra ? 3 : 4;
        }


        private static List<MateriaSlot> GenerateSlotDistribution(int totalSlots, ThingWithComps parent, Comp_MateriaComp comp)
        {
            var slots = new List<MateriaSlot>();
            if (totalSlots <= 0)
                return slots;

            var materiaTypes = DefDatabase<MateriaTypeDef>.AllDefs.Where(x => IsValidForSlotCreation(parent, comp, x)).ToList();
            var weightedTypes = CalculateTypeWeights(materiaTypes, parent.def);

            for (int i = 0; i < totalSlots; i++)
            {
                var selectedType = SelectMateriaType(weightedTypes);
                if (selectedType != null)
                {
                    int newSlotLevel = GetGeneratedSlotLevel(parent.def, parent.TryGetComp<CompQuality>()?.Quality, selectedType);
                    var slot = CreateMateriaSlot(parent, comp, selectedType, newSlotLevel);
                    if (slot != null)
                    {
                        slots.Add(slot);
                        Log.Message($"Slot {slot.SlotType.LabelCap} {slot.SlotLevel} added to {parent.Label}");
                    }
                }
            }

            return slots;
        }

        private static Dictionary<MateriaTypeDef, float> CalculateTypeWeights(List<MateriaTypeDef> types, ThingDef def, List<MateriaTypeDef> disallowedTypes = null)
        {
            var weights = new Dictionary<MateriaTypeDef, float>();
            float totalWeight = 0f;

            foreach (var type in types)
            {
                float weight = CalculateIndividualTypeWeight(type, def);
                weights[type] = weight;
                totalWeight += weight;
            }

            // Normalize weights
            if (totalWeight > 0)
            {
                foreach (var key in weights.Keys.ToList())
                {
                    weights[key] /= totalWeight;
                }
            }

            return weights;
        }

        private static float CalculateIndividualTypeWeight(MateriaTypeDef type, ThingDef def)
        {
            float weight = type.baseWeight;

            // Apply weapon type multiplier
            weight *= def.IsRangedWeapon ? type.rangedWeaponMultiplier : type.meleeWeaponMultiplier;

            // Apply tech level modifier
            var techWeight = type.techLevelWeights?.FirstOrDefault(w => w.techLevel == def.techLevel);
            if (techWeight != null)
            {
                weight *= techWeight.weight;
            }

            // Apply weapon tag modifiers
            if (def.weaponTags != null)
            {
                foreach (string tag in def.weaponTags)
                {
                    var tagWeight = type.weaponTagWeights?.FirstOrDefault(w => w.tag == tag);
                    if (tagWeight != null)
                    {
                        weight *= tagWeight.weight;
                    }
                }
            }

            return weight;
        }

        private static MateriaTypeDef SelectMateriaType(Dictionary<MateriaTypeDef, float> weightedTypes, List<MateriaTypeDef> disallowedTypes = null)
        {
            float roll = Rand.Value;
            float cumulative = 0f;

            foreach (var kvp in weightedTypes)
            {
                cumulative += kvp.Value;
                if (roll <= cumulative)
                {
                    return kvp.Key;
                }
            }

            return weightedTypes.Keys.FirstOrDefault();
        }

        private static MateriaSlot CreateMateriaSlot(ThingWithComps parent, Comp_MateriaComp comp, MateriaTypeDef type, int index)
        {
            if (!IsValidForSlotCreation(parent, comp, type))
                return null;

            return new MateriaSlot(parent, comp, type, index + 1);
        }

        private static bool IsValidForSlotCreation(ThingWithComps parent, Comp_MateriaComp comp, MateriaTypeDef type)
        {
            if (type.slotLimits == null)
                return true;

            foreach (var limit in type.slotLimits)
            {
                if (parent.HasThingCategory(limit.ThingCategory))
                {
                    if (limit.maxSlots == 0)
                    {
                        return false;
                    }
                }


                //var currentCount = comp.GetMateriaCountOfType(type);
                //if (currentCount >= limit.maxSlots)
                //    return false;
            }

            return true;
        }

    }
}
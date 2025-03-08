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
        public static MateriaGenerationConfigDef GenConfig = MateriaDefOf.MateriaGenerationConfig;

        static MateriaSlotGenerator()
        {
           
        }

        public static void GenerateSlots(ThingWithComps parent, Comp_Materia comp)
        {
            if (parent == null || comp == null)
                return;

            //Log.Message($"Gen Materia Slots for {parent.Label}");
            float baseScore = ScoringUtil.CalculateBaseScore(parent);
            //Log.Message($"  BaseScore: {baseScore}");

            int maxSlotsAllowed = GetMaxSlotsAllowedFor(parent.def);
            int slotCount = CalculateSlotCount(baseScore, maxSlotsAllowed);
            //Log.Message($"  SlotCount: {slotCount}");

            if (slotCount <= 0)
                return;

            GenerateSlots(slotCount, baseScore, parent, comp);
        }

        public static int CalculateSlotCount(float score, int maxSlotsAllowed)
        {
            // No slots for very poor items
            if (score < 20)
                return 0;

            // Calculate slot count based on score
            float slotRatio = score / 100f;
            int baseSlotCount = Mathf.FloorToInt(slotRatio * maxSlotsAllowed);

            // Add randomness with weighted chance based on score
            float randomChance = Rand.Value;
            float progressToNextSlot = slotRatio * maxSlotsAllowed - baseSlotCount;

            if (randomChance < progressToNextSlot)
                baseSlotCount += 1;

            // Ensure at least one slot for decent items
            if (score >= 45 && baseSlotCount == 0)
                baseSlotCount = 1;

            return Mathf.Clamp(baseSlotCount, 0, maxSlotsAllowed);
        }

        public static int CalculateSlotLevel(float score, Thing thing)
        {
            // Base max level is 4
            int maxLevel = 4;

            // Calculate the base level from score
            float levelRatio = score / 100f;
            int baseLevel = Mathf.Max(1, Mathf.FloorToInt(levelRatio * maxLevel));

            // Add chance for higher level based on leftover score
            float randomChance = Rand.Value;
            float progressToNextLevel = levelRatio * maxLevel - baseLevel + 0.2f; // Small bonus for chance of next level

            if (randomChance < progressToNextLevel && baseLevel < maxLevel)
                baseLevel += 1;

            // Ensure legendary/archotech items always have max level slots
            if (thing.TryGetQuality(out QualityCategory qc) && qc == QualityCategory.Legendary)
                baseLevel = maxLevel;
            else if (thing.def.techLevel == TechLevel.Archotech)
                baseLevel = maxLevel;

            return Mathf.Clamp(baseLevel, 1, maxLevel);
        }

        public static int GetMaxSlotsAllowedFor(ThingDef def)
        {
            if (def.IsApparel && def.apparel != null)
            {
                if (def.apparel.bodyPartGroups != null &&
                    (def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) ||
                     def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs)))
                    return GenConfig.maxArmourSlots;
            }
            else if (def.IsApparel && def.apparel != null && def.apparel.layers.Contains(ApparelLayerDefOf.Belt))
                return GenConfig.maxUtilityArmourSlots;
            else if (def.IsWeapon)
                return GenConfig.maxWeaponSlots;

            return 0;
        }

        private static void GenerateSlots(int slotCount, float baseScore, ThingWithComps parent, Comp_Materia comp)
        {
            if (slotCount <= 0)
                return;

            var materiaTypes = DefDatabase<MateriaSlotTypeDef>.AllDefs
                .Where(x => x.IsValidForSlotCreation(parent, comp))
                .ToList();

            var weightedTypes = CalculateTypeWeights(materiaTypes, parent.def, baseScore);

            for (int i = 0; i < slotCount; i++)
            {
                int slotLevel = CalculateSlotLevel(baseScore, parent);
                var selectedType = MateriaSlotTypeDef.SelectMateriaType(weightedTypes);
                if (selectedType != null && selectedType.IsValidForSlotCreation(parent, comp))
                {
                    var slot = comp.AddMateriaSlot(selectedType, slotLevel);
                    if (slot != null)
                    {
                        // Log.Message($"Slot {slot.SlotType.LabelCap} level {slotLevel} added to {parent.Label}");
                    }
                }
            }
        }

        private static Dictionary<MateriaSlotTypeDef, float> CalculateTypeWeights(
            List<MateriaSlotTypeDef> types,
            ThingDef def,
            float score,
            List<MateriaSlotTypeDef> disallowedTypes = null)
        {
            var weights = new Dictionary<MateriaSlotTypeDef, float>();
            float totalWeight = 0f;
            // Check if the item is a weapon
            bool isWeapon = def.IsWeapon || def.IsRangedWeapon || def.IsMeleeWeapon;
            bool isArmor = def.IsApparel;
            foreach (var type in types)
            {
                // Skip disallowed types if any
                if (disallowedTypes != null && disallowedTypes.Contains(type))
                    continue;
                float weight = type.baseWeight;
                if (isWeapon)
                {
                    // Weapons favor offensive materia types
                    if (type.IsAcceptableMateriaType(MateriaDefOf.RedMateria)) weight *= 5.25f;
                }
                else if (isArmor)
                {
                    // Armor favors defensive materia types
                    if (type.IsAcceptableMateriaType(MateriaDefOf.GreenMateria)) weight *= 3.0f;
                    else if (type.IsAcceptableMateriaType(MateriaDefOf.BlueMateria)) weight *= 2.0f;
                }
                if (score > 75)
                {
                    if (type.baseWeight >= 0.7f) weight *= 1.5f;
                }
                else if (score < 40)
                {
                    if (type.baseWeight <= 0.3f) weight *= 1.5f;
                    else if (type.baseWeight >= 0.7f) weight *= 0.5f;
                }
                weights[type] = weight;
                totalWeight += weight;
            }
            if (totalWeight > 0)
            {
                foreach (var key in weights.Keys.ToList())
                {
                    weights[key] /= totalWeight;
                }
            }
            return weights;
        }
        //private static Dictionary<MateriaSlotTypeDef, float> CalculateTypeWeights(
        //List<MateriaSlotTypeDef> types,
        //ThingDef def,
        //float score,
        //List<MateriaSlotTypeDef> disallowedTypes = null)
        //{
        //    var weights = new Dictionary<MateriaSlotTypeDef, float>();
        //    float totalWeight = 0f;
        //    bool isWeapon = def.IsWeapon || def.IsRangedWeapon || def.IsMeleeWeapon;
        //    bool isArmor = def.IsApparel;

        //    var compatibleTypes = new List<MateriaSlotTypeDef>();
        //    foreach (var type in types)
        //    {
        //        if (disallowedTypes != null && disallowedTypes.Contains(type))
        //            continue;


        //        if (isWeapon && !type.IsAcceptableMateriaType(MateriaDefOf.RedMateria))
        //            continue;

        //        if (isArmor && !type.IsAcceptableMateriaType(MateriaDefOf.GreenMateria) &&
        //            !type.IsAcceptableMateriaType(MateriaDefOf.BlueMateria))
        //            continue;

        //        compatibleTypes.Add(type);
        //    }


        //    if (compatibleTypes.Count == 0)
        //        compatibleTypes = types.Where(t => disallowedTypes == null || !disallowedTypes.Contains(t)).ToList();


        //    foreach (var type in compatibleTypes)
        //    {
        //        float weight = type.baseWeight;

        //        if (isWeapon)
        //        {
        //            //foreach (var item in type.weaponMateriaWeights)
        //            //{
        //            //    weight *= item.weight;

        //            //}

        //            if (type.IsAcceptableMateriaType(MateriaDefOf.RedMateria))
        //                weight *= 2.5f;
        //            if (type.IsAcceptableMateriaType(MateriaDefOf.PurpleMateria))
        //                weight *= 1.5f;
        //        }
        //        else if (isArmor)
        //        {
        //            if (type.IsAcceptableMateriaType(MateriaDefOf.GreenMateria))
        //                weight *= 2.0f;
        //            else if (type.IsAcceptableMateriaType(MateriaDefOf.BlueMateria))
        //                weight *= 1.5f;
        //        }


        //        if (score > 75)
        //        {
        //            if (type.baseWeight >= 0.7f) weight *= 1.5f;
        //        }
        //        else if (score < 40)
        //        {
        //            if (type.baseWeight <= 0.3f) weight *= 1.5f;
        //            else if (type.baseWeight >= 0.7f) weight *= 0.5f;
        //        }

        //        weights[type] = weight;
        //        totalWeight += weight;
        //    }

        //    if (totalWeight > 0)
        //    {
        //        foreach (var key in weights.Keys.ToList())
        //        {
        //            weights[key] /= totalWeight;
        //        }
        //    }

        //    return weights;
        //}
    }


}
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
            // Constructor
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

            var materiaTypes = DefDatabase<MateriaTypeDef>.AllDefs
                .Where(x => x.IsValidForSlotCreation(parent, comp))
                .ToList();

            var weightedTypes = CalculateTypeWeights(materiaTypes, parent.def, baseScore);

            for (int i = 0; i < slotCount; i++)
            {
                int slotLevel = CalculateSlotLevel(baseScore, parent);

                bool isMetaSlot = DetermineIfMetaSlot(baseScore, parent);

                if (isMetaSlot)
                {
                    var slot = comp.AddMateriaSlot(null, slotLevel);
                    if (slot != null)
                    {
                        //Log.Message($"Meta slot level {slotLevel} added to {parent.Label}");
                    }
                }
                else
                {
                    var selectedType = MateriaTypeDef.SelectMateriaType(weightedTypes);
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
        }

        private static bool DetermineIfMetaSlot(float score, Thing thing)
        {
            float baseChance = 0.05f + (score / 100f) * 0.2f;

            if (thing.TryGetQuality(out QualityCategory qc))
            {
                if (qc == QualityCategory.Excellent) baseChance += 0.05f;
                else if (qc == QualityCategory.Masterwork) baseChance += 0.1f;
                else if (qc == QualityCategory.Legendary) baseChance += 0.15f;
            }

            if (thing.def.techLevel == TechLevel.Spacer) baseChance += 0.05f;
            else if (thing.def.techLevel == TechLevel.Ultra) baseChance += 0.1f;
            else if (thing.def.techLevel == TechLevel.Archotech) baseChance += 0.15f;

            return Rand.Value < baseChance;
        }

        private static Dictionary<MateriaTypeDef, float> CalculateTypeWeights(
            List<MateriaTypeDef> types,
            ThingDef def,
            float score,
            List<MateriaTypeDef> disallowedTypes = null)
        {
            var weights = new Dictionary<MateriaTypeDef, float>();
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
                    if (type == MateriaDefOf.RedMateria) weight *= 5.25f;
                }
                else if (isArmor)
                {
                    // Armor favors defensive materia types
                    if (type == MateriaDefOf.GreenMateria) weight *= 3.0f;
                    else if (type == MateriaDefOf.BlueMateria) weight *= 2.0f;
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
    }

    //public static class MateriaSlotGenerator
    //{
    //    public static MateriaGenerationConfigDef GenConfig = MateriaDefOf.MateriaGenerationConfig;

    //    static MateriaSlotGenerator()
    //    {

    //    }

    //    public static void GenerateSlots(ThingWithComps parent, Comp_MateriaComp comp)
    //    {
    //        if (parent == null)
    //            return;
    //        if (comp == null)
    //            return;

    //        Log.Message($"Gen Materia Slots for {parent.Label}");
    //        var baseScore = ScoringUtil.CalculateBaseScore(parent);
    //        Log.Message($"  BaseScore: {baseScore}");
    //        var maxSlots = GetMaxSlotsFor(parent);
    //        Log.Message($"  MaximumSlots: {maxSlots}");
    //        GenerateSlotDistribution(maxSlots, parent, comp);
    //    }

    //    public static int GetMaxSlotsForScore(ThingDef thingDef, float score, int maxSlotsAllowed, float scoreMax = 100f)
    //    {
    //        if (thingDef == null)
    //            return 0;
    //        //the absolute most slots under perfect circumstances
    //        int slotCap = GetMaxSlotsAllowedFor(thingDef);
    //        int actualMaxSlots = 0;

    //        if (score >= scoreMax * 0.9)
    //            actualMaxSlots = slotCap;
    //        if (score >= scoreMax * 0.8)
    //            actualMaxSlots = Random.Range(slotCap - 1, slotCap);
    //        else if (score >= scoreMax * 0.70)
    //            actualMaxSlots = Random.Range(2, slotCap - 1);
    //        else if (score >= scoreMax * 0.50)
    //            actualMaxSlots = Random.Range(0, 2);
    //        else
    //            actualMaxSlots = Random.Range(0, 1);


    //        int minimumSlots = score >= 55 ? 1 : 0;
    //        //clamp the value between min and actual max
    //        return Mathf.Clamp(actualMaxSlots, minimumSlots, maxSlotsAllowed);
    //    }

    //    public static int GetMaxSlotsFor(Thing thing)
    //    {
    //        if (thing == null)
    //            return 0;

    //        int slotCap = GetMaxSlotsAllowedFor(thing.def);
    //        if (thing.TryGetQuality(out QualityCategory qc))
    //        {
    //            switch (qc)
    //            {
    //                case QualityCategory.Awful:
    //                    return 0;
    //                case QualityCategory.Poor:
    //                    return Rand.Value >= 0.75f ? 1 : 0;
    //                case QualityCategory.Normal:
    //                    return Random.Range(0, 2);
    //                case QualityCategory.Good:
    //                    return Random.Range(0, 3);
    //                case QualityCategory.Excellent:
    //                    return Random.Range(0, slotCap);
    //                case QualityCategory.Masterwork:
    //                    return Random.Range(slotCap - 1, slotCap);
    //                case QualityCategory.Legendary:
    //                    return slotCap;
    //                default: 
    //                    return Random.Range(0, 3);
    //            }
    //        }
    //        else
    //        {
    //            float stuffScore = thing.GetStuffScore();

    //            switch (thing.def.techLevel)
    //            {
    //                case TechLevel.Undefined:
    //                    return 0;
    //                case TechLevel.Animal:
    //                    return 0;
    //                case TechLevel.Neolithic:
    //                    return Rand.Value >= 0.75f ? 1 : 0;
    //                case TechLevel.Medieval:
    //                    return Random.Range(0, 2);
    //                case TechLevel.Industrial:
    //                    return Random.Range(0, 3);
    //                case TechLevel.Spacer:
    //                    return Random.Range(0, slotCap);
    //                case TechLevel.Ultra:
    //                    return Random.Range(slotCap - 1, slotCap);
    //                case TechLevel.Archotech:
    //                    return slotCap;
    //                default:
    //                    return Random.Range(0, 3);
    //            }
    //        }
    //    }
    //    public static int GetMaxSlotLevelFor(Thing thing)
    //    {
    //        if (thing == null)
    //            return 1;

    //        int maxSlotLevel = 4;
    //        if (thing.TryGetQuality(out QualityCategory qc))
    //        {
    //            switch (qc)
    //            {
    //                case QualityCategory.Awful:
    //                    return 0;
    //                case QualityCategory.Poor:
    //                    return Rand.Value >= 0.75f ? 2 : 1;
    //                case QualityCategory.Normal:
    //                    return Random.Range(1, 2);
    //                case QualityCategory.Good:
    //                    return Random.Range(1, 3);
    //                case QualityCategory.Excellent:
    //                    return Random.Range(1, maxSlotLevel);
    //                case QualityCategory.Masterwork:
    //                    return Random.Range(maxSlotLevel - 1, maxSlotLevel);
    //                case QualityCategory.Legendary:
    //                    return maxSlotLevel;
    //                default:
    //                    return Random.Range(1, 3);
    //            }
    //        }
    //        else
    //        {
    //            float stuffScore = thing.GetStuffScore();
    //            switch (thing.def.techLevel)
    //            {
    //                case TechLevel.Undefined:
    //                    return 1;
    //                case TechLevel.Animal:
    //                    return 1;
    //                case TechLevel.Neolithic:
    //                    return Rand.Value >= 0.75f ? 2 : 1;
    //                case TechLevel.Medieval:
    //                    return Random.Range(1, 2);
    //                case TechLevel.Industrial:
    //                    return Random.Range(1, 3);
    //                case TechLevel.Spacer:
    //                    return Random.Range(1, maxSlotLevel);
    //                case TechLevel.Ultra:
    //                    return Random.Range(maxSlotLevel - 1, maxSlotLevel);
    //                case TechLevel.Archotech:
    //                    return maxSlotLevel;
    //                default:
    //                    return Random.Range(1, 3);
    //            }
    //        }
    //    }




    //    public static int GetMaxSlotsAllowedFor(ThingDef def)
    //    {
    //        if (def.IsApparel && def.apparel != null)
    //        {
    //            if (def.apparel.bodyPartGroups != null && def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) || def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs))
    //                return GenConfig.maxArmourSlots;
    //        }
    //        else if (def.IsApparel && def.apparel != null && def.apparel.layers.Contains(ApparelLayerDefOf.Belt))
    //            return GenConfig.maxUtilityArmourSlots;
    //        else if (def.IsWeapon)
    //            return GenConfig.maxWeaponSlots;

    //        return 0;
    //    }


    //    private static void GenerateSlotDistribution(int totalSlots, ThingWithComps parent, Comp_MateriaComp comp)
    //    {
    //        var slots = new List<MateriaSlot>();
    //        if (totalSlots <= 0)
    //            return;
    //        var materiaTypes = DefDatabase<MateriaTypeDef>.AllDefs.Where(x => x.IsValidForSlotCreation(parent, comp)).ToList();
    //        var weightedTypes = CalculateTypeWeights(materiaTypes, parent.def);

    //        for (int i = 0; i < totalSlots; i++)
    //        {
    //            var selectedType = MateriaTypeDef.SelectMateriaType(weightedTypes);
    //            if (selectedType != null)
    //            {
    //                int newSlotLevel = GetMaxSlotLevelFor(parent);
    //                //small chance to take any type
    //                if (Rand.Value >= 0.8173f)
    //                {
    //                    var slot = comp.AddMateriaSlot(null, newSlotLevel);
    //                    if (slot != null)
    //                    {
    //                        Log.Message($"Meta slot {slot.SlotLevel} added to {parent.Label}");
    //                    }
    //                }
    //                else
    //                {
    //                    if (selectedType.IsValidForSlotCreation(parent, comp))
    //                    {
    //                        var slot = comp.AddMateriaSlot(selectedType, newSlotLevel);
    //                        if (slot != null)
    //                        {
    //                            Log.Message($"Slot {slot.SlotType.LabelCap} {slot.SlotLevel} added to {parent.Label}");
    //                        }
    //                    }
    //                }



    //            }
    //        }
    //    }
    //    private static Dictionary<MateriaTypeDef, float> CalculateTypeWeights(List<MateriaTypeDef> types, ThingDef def, List<MateriaTypeDef> disallowedTypes = null)
    //    {
    //        var weights = new Dictionary<MateriaTypeDef, float>();
    //        float totalWeight = 0f;

    //        // Check if the item is a weapon
    //        bool isWeapon = def.IsWeapon || def.IsRangedWeapon || def.IsMeleeWeapon;

    //        foreach (var type in types)
    //        {
    //            float weight = type.baseWeight;


    //            if (isWeapon && type == MateriaDefOf.RedMateria)
    //            {
    //                weight *= 5.25f;
    //            }

    //            // Skip disallowed types if any
    //            if (disallowedTypes != null && disallowedTypes.Contains(type))
    //            {
    //                continue;
    //            }

    //            weights[type] = weight;
    //            totalWeight += weight;
    //        }

    //        // Normalize weights
    //        if (totalWeight > 0)
    //        {
    //            foreach (var key in weights.Keys.ToList())
    //            {
    //                weights[key] /= totalWeight;
    //            }
    //        }

    //        return weights;
    //    }
    //}
}
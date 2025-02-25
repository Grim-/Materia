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

        public static List<MateriaSlot> GenerateSlots(ThingWithComps parent, Comp_MateriaComp comp)
        {
            if (parent == null)
                return null;
            if (comp == null)
                return null;

            Log.Message($"Gen Materia Slots for {parent.Label}");
            var baseScore = CalculateBaseScore(parent, parent.def);
            Log.Message($"  BaseScore: {baseScore}");
            var maxSlots = GetMaxSlotsFor(parent);
            Log.Message($"  MaximumSlots: {maxSlots}");
            List<MateriaSlot> generatedSlots = GenerateSlotDistribution(maxSlots, parent, comp);
            return generatedSlots;
        }

        public static int GetMaxSlotsForScore(ThingDef thingDef, float score, int maxSlotsAllowed, float scoreMax = 100f)
        {
            if (thingDef == null)
                return 0;
            //the absolute most slots under perfect circumstances
            int slotCap = GetMaxSlotsAllowedFor(thingDef);
            int actualMaxSlots = 0;

            if (score >= scoreMax * 0.9)
                actualMaxSlots = slotCap;
            if (score >= scoreMax * 0.8)
                actualMaxSlots = Random.Range(slotCap - 1, slotCap);
            else if (score >= scoreMax * 0.70)
                actualMaxSlots = Random.Range(2, slotCap - 1);
            else if (score >= scoreMax * 0.50)
                actualMaxSlots = Random.Range(0, 2);
            else
                actualMaxSlots = Random.Range(0, 1);


            int minimumSlots = score >= 55 ? 1 : 0;
            //clamp the value between min and actual max
            return Mathf.Clamp(actualMaxSlots, minimumSlots, maxSlotsAllowed);
        }

        public static int GetMaxSlotsFor(Thing thing)
        {
            if (thing == null)
                return 0;

            int slotCap = GetMaxSlotsAllowedFor(thing.def);
            if (thing.TryGetQuality(out QualityCategory qc))
            {
                switch (qc)
                {
                    case QualityCategory.Awful:
                        return 0;
                    case QualityCategory.Poor:
                        return Rand.Value >= 0.75f ? 1 : 0;
                    case QualityCategory.Normal:
                        return Random.Range(0, 2);
                    case QualityCategory.Good:
                        return Random.Range(0, 3);
                    case QualityCategory.Excellent:
                        return Random.Range(0, slotCap);
                    case QualityCategory.Masterwork:
                        return Random.Range(slotCap - 1, slotCap);
                    case QualityCategory.Legendary:
                        return slotCap;
                    default: 
                        return Random.Range(0, 3);
                }
            }
            else
            {

                switch (thing.def.techLevel)
                {
                    case TechLevel.Undefined:
                        return 0;
                    case TechLevel.Animal:
                        return 0;
                    case TechLevel.Neolithic:
                        return Rand.Value >= 0.75f ? 1 : 0;
                    case TechLevel.Medieval:
                        return Random.Range(0, 2);
                    case TechLevel.Industrial:
                        return Random.Range(0, 3);
                    case TechLevel.Spacer:
                        return Random.Range(0, slotCap);
                    case TechLevel.Ultra:
                        return Random.Range(slotCap - 1, slotCap);
                    case TechLevel.Archotech:
                        return slotCap;
                    default:
                        return Random.Range(0, 3);
                }
            }
        }
        public static int GetMaxSlotLevelFor(Thing thing)
        {
            if (thing == null)
                return 0;

            int maxSlotLevel = 4;
            if (thing.TryGetQuality(out QualityCategory qc))
            {
                switch (qc)
                {
                    case QualityCategory.Awful:
                        return 0;
                    case QualityCategory.Poor:
                        return Rand.Value >= 0.75f ? 1 : 0;
                    case QualityCategory.Normal:
                        return Random.Range(0, 2);
                    case QualityCategory.Good:
                        return Random.Range(0, 3);
                    case QualityCategory.Excellent:
                        return Random.Range(0, maxSlotLevel);
                    case QualityCategory.Masterwork:
                        return Random.Range(maxSlotLevel - 1, maxSlotLevel);
                    case QualityCategory.Legendary:
                        return maxSlotLevel;
                    default:
                        return Random.Range(0, 3);
                }
            }
            else
            {

                switch (thing.def.techLevel)
                {
                    case TechLevel.Undefined:
                        return 0;
                    case TechLevel.Animal:
                        return 0;
                    case TechLevel.Neolithic:
                        return Rand.Value >= 0.75f ? 1 : 0;
                    case TechLevel.Medieval:
                        return Random.Range(0, 2);
                    case TechLevel.Industrial:
                        return Random.Range(0, 3);
                    case TechLevel.Spacer:
                        return Random.Range(0, maxSlotLevel);
                    case TechLevel.Ultra:
                        return Random.Range(maxSlotLevel - 1, maxSlotLevel);
                    case TechLevel.Archotech:
                        return maxSlotLevel;
                    default:
                        return Random.Range(0, 3);
                }
            }
        }
        public static int GetMaxSlotsAllowedFor(ThingDef def)
        {
            if (def.IsApparel && def.apparel != null)
            {
                if (def.apparel.bodyPartGroups != null && def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) || def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs))
                    return GenConfig.maxArmourSlots;
            }
            else if (def.IsApparel && def.apparel != null && def.apparel.layers.Contains(ApparelLayerDefOf.Belt))
                return GenConfig.maxUtilityArmourSlots;
            else if (def.IsWeapon)
                return GenConfig.maxWeaponSlots;

            return 0;
        }

        public static float CalculateBaseScore(Thing thing, ThingDef def)
        {
            if (def == null) 
                return 1f;

            float score = thing.GetTechLevelScore() +
                         thing.GetMarketValueScore() +
                         thing.GetStuffScore() +
                         thing.GetQualityScore() +
                         thing.GetResearchScore();

            float finalScore = Mathf.Clamp(score, 0, 100f);
            return finalScore;
        }


        private static List<MateriaSlot> GenerateSlotDistribution(int totalSlots, ThingWithComps parent, Comp_MateriaComp comp)
        {
            var slots = new List<MateriaSlot>();
            if (totalSlots <= 0)
                return slots;

            var materiaTypes = DefDatabase<MateriaTypeDef>.AllDefs.Where(x => x.IsValidForSlotCreation(parent, comp)).ToList();
            var weightedTypes = CalculateTypeWeights(materiaTypes, parent.def);

            for (int i = 0; i < totalSlots; i++)
            {
                var selectedType = MateriaTypeDef.SelectMateriaType(weightedTypes);
                if (selectedType != null)
                {
                    int newSlotLevel = GetMaxSlotLevelFor(parent);


                    if (selectedType.IsValidForSlotCreation(parent, comp))
                    {
                        var slot = new MateriaSlot(parent, comp, selectedType, newSlotLevel);
                        if (slot != null)
                        {
                            slot.SetLevel(newSlotLevel);
                            slots.Add(slot);
                            Log.Message($"Slot {slot.SlotType.LabelCap} {slot.SlotLevel} added to {parent.Label}");
                        }
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
                float weight = type.baseWeight;
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
    }
}
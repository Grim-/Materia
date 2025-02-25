using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Materia
{
    public class MateriaGenerationConfigDef : Def
    {
        public int maxArmourSlots = 4;
        public int maxUtilityArmourSlots = 2;
        public int maxWeaponSlots = 3;
        public float marketValueMultiplier = 0.2f;
        public float qualityScoreMutliplier = 0.3f;
        public float researchRequirementMutliplier = 1.2f;

        public float stuffScoreFactor = 0.09f;
        public float qualityScoreFactor = 5f;
        public QualityCategory minimumQuality = QualityCategory.Normal;
        public float techLevelScoreFactor = 8f;
        public TechLevel minimumTechLevel = TechLevel.Industrial;

        public float researchScoreFactor = 3;


        public List<SlotScoreData> slotScoreData;

        public List<SlotTechLevel> techLevelSlotCaps;

        public static int MAX_SLOT_LEVEL = 4;

        public int GetMaxSlotsAllowedFor(ThingDef def)
        {
            if (def.IsApparel && def.apparel != null)
            {
                if (def.apparel.bodyPartGroups != null && def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) || def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs))
                    return maxArmourSlots;
            }
            else if (def.IsApparel && def.apparel != null && def.apparel.layers.Contains(ApparelLayerDefOf.Belt))
                return maxUtilityArmourSlots;
            else if (def.IsWeapon)
                return maxWeaponSlots;

            return 0;
        }

        public int GetSlotLevelTechCap(ThingDef def)
        {
            SlotTechLevel techLevel = techLevelSlotCaps.Find(x => x.techLevel == def.techLevel);

            return techLevel != null ? techLevel.maxSlotLevel : MAX_SLOT_LEVEL;
        }
    }



    public class SlotTechLevel
    {
        public TechLevel techLevel = TechLevel.Industrial;
        public int maxSlotLevel = 1;
    }

    public class SlotScoreData
    {
        public float score;
        public IntRange slotAmounts;
    }
}

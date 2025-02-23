using RimWorld;
using System.Collections.Generic;
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
    }
}

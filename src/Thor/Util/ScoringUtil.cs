using RimWorld;
using UnityEngine;
using Verse;

namespace Materia
{
    public static class ScoringUtil
    {
        public static int Score(this Thing thing, int min = 0, int max = 100)
        {
            int techScore = thing.GetTechLevelScore();
            int marketValueScore = thing.GetMarketValueScore();
            int stuffScore = (int)thing.GetStuffScore();
            int qualityScore = thing.GetQualityScore();
            int researchScore = (int)thing.GetResearchScore();

            int subTotal = techScore + marketValueScore + stuffScore + qualityScore + researchScore;
            return Mathf.Clamp(subTotal, min, max);
        }

        public static int GetTechLevelScore(this Thing thing, int perLevel = 4, TechLevel minTechLevel = TechLevel.Industrial)
        {
            return thing.def.techLevel >= minTechLevel ? (int)thing.def.techLevel * perLevel : 0;
        }

        public static int GetMarketValueScore(this Thing thing, float valuePer = 0.03f, int min = 0, int max = 30)
        {
            int preClampMarketValue = (int)(thing.MarketValue * valuePer);
            int marketValue = Mathf.Clamp(preClampMarketValue, min, max);
            return marketValue;
        }
        public static float GetStuffScore(this Thing thing, float valuePer = 40f, int min = 0, int max = 30)
        {
            if (!thing.def.MadeFromStuff || thing.Stuff == null || thing.Stuff.stuffProps == null)
                return 0;


            float commonality = thing.Stuff.stuffProps.commonality;
            return Mathf.Clamp((1 - commonality) * valuePer, min, max);
        }

        public static int GetQualityScore(this Thing thing, float valuePer = 8f, QualityCategory minQuality = QualityCategory.Normal, int min = 0, int max = 40)
        {
            if (!thing.TryGetQuality(out QualityCategory qc))
            {
                return 0;
            }

            int preClampQualityValue = (int)(qc >= minQuality ? (int)qc * valuePer : 0);
            int qualityValue = Mathf.Clamp(preClampQualityValue, min, max);
            return qualityValue;
        }

        public static float GetResearchScore(this Thing thing, float valuePer = 6f, TechLevel minTechLevel = TechLevel.Industrial, int min = 0, int max = 20)
        {
            if (thing.def.recipeMaker?.researchPrerequisite != null && thing.def.recipeMaker.researchPrerequisite.techLevel >= minTechLevel)
            {
                return Mathf.Clamp((int)thing.def.recipeMaker.researchPrerequisite.techLevel * valuePer, min, max);
            }
            else
            {
                return 0;
            }
        }


        public static float GetArmorPenaltyScore(this Thing thing)
        {
            if (!thing.def.IsApparel || thing.def.apparel == null) return 0;
            return (thing.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) ||
                    thing.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs)) ? 10f : -20f;
        }

        public static string GetScoreString(this Thing thing)
        {
            QualityCategory quality = thing.TryGetQuality(out QualityCategory qc) ? qc : QualityCategory.Awful;
            var totalScore = Score(thing);
            return
                $"Scores:\n" +
                   $"Tech({thing.def.techLevel}): {GetTechLevelScore(thing)}\n" +
                   $"Value: {GetMarketValueScore(thing)}\n" +
                   $"Stuff({thing.Stuff?.defName}): {GetStuffScore(thing)}\n" +
                   $"Quality: {GetQualityScore(thing)}\n" +
                   $"Research: {GetResearchScore(thing)}\n" +
                   $"Score Total: {totalScore}\n";
        }
    }
}

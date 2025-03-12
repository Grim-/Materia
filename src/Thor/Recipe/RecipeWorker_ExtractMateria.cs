using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Materia
{
    public class RecipeWorker_ExtractMateria : RecipeWorker
    {
        MateriaRecipeDef Def => (MateriaRecipeDef)this.recipe;


        private static Dictionary<ThingDef, float> rockRarityCache = new Dictionary<ThingDef, float>();
        private float minChance = 0.3f;
        private float bonusPerSkill = 0.1f;
        private float bonusPerQuantity = 0.03f;

        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {
            ingredient.Destroy();
        }


        public virtual List<MateriaDef> GetAllowedMateria(int materiaLevel)
        {
            List<MateriaDef> allowed = new List<MateriaDef>();

            if (Def.allowedTypes == null)
            {
                allowed = DefDatabase<MateriaDef>.AllDefs.Where(x => x.slotLevel == materiaLevel).ToList();
            }
            else
            {
                allowed = DefDatabase<MateriaDef>.AllDefs.Where(x => x.SlotType != null && Def.allowedTypes.Contains(x.SlotType) && x.slotLevel == materiaLevel).ToList();
            }

            if (Def.disallowedTypes != null)
            {
                allowed.RemoveWhere(x => x.SlotType != null && Def.disallowedTypes.Contains(x.SlotType));
            }

            return allowed;
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            if (billDoer?.Map == null || ingredients == null || !ingredients.Any()) return;

            // Process all stone chunks
            float totalRarity = 0f;
            int totalStones = 0;

            // Calculate average rarity from all stones
            foreach (Thing ingredient in ingredients)
            {
                if (ingredient?.def == null) continue;

                ThingDef rock = ingredient.def;
                totalRarity += GetRockRarityValue(rock);
                totalStones++;
            }
            totalStones = ingredients.Count;
             

            // Calculate average rarity
            float avgRockRarity = totalRarity / totalStones;

            // Calculate skill bonus
            int miningSkill = billDoer?.skills?.GetSkill(SkillDefOf.Mining)?.Level ?? 0;
            int craftingSkill = billDoer?.skills?.GetSkill(SkillDefOf.Crafting)?.Level ?? 0;

            // Use both mining and crafting skills (average)
            float skillLevel = (miningSkill + craftingSkill) / 2.0f;
            float skillBonus = 1f + (skillLevel * bonusPerSkill);

            // More chunks = slightly better chance
            float quantityBonus = 1f + (Math.Min(totalStones, 10) * bonusPerQuantity);

            // Calculate final chance
            float baseChance = 0.05f * avgRockRarity * skillBonus * quantityBonus;
            baseChance = Math.Min(baseChance, minChance);

            List<MateriaDef> allMateria = GetAllowedMateria(Def.generatedMateriaLevel.RandomInRange);

            if (allMateria.Count == 0)
                return;

            Dictionary<MateriaDef, float> materiaWeights = new Dictionary<MateriaDef, float>();

            foreach (MateriaDef materiaDef in allMateria)
            {
                float weight = materiaDef.commonality;
                weight /= (float)Math.Pow(4.0, materiaDef.slotLevel - 1);

                //rarer higher slot levels
                if (materiaDef.slotLevel > 1)
                {
                    weight *= avgRockRarity * (materiaDef.slotLevel - 1);
                }

                //rare if less common or higher slot levels
                if (materiaDef.commonality < 0.5f || materiaDef.slotLevel > 1)
                {
                    weight *= skillBonus;
                }

                materiaWeights[materiaDef] = Math.Max(weight, 0.001f);
            }

            MateriaDef selectedMateria = null;
            float totalWeight = materiaWeights.Values.Sum();
            float roll = Rand.Value * totalWeight;
            float currentWeight = 0f;

            foreach (KeyValuePair<MateriaDef, float> pair in materiaWeights)
            {
                currentWeight += pair.Value;
                if (roll <= currentWeight)
                {
                    selectedMateria = pair.Key;
                    break;
                }
            }

            if (selectedMateria == null) return;

            Thing materia = ThingMaker.MakeThing(selectedMateria);
            materia.stackCount = 1;

            if (GenPlace.TryPlaceThing(materia, billDoer.Position, billDoer.Map, ThingPlaceMode.Near))
            {
                // Send message
                Messages.Message("Materia.ExtractedFromStone".Translate(billDoer.LabelShort, selectedMateria.label),
                    MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                materia.Destroy();
            }
        }

        private float GetRockRarityValue(ThingDef rockDef)
        {
            if (rockRarityCache.TryGetValue(rockDef, out float cachedValue))
            {
                return cachedValue;
            }

            float rarityValue = 1.0f;
            rarityValue *= (1.0f + (rockDef.BaseMarketValue / 10f));

            if (rockDef.building?.mineableThing != null)
            {
                rarityValue *= (1.0f + (rockDef.building.mineableThing.BaseMarketValue / 20f));
            }

            rockRarityCache[rockDef] = rarityValue;
            return rarityValue;
        }
    }
}

using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Thor
{
    public class RecipeWorker_BreakdownMateria : RecipeWorker
    {
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {
            ingredient.Destroy();
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            Thing materia = ingredients.FirstOrDefault();
            if (materia?.def is MateriaDef materiaDef)
            {
                foreach (MateriaBreakdownMaterials breakdown in materiaDef.breakdownMaterials)
                {
                    if (breakdown.thing != null)
                    {
                        int amount = breakdown.amount.RandomInRange;
                        if (amount > 0)
                        {
                            Thing product = ThingMaker.MakeThing(breakdown.thing);
                            product.stackCount = amount;

                            if (!GenPlace.TryPlaceThing(product, billDoer.Position, billDoer.Map, ThingPlaceMode.Near))
                            {
                                product.Destroy();
                            }
                        }
                    }
                }

                FleckMaker.ThrowLightningGlow(billDoer.Position.ToVector3(), billDoer.Map, 1f);      
            }
        }
    }
}

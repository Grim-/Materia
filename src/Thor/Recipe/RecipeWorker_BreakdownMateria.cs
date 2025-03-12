using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Materia
{
    public class RecipeWorker_BreakdownMateria : RecipeWorker
    {
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {
            ingredient.Destroy();
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            foreach (var item in ingredients)
            {
                Thing materia = item;
                if (materia?.def is MateriaDef materiaDef)
                {

                    //if (materiaDef.SlotType != null)
                    //{

                    //    MateriaTypeDef materiaTypeDef = materiaDef.SlotType;

                    //    int amountToMake = materiaDef.slotLevel * 2;

                    //    Thing product = ThingMaker.MakeThing(breakdown.thing);
                    //    product.stackCount = materiaDef.slotLevel * 2;

                    //    if (!GenPlace.TryPlaceThing(product, billDoer.Position, billDoer.Map, ThingPlaceMode.Near))
                    //    {
                    //        product.Destroy();
                    //    }




                    //}

                    foreach (MateriaBreakdownMaterials breakdown in materiaDef.breakdownMaterials)
                    {
                        if (breakdown.thing != null)
                        {
                            Thing product = ThingMaker.MakeThing(breakdown.thing);
                            product.stackCount = breakdown.amount.RandomInRange;

                            if (!GenPlace.TryPlaceThing(product, billDoer.Position, billDoer.Map, ThingPlaceMode.Near))
                            {
                                product.Destroy();
                            }
                        }
                    }

                    FleckMaker.ThrowLightningGlow(billDoer.Position.ToVector3(), billDoer.Map, 1f);
                }
            }

        }
    }
}

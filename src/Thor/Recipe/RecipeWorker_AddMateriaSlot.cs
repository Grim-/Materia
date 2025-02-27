using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Materia
{
    public class RecipeWorker_AddMateriaSlot : RecipeWorker
    {
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            if (ingredients.NullOrEmpty())
            {
                Log.Error("RecipeWorker_AddMateriaSlot: Ingredients list is null or empty");
                return;
            }
            Thing weapon = ingredients.FirstOrDefault(x => x.TryGetComp<Comp_Materia>() != null);
            if (weapon == null)
            {
                Log.Error("RecipeWorker_AddMateriaSlot: No weapon with MateriaComp found in ingredients");
                return;
            }
            Comp_Materia materiaComp = weapon.TryGetComp<Comp_Materia>();
            if (materiaComp == null)
            {
                Log.Error($"RecipeWorker_AddMateriaSlot: Failed to get MateriaComp from weapon {weapon.def.defName}");
                return;
            }
            Thing dust = ingredients.FirstOrDefault(x => x.def is MateriaDustDef);
            MateriaDustDef dustDef = dust?.def as MateriaDustDef;
            if (dustDef == null)
            {
                Log.Error("RecipeWorker_AddMateriaSlot: No MateriaDust found in ingredients or invalid dust definition");
                return;
            }
            if (dustDef.createsSlotType == null)
            {
                Log.Error($"RecipeWorker_AddMateriaSlot: MateriaDust {dustDef.defName} has no createsSlotType defined");
                return;
            }
            materiaComp.AddMateriaSlot(dustDef.createsSlotType);
            Messages.Message($"Added {dustDef.createsSlotType.label} materia slot to {weapon.Label}", MessageTypeDefOf.PositiveEvent);
        }

        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {
            if (ingredient.def is MateriaDustDef)
            {
                base.ConsumeIngredient(ingredient, recipe, map);
            }
        }
    }
}

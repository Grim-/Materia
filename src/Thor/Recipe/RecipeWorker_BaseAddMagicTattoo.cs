using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Thor
{

    public class MagicTattooRecipeDef : RecipeDef
    {
        public TattooDef tattooDef;

        public MagicTattooRecipeDef()
        {
            workerClass = typeof(RecipeWorker_BaseAddMagicTattoo);
        }
    }


    public class RecipeWorker_BaseAddMagicTattoo : Recipe_Surgery
    {
        MagicTattooRecipeDef Def => (MagicTattooRecipeDef)this.recipe;

        protected override void OnSurgerySuccess(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.OnSurgerySuccess(pawn, part, billDoer, ingredients, bill);

            TattooDef faceTattoo = Def.tattooDef;
            pawn.style.FaceTattoo = faceTattoo;
        }
    }
}

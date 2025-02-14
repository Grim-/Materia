using RimWorld;
using Verse;

namespace Thor
{
    public class MateriaEffectDef_GrantAbility : MateriaEffectDef
    {
        public AbilityDef ability;
    }

    public class MateriaEffect_GrantAbility : MateriaWorker
    {
        MateriaEffectDef_GrantAbility Def => (MateriaEffectDef_GrantAbility)def;

        private Ability abilityRef;

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);

            if (pawn.abilities.GetAbility(Def.ability) == null)
            {
                pawn.abilities.GainAbility(Def.ability);
                abilityRef = pawn.abilities.GetAbility(Def.ability);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);

            if (abilityRef != null && pawn.abilities.GetAbility(Def.ability) != null)
            {
                pawn.abilities.RemoveAbility(Def.ability);
                abilityRef = null;
            }
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref abilityRef, "abilityRef");
        }
    }

    public class MateriaEffectDef_GrantGene : MateriaEffectDef
    {
        public GeneDef gene;
        public bool isXenogene = true;
    }

    public class MateriaEffect_GrantGene : MateriaWorker
    {
        MateriaEffectDef_GrantGene Def => (MateriaEffectDef_GrantGene)def;

        private Gene geneRef;

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);

            if (!pawn.genes.HasActiveGene(Def.gene))
            {
                geneRef = pawn.genes.AddGene(Def.gene, Def.isXenogene);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);

            if (geneRef != null && pawn.genes.HasActiveGene(Def.gene))
            {
                pawn.genes.RemoveGene(geneRef);
                geneRef = null;
            }
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref geneRef, "geneRef");
        }
    }
}
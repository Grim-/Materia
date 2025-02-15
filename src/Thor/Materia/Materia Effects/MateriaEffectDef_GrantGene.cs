using UnityEngine;
using Verse;

namespace Thor
{
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
using UnityEngine;
using Verse;

namespace Materia
{
    public class MateriaEffectDef_GrantGene : MateriaEffectDef
    {
        public GeneDef gene;
        public bool isXenogene = true;

        public MateriaEffectDef_GrantGene()
        {
            workerClass = typeof(MateriaEffect_GrantGene);
        }
        public override string EffectDescription => $"Grants the {gene.LabelCap} gene while equipped";
    }

    public class MateriaEffect_GrantGene : MateriaWorker
    {
        MateriaEffectDef_GrantGene Def => (MateriaEffectDef_GrantGene)def;

        private Gene geneRef;

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);


            if (!ModsConfig.BiotechActive)
            {
                return;
            }


            if (!pawn.genes.HasActiveGene(Def.gene))
            {
                geneRef = pawn.genes.AddGene(Def.gene, Def.isXenogene);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);

            if (!ModsConfig.BiotechActive)
            {
                return;
            }

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
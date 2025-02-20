using RimWorld;
using System.Linq;
using Verse;

namespace Materia
{
    public class MateriaEffectDef_GrantTrait : MateriaEffectDef
    {
        public TraitDef trait;
        public bool overwriteExisting = true;
        public bool removeOnUnEquip = true;

        public MateriaEffectDef_GrantTrait()
        {
            workerClass = typeof(MateriaEffect_GrantTrait);
        }

        public override string EffectDescription => $"Grants the {trait.LabelCap} trait while equipped";


    }

    public class MateriaEffect_GrantTrait : MateriaWorker
    {
        MateriaEffectDef_GrantTrait Def => (MateriaEffectDef_GrantTrait)def;

        private bool hasGranted = false;

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            if (pawn.story != null && !pawn.story.traits.HasTrait(Def.trait))
            {
                int defaultDegree = 0;
                TraitDegreeData degreeData = Def.trait.degreeDatas.FirstOrDefault(d => d.degree == defaultDegree)
                    ?? Def.trait.degreeDatas.FirstOrDefault();

                if (degreeData != null)
                {
                    pawn.story.traits.GainTrait(new Trait(Def.trait, defaultDegree));
                    hasGranted = true;
                }
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);

            if (hasGranted && pawn.story.traits.HasTrait(Def.trait))
            {
                Trait trait = pawn.story.traits.GetTrait(Def.trait);
                if (trait != null)
                {
                    pawn.story.traits.RemoveTrait(trait);
                    hasGranted = false;
                }
            }
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref hasGranted, "hasGranted");
        }
    }
}
using RimWorld;
using System.Linq;
using Verse;

namespace Materia
{
    public class MateriaEffectDef_GrantHediff : MateriaEffectDef
    {
        public HediffDef hediff;
        public bool overwriteExisting = true;
        public bool removeOnUnEquip = true;

        public MateriaEffectDef_GrantHediff()
        {
            workerClass = typeof(MateriaEffect_GrantHediff);
        }

        public override string EffectDescription => $"Grants the {hediff.LabelCap} status while equipped";


    }

    public class MateriaEffect_GrantHediff : MateriaWorker
    {
        MateriaEffectDef_GrantHediff Def => (MateriaEffectDef_GrantHediff)def;

        private Hediff hediffRef;

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            if (!pawn.health.hediffSet.HasHediff(Def.hediff))
            {
                hediffRef = pawn.health.AddHediff(Def.hediff);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);

            if (hediffRef != null && pawn.health.hediffSet.HasHediff(hediffRef.def))
            {
                pawn.health.hediffSet.GetFirstHediffOfDef(Def.hediff);
                hediffRef = null;
            }
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref hediffRef, "hediffRef");
        }
    }
}
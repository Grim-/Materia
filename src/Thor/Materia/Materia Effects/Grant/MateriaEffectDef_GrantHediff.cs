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


            if (Def.hediff != null && !EquippingPawn.health.hediffSet.HasHediff(Def.hediff))
            {
                hediffRef = EquippingPawn.health.AddHediff(Def.hediff);

                if (hediffRef != null && hediffRef is HediffWithComps withComps)
                {
                    foreach (var item in withComps.comps)
                    {

                        if (item is IMateriaSlotParent slotParent)
                        {
                            slotParent.SetMateriaSlot(this.MateriaSlot);
                        }

                    }
                }
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {


            if (hediffRef != null && EquippingPawn.health.hediffSet.HasHediff(hediffRef.def))
            {
                EquippingPawn.health.hediffSet.GetFirstHediffOfDef(Def.hediff);
                hediffRef = null;
            }
            base.Notify_Unequipped(pawn);
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref hediffRef, "hediffRef");
        }
    }
}
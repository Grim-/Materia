using System;
using Verse;

namespace Materia
{
    public class SlotLinkRuleDef : Def
    {
        public Type workerClass;

        public SlotLinkRuleWorker CreateWorker(Comp_MateriaComp materiaComp, MateriaSlot materiaSlot)
        {
            SlotLinkRuleWorker slotLinkRuleWorker = (SlotLinkRuleWorker)Activator.CreateInstance(workerClass);
            slotLinkRuleWorker.MateriaComp = materiaComp;
            slotLinkRuleWorker.MateriaSlot = materiaSlot;
            slotLinkRuleWorker.Def = this;
            return slotLinkRuleWorker;
        }

    }
}

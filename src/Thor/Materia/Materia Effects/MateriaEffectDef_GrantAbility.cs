using RimWorld;
using Verse;

namespace Thor
{
    public class MateriaEffectDef_GrantAbility : MateriaEffectDef
    {
        public AbilityDef ability;

        public override string EffectDescription => $"Grants the {ability.LabelCap} ability while equipped";
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


}
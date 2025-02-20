using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Materia
{
    public class CompProperties_MagicalTome : CompProperties
    {
        public List<AbilityDef> abilityDefs;

        public CompProperties_MagicalTome()
        {
            this.compClass = typeof(Comp_MagicalTome);
        }
    }

    public class Comp_MagicalTome : ThingComp
    {
        private List<Ability> abilities = new List<Ability>();

        private CompProperties_MagicalTome Props
        {
            get
            {
                return this.props as CompProperties_MagicalTome;
            }
        }
        protected Pawn Holder
        {
            get
            {
                ThingWithComps parent = this.parent;
                Pawn_EquipmentTracker pawn_EquipmentTracker = ((parent != null) ? parent.ParentHolder : null) as Pawn_EquipmentTracker;
                if (pawn_EquipmentTracker == null)
                {
                    return null;
                }
                return pawn_EquipmentTracker.pawn;
            }
        }
        public List<Ability> AbilitiesForReading
        {
            get
            {
                if (this.abilities.NullOrEmpty())
                {
                    this.abilities = new List<Ability>();
                    foreach (AbilityDef abilityDef in Props.abilityDefs)
                    {
                        this.abilities.Add(AbilityUtility.MakeAbility(abilityDef, Holder));
                    }
                }
                return this.abilities;
            }
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            if (Holder != null)
            {
                foreach (Ability ability in AbilitiesForReading)
                {
                    ability.pawn = Holder;
                    ability.verb.caster = Holder;
                }
            }
        }

        public virtual void UsedOnce()
        {
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            foreach (Ability ability in AbilitiesForReading)
            {
                ability.pawn = pawn;
                ability.verb.caster = pawn;
            }
            pawn.abilities.Notify_TemporaryAbilitiesChanged();
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            pawn.abilities.Notify_TemporaryAbilitiesChanged();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref abilities, "abilities", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit && Holder != null)
            {
                foreach (Ability ability in AbilitiesForReading)
                {
                    ability.pawn = Holder;
                    ability.verb.caster = Holder;
                }
            }
        }
    }
}

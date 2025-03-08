using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Materia
{
    public class CompProperties_MateriaSetBonus : CompProperties
    {
        public List<MateriaSetBonus> setBonuses = new List<MateriaSetBonus>();

        public CompProperties_MateriaSetBonus()
        {
            compClass = typeof(Comp_MateriaSetBonus);
        }
    }

    public class Comp_MateriaSetBonus : ThingComp, IStatProvider
    {
        private List<MateriaSetBonus> _setBonuses = new List<MateriaSetBonus>();
        private Pawn _equippedPawn = null;

        public List<MateriaSetBonus> SetBonuses => _setBonuses;
        public CompProperties_MateriaSetBonus Props => (CompProperties_MateriaSetBonus)props;

        public Pawn EquippedPawn => _equippedPawn;
        private bool initialized = false;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (!respawningAfterLoad && !initialized)
            {
                InitializeSetBonuses();
                initialized = true;
            }
        }

        private void InitializeSetBonuses()
        {
            if (Props.setBonuses != null && Props.setBonuses.Count > 0)
            {
                _setBonuses = new List<MateriaSetBonus>(Props.setBonuses);

                foreach (var bonus in _setBonuses)
                {
                    bonus.InitializeEffects(parent, this);
                }
            }
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            _equippedPawn = pawn;
            CheckSetBonuses();

            MateriaManager.RegisterSetBonusComp(this);
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            DeactivateAllBonuses();
            _equippedPawn = null;

            MateriaManager.UnregisterSetBonusComp(this);
        }

        public void CheckSetBonuses()
        {
            if (_equippedPawn == null)
                return;

            foreach (var setBonus in _setBonuses)
            {
                bool shouldBeActive = setBonus.CheckRequirements(_equippedPawn);

                if (shouldBeActive != setBonus.isActive)
                {
                    if (shouldBeActive)
                        ActivateSetBonus(setBonus);
                    else
                        DeactivateSetBonus(setBonus);
                }
            }
        }

        private void ActivateSetBonus(MateriaSetBonus setBonus)
        {
            Log.Message("Activating set bonus");
            setBonus.isActive = true;

            if (_equippedPawn != null && _equippedPawn.Map != null)
            {
                MoteMaker.ThrowText(_equippedPawn.DrawPos, _equippedPawn.Map, $"{setBonus.label} activated!", 3.5f);
            }

            if (setBonus.ActiveEffects.NullOrEmpty())
            {
                setBonus.InitializeEffects(parent, this);
            }

            foreach (var effect in setBonus.ActiveEffects)
            {
                effect.Notify_Equipped(_equippedPawn);
                effect.Notify_MateriaEquipped();
            }
        }

        private void DeactivateSetBonus(MateriaSetBonus setBonus)
        {
            setBonus.isActive = false;

            if (_equippedPawn != null && _equippedPawn.Map != null)
            {
                MoteMaker.ThrowText(_equippedPawn.DrawPos, _equippedPawn.Map, $"{setBonus.label} deactivated", 3.5f);
            }

            foreach (var effect in setBonus.ActiveEffects)
            {
                effect.Notify_MateriaUnequipped();
                effect.Notify_Unequipped(_equippedPawn);
            }
        }

        private void DeactivateAllBonuses()
        {
            foreach (var setBonus in _setBonuses.Where(b => b.isActive))
            {
                DeactivateSetBonus(setBonus);
            }
        }


        public bool HasActiveBonuses()
        {
            return _setBonuses.Any(b => b.isActive);
        }

        public override string CompInspectStringExtra()
        {
            if (!HasActiveBonuses())
                return null;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Active materia set bonuses:");

            foreach (var bonus in _setBonuses.Where(b => b.isActive))
            {
                sb.AppendLine($"  - {bonus.label}");
            }

            return sb.ToString().TrimEnd();
        }

        public override void CompTick()
        {
            base.CompTick();
            // Tick active effects
            if (_equippedPawn != null)
            {
                foreach (var setBonus in _setBonuses.Where(b => b.isActive))
                {
                    foreach (var effect in setBonus.ActiveEffects)
                    {
                        effect.OnTick(_equippedPawn);
                    }
                }
            }
        }

        #region IStatProvider
        public IEnumerable<StatModifier> GetStatOffsets(StatDef stat)
        {
            foreach (var setBonus in _setBonuses.Where(b => b.isActive))
            {
                foreach (var effect in setBonus.ActiveEffects)
                {
                    if (effect.def is MateriaEffectDef_PawnStatOffset pawnStatDef && pawnStatDef.statToAffect == stat)
                    {
                        yield return new StatModifier
                        {
                            stat = pawnStatDef.statToAffect,
                            value = pawnStatDef.statOffset
                        };
                    }
                }
            }
        }

        public bool HasStatOffsetFor(StatDef stat)
        {
            foreach (var setBonus in _setBonuses.Where(b => b.isActive))
            {
                foreach (var effect in setBonus.ActiveEffects)
                {
                    if (effect.def is MateriaEffectDef_PawnStatOffset pawnStatDef && pawnStatDef.statToAffect == stat)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HasStatFactorFor(StatDef stat)
        {
            foreach (var setBonus in _setBonuses.Where(b => b.isActive))
            {
                foreach (var effect in setBonus.ActiveEffects)
                {
                    if (effect.def is MateriaEffectDef_PawnStatFactor pawnStatDef && pawnStatDef.statToAffect == stat)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public IEnumerable<StatModifier> GetStatFactors(StatDef stat)
        {
            foreach (var setBonus in _setBonuses.Where(b => b.isActive))
            {
                foreach (var effect in setBonus.ActiveEffects)
                {
                    if (effect.def is MateriaEffectDef_PawnStatFactor pawnStatDef && pawnStatDef.statToAffect == stat)
                    {
                        yield return new StatModifier
                        {
                            stat = pawnStatDef.statToAffect,
                            value = pawnStatDef.statFactor
                        };
                    }
                }
            }
        }

        public string GetExplanation(StatDef stat)
        {
            StringBuilder result = new StringBuilder();

            foreach (var setBonus in _setBonuses.Where(b => b.isActive))
            {
                foreach (var effect in setBonus.ActiveEffects)
                {
                    if (effect.def is MateriaEffectDef_PawnStat pawnStatDef && pawnStatDef.statToAffect == stat)
                    {
                        result.AppendLine($"   Set Bonus: {setBonus.label}");
                        result.AppendLine($"     {pawnStatDef.GetExplanationString()}");
                    }
                }
            }

            return result.ToString();
        }
        #endregion

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref initialized, "initialized", false);
            Scribe_References.Look(ref _equippedPawn, "equippedPawn");
            Scribe_Collections.Look(ref _setBonuses, "setBonuses", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (_setBonuses == null)
                {
                    InitializeSetBonuses();
                }

                if (_equippedPawn != null)
                {
                    foreach (var bonus in _setBonuses.Where(b => b.isActive))
                    {
                        foreach (var effect in bonus.ActiveEffects)
                        {
                            effect.Notify_Equipped(_equippedPawn);
                        }
                    }

                    MateriaManager.RegisterSetBonusComp(this);
                }
            }
        }
    }
}

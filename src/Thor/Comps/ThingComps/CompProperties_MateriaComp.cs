using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Thor
{
    public class CompProperties_MateriaComp : CompProperties
    {
        public List<MateriaSlotConfig> materiaSlots;

        public CompProperties_MateriaComp()
        {
            compClass = typeof(Comp_MateriaComp);
        }
    }

    public class Comp_MateriaComp : CompEquippable, IStatProvider
    {
        private List<MateriaSlot> materiaSlots = new List<MateriaSlot>();
        public List<MateriaSlot> MateriaSlots => materiaSlots.ToList();

        private List<MateriaSlotLink> _ActiveLinks = new List<MateriaSlotLink>();
        private List<MateriaSlotLink> ActiveLinks => _ActiveLinks;

        private bool initializedSlots = false;
        private CompProperties_MateriaComp Props => (CompProperties_MateriaComp)props;

        protected Pawn _EquippedPawn = null;
        public Pawn EquippedPawn => _EquippedPawn;
        public bool HasEquipOwner => EquippedPawn != null;


        public override void PostPostMake()
        {
            if (!initializedSlots)
            {
                if (Props.materiaSlots == null || Props.materiaSlots.Count == 0)
                {
                    GenerateSlotsForWeapon();
                }
                else
                {
                    CreateMateriaSlots();
                } 
                initializedSlots = true;
            }
            base.PostPostMake();
        }

        private void GenerateSlotsForWeapon()
        {
            QualityCategory? quality = null;
            if (parent.TryGetComp<CompQuality>() is CompQuality qualityComp)
            {
                quality = qualityComp.Quality;
            }

            materiaSlots = MateriaSlotGenerator.GenerateSlots(parent, this, quality);
        }

        protected void CreateMateriaSlots()
        {
            if (materiaSlots == null || materiaSlots.Count == 0)
            {
                materiaSlots = new List<MateriaSlot>();
                for (int i = 0; i < Props.materiaSlots.Count; i++)
                {
                    AddMateriaSlot(Props.materiaSlots[i]);
                }
            }
        }
        #region events
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_ParentDestroyed();
            });
            base.PostDestroy(mode, previousMap);
        }
        public override void PostDeSpawn(Map map)
        {
            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_ParentDestroyed();
            });
            base.PostDeSpawn(map);
        }
        public override void PostDraw()
        {
            base.PostDraw();

            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.PostDraw();
            });
        }


        public override void CompTick()
        {
          //dont call base method, it only calls CompEquippable which tries to tick verbs that arent used or might not even exist (see expose data)
        }
        public virtual void EquipTick()
        {
            if (_EquippedPawn != null)
            {
                foreach (var item in MateriaSlots)
                {
                    if (item.SlottedMateria != null)
                    {
                        item.SlottedMateria.TickMateria(_EquippedPawn);
                    }
                }
            }
        }
        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            //Log.Message("Materia Comp Notify_Equipped");

            _EquippedPawn = pawn;

            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_Equipped(pawn);
            });
        }
        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            //Log.Message("Materia Comp Notify_Unequipped");
            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_Unequipped(pawn);
            });

            _EquippedPawn = null;
        }
        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            base.Notify_Killed(prevMap, dinfo);


            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_OwnerKilled(prevMap, dinfo);
            });
        }
        public override void Notify_KilledPawn(Pawn pawn)
        {
            base.Notify_KilledPawn(pawn);

            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_KilledPawn(pawn);
            });
        }
        public virtual void Notify_ProjectileImpact(Pawn Attacker, Thing Target, Projectile Projectile)
        {
            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_ProjectileImpact(Attacker, Target, Projectile);
            });
        }
        public virtual DamageInfo Notify_ProjectileApplyDamageToTarget(DamageInfo Damage, Pawn Attacker, Thing Target, Projectile Projectile)
        {
            DamageInfo damageInfo = new DamageInfo(Damage);

            DoForAllMateria((Slot) =>
            {
                damageInfo =  Slot.SlottedMateria.Notify_ProjectileApplyDamageToTarget(damageInfo, Attacker, Target, Projectile);
            });

            return damageInfo;
        }
        public virtual DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult DamageWorkerResult)
        {
            DamageWorker.DamageResult damageResult = new DamageWorker.DamageResult();

            foreach (var item in MateriaSlots)
            {
                if (item.SlottedMateria != null)
                {
                    damageResult = item.SlottedMateria.Notify_ApplyMeleeDamageToTarget(target, Attacker, DamageWorkerResult);
                }
            }

            return damageResult;
        }

        public virtual void Notify_OwnerThoughtGained(Thought Thought, Pawn otherPawn)
        {
            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_OwnerThoughtGained(Thought, otherPawn);
            });

        }

        public virtual void Notify_OwnerThoughtLost(Thought Thought)
        {
            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_OwnerThoughtLost(Thought);
            });
        }

        public virtual void Notify_OwnerHediffGained(Hediff Hediff, BodyPartRecord partRecord, DamageInfo? dinfo, DamageWorker.DamageResult damageResult)
        {
            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_OwnerHediffGained(Hediff, partRecord, dinfo, damageResult);
            });
        }
        public virtual void Notify_OwnerHediffRemoved(Hediff Hediff)
        {
            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_OwnerHediffRemoved(Hediff);
            });
        }
        #endregion

        #region materia


        public bool HasFreeValidMateriaSlotFor(MateriaDef materia)
        {
            return materiaSlots.Any(slot => !slot.IsOccupied && slot.CanAccept(materia));
        }
        protected virtual void DoForAllMateria(Action<MateriaSlot> actionToDo)
        {
            foreach (var item in MateriaSlots)
            {
                if (item.SlottedMateria != null)
                {
                    actionToDo?.Invoke(item);
                }
            }
        }

        public void EquipMateria(MateriaDef materiaDef, MateriaSlot materiaslot, bool force = false)
        {
            if (materiaslot == null || materiaslot.IsOccupied)
                return;

            materiaslot.EquipMateria(materiaDef);
            ValidateAndUpdateSlotLinks();
        }
        public void EquipMateria(MateriaDef materiaDef)
        {
            var slot = materiaSlots.FirstOrDefault(s => !s.IsOccupied && s.CanAccept(materiaDef));
            if (slot == null)
                return;
            EquipMateria(materiaDef, slot);
        }

        public void UnequipMateria(MateriaSlot materiaslot)
        {
            if (materiaslot == null || !materiaslot.IsOccupied)
                return;

            Thing materia = ThingMaker.MakeThing(materiaslot.SlottedMateria.def);
            if (EquippedPawn != null)
            {
                EquippedPawn.inventory.TryAddAndUnforbid(materia);
            }
            else
            {
                GenPlace.TryPlaceThing(materia, this.parent.Position, this.parent.Map, ThingPlaceMode.Near);
            }
            materiaslot.UnequipMateria();
            ValidateAndUpdateSlotLinks();
        }

        private void ValidateAndUpdateSlotLinks()
        {
            CheckForBrokenMateriaLinks();
            CheckForValidLinks();
        }

        private void CheckForValidLinks()
        {
            for (int i = 0; i < materiaSlots.Count; i++)
            {

                for (int x = 0; x < materiaSlots[i].LinkingRules.Count; x++)
                {
                    if (materiaSlots[i].LinkingRules[x].ValidateSlots(materiaSlots, ActiveLinks))
                    {
                        materiaSlots[i].LinkingRules[x].CreateLinks(materiaSlots, ActiveLinks);
                    }
                }
            }
        }

        private void CheckForBrokenMateriaLinks()
        {
            foreach (var link in ActiveLinks.ToList())
            {
                if (link.Worker != null && !link.Worker.ValidateSlots(materiaSlots, ActiveLinks))
                {
                    BreakMateriaLink(link.OriginSlot, link.DestinationSlot);
                }
            }
        }

        public void CreateMateriaLink(MateriaSlot origin, MateriaSlot destination)
        {

            Log.Message($"Linking {origin.SlottedMateria.def.LabelCap} to {destination.SlottedMateria.def.LabelCap}");
            ActiveLinks.Add(new MateriaSlotLink { OriginSlot = origin, DestinationSlot = destination });
        }

        public void BreakMateriaLink(MateriaSlot origin, MateriaSlot destination)
        {
            Log.Message($"Breaking Linking {origin.SlottedMateria.def.LabelCap} from {destination.SlottedMateria.def.LabelCap}");
            ActiveLinks.RemoveAll(l => (l.OriginSlot == origin && l.DestinationSlot == destination) ||
                                      (l.OriginSlot == destination && l.DestinationSlot == origin));
        }

        public void AddMateriaSlot(MateriaTypeDef SlotType, int slotLevel = 1)
        {
            MateriaSlot materiaSlot = new MateriaSlot(this.parent, this, SlotType, this.materiaSlots.Count + 1, slotLevel);
            materiaSlots.Add(materiaSlot);
        }

        public void AddMateriaSlot(MateriaSlotConfig SlotConfig)
        {
            MateriaSlot materiaSlot = new MateriaSlot(this.parent, this, SlotConfig.slotType, this.materiaSlots.Count + 1, SlotConfig.slotLevel);
  
            materiaSlots.Add(materiaSlot);

            if (SlotConfig.materiaToAutoSlot != null)
            {
                EquipMateria(SlotConfig.materiaToAutoSlot, materiaSlot, true);
            }

            materiaSlot.SetLockStatus(SlotConfig.slotIsLocked);
        }

        public bool CanEquipMateria(MateriaDef materia)
        {
            return HasFreeValidMateriaSlotFor(materia);
        }

        #endregion

        #region Equipment Stat Modifiers
        private IEnumerable<MateriaEffectDef_EquipmentStat> FindStatEffects(StatDef stat)
        {
            foreach (var slot in materiaSlots)
            {
                if (slot.IsOccupied && slot.SlottedMateria != null)
                {
                    foreach (var effect in slot.SlottedMateria.GetEffectsOfType<MateriaEffect_EquipmentStat>())
                    {
                        var def = (MateriaEffectDef_EquipmentStat)effect.def;
                        if (def.statToAffect == stat)
                        {
                            yield return def;
                        }
                    }
                }
            }
        }

        public override float GetStatFactor(StatDef stat)
        {
            float factor = base.GetStatFactor(stat);
            foreach (var slot in materiaSlots)
            {
                if (slot.IsOccupied && slot.SlottedMateria != null)
                {
                    foreach (var effect in slot.SlottedMateria.GetEffectsOfType<MateriaEffect_EquipmentStat>())
                    {
                        var def = (MateriaEffectDef_EquipmentStat)effect.def;
                        if (def.statToAffect == stat)
                        {
                            factor += def.StatFactors.GetStatFactorFromList(stat);
                        }

                    }
                }
            }
            return factor;
        }

        public override float GetStatOffset(StatDef stat)
        {
            float offset = base.GetStatOffset(stat);
            foreach (var slot in materiaSlots)
            {
                if (slot.IsOccupied && slot.SlottedMateria != null)
                {
                    foreach (var effect in slot.SlottedMateria.GetEffectsOfType<MateriaEffect_EquipmentStat>())
                    {
                        var def = (MateriaEffectDef_EquipmentStat)effect.def;

                        if (effect.def is MateriaEffectDef_EquipmentStat equimentStatDef && def.statToAffect == stat)
                        {
                            offset += equimentStatDef.StatOffsets.GetStatOffsetFromList(stat);
                        }

                    }
                }
            }
            return offset;
        }

        public override void GetStatsExplanation(StatDef stat, StringBuilder sb)
        {
            base.GetStatsExplanation(stat, sb);

            foreach (var slot in materiaSlots)
            {
                if (slot.IsOccupied && slot.SlottedMateria != null)
                {
                    foreach (var effect in slot.SlottedMateria.GetEffectsOfType<MateriaEffect_EquipmentStat>())
                    {
                        var def = (MateriaEffectDef_EquipmentStat)effect.def;
                        if (def.statToAffect == stat)
                        {
                            var offset = def.StatOffsets.GetStatOffsetFromList(stat);
                            sb.AppendLine($"Materia {effect}: ×{offset:F2}");
                        }
                    }
                }
            }
        }
        #endregion

        #region Equip Pawn Stat Modifiers

        public IEnumerable<StatModifier> GetStatOffsets(StatDef stat)
        {
            foreach (var slot in materiaSlots)
            {
                if (slot.IsOccupied && slot.SlottedMateria != null)
                {
                    foreach (var effect in slot.SlottedMateria.GetEffectsOfType<MateriaEffect_PawnStatOffset>())
                    {
                        var def = (MateriaEffectDef_PawnStatOffset)effect.def;
                        if (def.statToAffect == stat)
                        {
                            yield return new StatModifier
                            {
                                stat = def.statToAffect,
                                value = def.statOffset
                            };
                        }
                    }
                }
            }
        }
        public IEnumerable<StatModifier> GetStatFactors(StatDef stat)
        {
            foreach (var slot in materiaSlots)
            {
                if (slot.IsOccupied && slot.SlottedMateria != null)
                {
                    foreach (var effect in slot.SlottedMateria.GetEffectsOfType<MateriaEffect_PawnStatFactor>())
                    {
                        var def = (MateriaEffectDef_PawnStatFactor)effect.def;
                        if (def.statToAffect == stat)
                        {
                            yield return new StatModifier
                            {
                                stat = def.statToAffect,
                                value = def.statFactor,
                            };
                        }
                    }
                }
            }
        }
        public string GetExplanation(StatDef stat)
        {
            StringBuilder result = new StringBuilder();

            foreach (var slot in materiaSlots)
            {
                if (slot.IsOccupied && slot.SlottedMateria != null)
                {
                    foreach (var effect in slot.SlottedMateria.ActiveEffects)
                    {
                        if (effect.def is MateriaEffectDef_PawnStat pawnStatDef)
                        {
                            if (pawnStatDef.statToAffect == stat)
                            {
                                result.AppendLine($"   {slot.SlottedMateria.def.GetColouredLabel()}: ");
                                result.AppendLine($"     {pawnStatDef.GetExplanationString()}");
                            }
                        }
                    }
                }
               // result.AppendLine($"\n");
            }

            return result.ToString();
        }

        #endregion

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            foreach (var item in base.CompGetGizmosExtra())
            {
                yield return item;
            }

            if (materiaSlots != null)
            {
                foreach (var slot in materiaSlots)
                {
                    if (slot.SlottedMateria != null)
                    {
                        foreach (var item in slot.SlottedMateria.CompGetGizmosExtra())
                        {
                            yield return item;
                        }
                    }
                }
            }

            yield return new Command_Action()
            {
                defaultLabel = "Manage Materia",
                action = () =>
                {
                    Find.WindowStack.Add(new Window_MateriaSelection(this.parent, _EquippedPawn, this));
                }
            };
        }

        private string GetEquippedMateriaInfo(bool includeDescription = false)
        {
            if (materiaSlots.NullOrEmpty() || !materiaSlots.Any(s => s.IsOccupied))
                return "No materia equipped";

            var equippedMateria = materiaSlots
                .Where(s => s.IsOccupied)
                .Select(s => includeDescription
                    ? $"  [{s.SlottedMateria.def.GetColouredLabel()}] \n     {s.EffectDescription}"
                    : $"  [{s.SlottedMateria.def.GetColouredLabel()}]")
                .ToList();

            StringBuilder output = new StringBuilder();
            output.AppendLine("Equipped materia:");
            output.AppendLine(string.Join(Environment.NewLine, equippedMateria));
            return output.ToString().TrimEnd();
        }

        public override string CompInspectStringExtra()
        {
            return GetEquippedMateriaInfo();
        }

        public override string GetDescriptionPart()
        {
            return GetEquippedMateriaInfo(includeDescription: true);
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref initializedSlots, "initializedSlots", false);
            Scribe_References.Look(ref _EquippedPawn, "equippedPawn");

            //stupid hack work around because it saves the same verbs twice with the key and load ids
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                this.verbTracker = null;
            }

            Scribe_Deep.Look<VerbTracker>(ref this.verbTracker, "eefefefefe", new object[]
            {
                this
            });

            Scribe_Collections.Look(ref materiaSlots, "materiaSlots", LookMode.Deep);

            //stupid hack work around because it saves the same verbs twice with the key and load ids
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                this.verbTracker = new VerbTracker(this);
            }

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (materiaSlots == null)
                {
                    CreateMateriaSlots();
                }

                // If the pawn was equipped when saved, reapply the materia effects
                if (_EquippedPawn != null)
                {
                    DoForAllMateria((Slot) =>
                    {
                        Slot.SlottedMateria.Notify_Equipped(_EquippedPawn);
                    });
                }
            }
        }
    }
}

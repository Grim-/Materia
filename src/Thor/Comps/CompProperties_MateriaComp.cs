using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Thor
{
    public class CompProperties_MateriaComp : CompProperties
    {
        public List<MateriaTypeDef> materiaSlots;

        public CompProperties_MateriaComp()
        {
            compClass = typeof(Comp_MateriaComp);
        }
    }

    public class Comp_MateriaComp : CompEquippable, IStatProvider
    {
        private List<MateriaSlot> materiaSlots = new List<MateriaSlot>();
        public List<MateriaSlot> MateriaSlots => materiaSlots.ToList();
        private bool initializedSlots = false;
        private CompProperties_MateriaComp Props => (CompProperties_MateriaComp)props;

        protected Pawn EquippedPawn = null;

        public override void PostPostMake()
        {
            if (!initializedSlots)
            {
                GenerateSlotsForWeapon();
                initializedSlots = true;
            }
            base.PostPostMake();
            CreateMateriaSlots();
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

        #region events

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            //Log.Message("Materia Comp Notify_Equipped");

            EquippedPawn = pawn;

            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_Equipped(pawn);
            });
        }

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

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            //Log.Message("Materia Comp Notify_Unequipped");
            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_Unequipped(pawn);
            });

            EquippedPawn = null;
        }

        public override void CompTick()
        {
           // base.CompTick();
        }

        public virtual void EquipTick()
        {
            if (EquippedPawn != null)
            {
                foreach (var item in MateriaSlots)
                {
                    if (item.SlottedMateria != null)
                    {
                        item.SlottedMateria.TickMateria(EquippedPawn);
                    }
                }
            }
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

        #endregion

        #region materia

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
        public void EquipMateria(MateriaDef materiaDef)
        {
            var slot = materiaSlots.FirstOrDefault(s => !s.IsOccupied && s.CanAccept(materiaDef));
            if (slot == null)
                return;

            slot.EquipMateria(materiaDef);
            CheckAndCreateMateriaLinks(slot);
        }
        public void UnequipMateria(MateriaDef materiaDef)
        {
            var slot = materiaSlots.FirstOrDefault(s => s.IsOccupied && s.SlottedMateria.def == materiaDef);
            if (slot == null)
                return;

            RemoveMateriaLinks(slot);
            slot.UnequipMateria();
        }
        private void CheckAndCreateMateriaLinks(MateriaSlot newSlot)
        {
            if (!newSlot.IsOccupied)
                return;

            foreach (var slot in materiaSlots)
            {
                if (slot != newSlot && slot.IsOccupied &&
                    newSlot.SlottedMateria.def.CanLinkWith(slot.SlottedMateria.def))
                {
                    CreateMateriaLink(newSlot, slot);
                }
            }
        }
        private void CreateMateriaLink(MateriaSlot source, MateriaSlot target)
        {
            source.SlottedMateria.Notify_MateriaLinked(source.SlottedMateria.def, target.SlottedMateria.def);
            target.SlottedMateria.Notify_MateriaLinked(target.SlottedMateria.def, source.SlottedMateria.def);
        }
        private void RemoveMateriaLinks(MateriaSlot slot)
        {
            if (!slot.IsOccupied)
                return;

            foreach (var otherSlot in materiaSlots)
            {
                if (otherSlot != slot && otherSlot.IsOccupied &&
                    slot.SlottedMateria.def.CanLinkWith(otherSlot.SlottedMateria.def))
                {
                    slot.SlottedMateria.Notify_MateriaUnlinked(slot.SlottedMateria.def, otherSlot.SlottedMateria.def);
                    otherSlot.SlottedMateria.Notify_MateriaUnlinked(otherSlot.SlottedMateria.def, slot.SlottedMateria.def);
                }
            }
        }

        public void AddMateriaSlot(MateriaTypeDef SlotType)
        {
            materiaSlots.Add(new MateriaSlot(this.parent, this, SlotType));
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

                        if (def.statToAffect == stat)
                        {
                            offset += def.StatOffsets.GetStatOffsetFromList(stat);
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
                    foreach (var effect in slot.SlottedMateria.GetEffectsOfType<MateriaEffect_PawnStat>())
                    {
                        var def = (MateriaEffectDef_PawnStat)effect.def;
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
                    foreach (var effect in slot.SlottedMateria.GetEffectsOfType<MateriaEffect_PawnStat>())
                    {
                        var def = (MateriaEffectDef_PawnStat)effect.def;
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
                        if (effect is MateriaEffect_PawnStat statEffect)
                        {
                            var def = (MateriaEffectDef_PawnStat)effect.def;
                            if (def.statToAffect == stat)
                            {
                                result.AppendLine($"  {slot.SlottedMateria.def.GetColouredLabel()}: ");
                                if (def.statOffset != 0)
                                    result.AppendLine($"    {def.statOffset:+#;-#;0}");
                                if (def.statFactor != 1)
                                    result.AppendLine($"    x{def.statFactor.ToStringPercent()}%");
                            }
                        }
                    }
                }
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
                    Find.WindowStack.Add(new Window_MateriaSelection(this.parent, EquippedPawn, this));
                }
            };


            if (Prefs.DevMode)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "Add Materia Slot",
                    action = () =>
                    {

                        List<FloatMenuOption> menuOptions = new List<FloatMenuOption>();

                        foreach (var item in DefDatabase<MateriaTypeDef>.AllDefsListForReading)
                        {
                            menuOptions.Add(new FloatMenuOption($"Add {item.defName} slot", () =>
                            {
                                AddMateriaSlot(item);
                            }));
                        }


                        Find.WindowStack.Add(new FloatMenu(menuOptions));
                    }
                };
            }

        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref initializedSlots, "initializedSlots", false);

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
            }
        }

        public override string CompInspectStringExtra()
        {
            if (materiaSlots.NullOrEmpty() || !materiaSlots.Any(s => s.IsOccupied))
                return "No materia equipped";

            var equippedMateria = materiaSlots
                .Where(s => s.IsOccupied)
                .Select(s => $"  [{s.SlottedMateria.def.GetColouredLabel()}]")
                .ToList();

            StringBuilder output = new StringBuilder();
            output.AppendLine("Equipped materia:");
            output.AppendLine(string.Join(Environment.NewLine, equippedMateria));

            return output.ToString().TrimEnd();
        }

        public override string GetDescriptionPart()
        {
            if (materiaSlots.NullOrEmpty() || !materiaSlots.Any(s => s.IsOccupied))
                return "No materia equipped";

            var equippedMateria = materiaSlots
                .Where(s => s.IsOccupied)
                .Select(s => $"  [{s.SlottedMateria.def.GetColouredLabel()}] \n     {s.SlottedMateria.def.description}")
                .ToList();

            StringBuilder output = new StringBuilder();
            output.AppendLine("Equipped materia:");
            output.AppendLine(string.Join(Environment.NewLine, equippedMateria));

            return output.ToString().TrimEnd();
        }
    }


    //public class RecipeWorker_AddMateriaSlot : RecipeWorker
    //{
    //    public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
    //    {
    //        base.Notify_IterationCompleted(billDoer, ingredients);

    //        Thing weapon = ingredients.Find(x => !(x.def is MateriaDustDef));
    //        if (weapon == null) 
    //            return;

    //        Comp_MateriaComp materiaComp = weapon.TryGetComp<Comp_MateriaComp>();
    //        if (materiaComp != null)
    //        {
    //            Thing dust = ingredients.Find(x => x.def is MateriaDustDef);
    //            if (dust?.def is MateriaDustDef dustDef)
    //            {
    //                materiaComp.AddMateriaSlot(dustDef.createsSlotType);
    //            }
    //        }
    //    }
    //}


    //public class SpecialThingFilterWorker_MateriaWeapons : SpecialThingFilterWorker
    //{
    //    public override bool Matches(Thing t)
    //    {
    //        return t.TryGetComp<Comp_MateriaComp>() != null;
    //    }

    //    public override bool AlwaysMatches(ThingDef def)
    //    {
    //        return def.HasComp(typeof(Comp_MateriaComp));
    //    }
    //}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

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

    public class Comp_MateriaComp : CompEquippable
    {
        private List<MateriaSlot> materiaSlots = new List<MateriaSlot>();
        public List<MateriaSlot> MateriaSlots => materiaSlots.ToList();

        private CompProperties_MateriaComp Props => (CompProperties_MateriaComp)props;

        protected Pawn EquippedPawn = null;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                CreateMateriaSlots();
                EquipMateria(DefDatabase<MateriaDef>.GetNamed("FireMateria"));
            }
        }

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


        protected void AddMateriaSlot(MateriaTypeDef SlotType)
        {
            materiaSlots.Add(new MateriaSlot(this.parent, this, SlotType));
        }

        public bool CanEquipMateria(MateriaDef materia)
        {
            return HasFreeValidMateriaSlotFor(materia);
        }

        public bool HasFreeValidMateriaSlotFor(MateriaDef materia)
        {
            return materiaSlots.Any(slot => !slot.IsOccupied && slot.CanAccept(materia));
        }

        public virtual DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, DamageWorker.DamageResult DamageWorkerResult)
        {
            DamageWorker.DamageResult damageResult = new DamageWorker.DamageResult();

            foreach (var item in MateriaSlots)
            {
                if (item.SlottedMateria != null)
                {
                    damageResult = item.SlottedMateria.Notify_ApplyMeleeDamageToTarget(target, DamageWorkerResult);
                }
            }

            return damageResult;
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

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            foreach (var item in base.CompGetGizmosExtra())
            {
                yield return item;
            }

            yield return new Command_Action()
            {
                defaultLabel = "Manage Materia",
                action = () =>
                {
                    Find.WindowStack.Add(new Window_MateriaSelection(this.parent, this));
                }
            };

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


        public override IEnumerable<Gizmo> CompGetEquippedGizmosExtra()
        {
            //return base.CompGetEquippedGizmosExtra();


            yield return new Command_Action()
            {
                defaultLabel = "Manage Materia",
                action = () =>
                {
                    Find.WindowStack.Add(new Window_MateriaSelection(this.parent, this));
                }
            };
        }


        public override void PostExposeData()
        {
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

            return "Equipped materia: " + materiaSlots
                .Where(s => s.IsOccupied)
                .Select(s => s.SlottedMateria.def.label)
                .ToCommaList();
        }
    }
    public class ThingDef_Materia : ThingDef
    {
        // Preserved materia-specific properties
        public float commonality = 1f;
        public MateriaTypeDef SlotType;
        public List<MateriaEffectDef> effects;
        public List<MateriaLinkBonus> links;
    }

    public class Thing_Materia : ThingWithComps
    {
        public ThingDef_Materia MateriaProps => def as ThingDef_Materia;
    }
}

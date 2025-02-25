using System;
using System.Collections.Generic;
using Verse;

namespace Materia
{
    public static class MateriaUtil
    {
        public static List<Thing> GenerateBreakDownMaterials(this MateriaDef MateriaDef, IntVec3 Position, Map map)
        {
            List<Thing> newThings = new List<Thing>();

            if (MateriaDef.breakdownMaterials == null)
            {
                return newThings;
            }
            foreach (var item in MateriaDef.breakdownMaterials)
            {
                for (int i = 0; i < item.amount.RandomInRange; i++)
                {
                    Thing thing = ThingMaker.MakeThing(item.thing);
                    GenSpawn.Spawn(thing, Position, map);
                    newThings.Add(thing);
                }
            }
            return newThings;
        }


        public static void QuickHeal(this Pawn pawn, float healAmount)
        {
            if (HealthUtility.TryGetWorstHealthCondition(pawn, out Hediff hediff, out BodyPartRecord part, null))
            {
                if (hediff == null || hediff.def == null)
                {
                    return;
                }
                 
                HealthUtility.AdjustSeverity(pawn, hediff.def, healAmount);
            }
        }

        public static void DropAndEquip(this Pawn_EquipmentTracker equipment, ThingWithComps newEquipment)
        {
            if (equipment?.pawn == null || newEquipment == null) return;

            // Check for existing weapon in the same equipment slot
            var existingEquipment = equipment.AllEquipmentListForReading
                .FirstOrDefault(x => x.def.equipmentType == newEquipment.def.equipmentType);

            if (existingEquipment != null)
            {
                // Drop the existing equipment near the pawn
                equipment.Remove(existingEquipment);
                GenPlace.TryPlaceThing(existingEquipment, equipment.pawn.Position, equipment.pawn.Map, ThingPlaceMode.Near);
            }

            // Equip the new weapon
            equipment.AddEquipment(newEquipment);
        }

        public static List<FloatMenuOption> GenerateMateriaOptions(Comp_MateriaComp materiaComp, Pawn usingPawn, MateriaSlot slot)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            if (Prefs.DevMode)
            {
                foreach (MateriaDef materiaDef in DefDatabase<MateriaDef>.AllDefs)
                {
                    var cacheMatdef = materiaDef;
                    if (slot.CanAccept(cacheMatdef))
                    {
                        options.Add(new FloatMenuOption(cacheMatdef.label, () =>
                        {
                            var cacheSlot = slot;
                            materiaComp.EquipMateria(cacheMatdef, cacheSlot);
                        }));
                    }
                }
            }
            else
            {
                foreach (Thing thing in usingPawn.EquippedWornOrInventoryThings)
                {
                    if (thing.def is MateriaDef materiaDef && slot.CanAccept(materiaDef))
                    {
                        var cacheMatdef = materiaDef;
                        options.Add(new FloatMenuOption(materiaDef.GetColouredLabel() + $" Level {materiaDef.slotLevel}", () =>
                        {
                            var cacheSlot = slot;
                            materiaComp.EquipMateria(cacheMatdef, cacheSlot);
                            usingPawn.inventory.RemoveCount(thing.def, 1);
                        }));
                    }
                }
            }

            if (options.Count == 0)
            {
                options.Add(new FloatMenuOption("No compatible materia available", null));
            }

            options.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.OrdinalIgnoreCase));
            return options;
        }
    }


}

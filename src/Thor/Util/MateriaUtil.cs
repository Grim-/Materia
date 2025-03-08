using LudeonTK;
using RimWorld;
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

        [DebugAction("Materia", "Test Spawn Meteor", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SpawnMeteor()
        {
            Find.Targeter.BeginTargeting(new TargetingParameters()
            {
                canTargetLocations = true
            },
            (LocalTargetInfo target) =>
            {
                if (target.Cell.IsValid && target.Cell.InBounds(Find.CurrentMap))
                {
                    Meteor meteor = (Meteor)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("Materia_Meteor"));
                    GenSpawn.Spawn(meteor, target.Cell, Find.CurrentMap);

                    meteor.Launch(target.Cell);
                }
            }
            );


        }
        public static bool HasThingCategoryOrParent(this Thing thing, ThingCategoryDef categoryToCheck)
        {
            if (thing.HasThingCategory(categoryToCheck))
                return true;

            foreach (ThingCategoryDef parentCategory in categoryToCheck.Parents)
            {
                if (thing.HasThingCategory(parentCategory))
                    return true;
            }

            foreach (ThingCategoryDef childCategory in categoryToCheck.ThisAndChildCategoryDefs)
            {
                if (thing.HasThingCategory(childCategory))
                    return true;
            }

            return false;
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
        public static List<Comp_Materia> GetAllEquippedOrWornMateriaComps(this Pawn pawn)
        {
            List<Comp_Materia> materiacomps = new List<Comp_Materia>();


            if (pawn.equipment != null)
            {
                foreach (Thing thing in pawn.equipment.AllEquipmentListForReading)
                {
                    if (thing is ThingWithComps withComps)
                    {
                        foreach (var item in withComps.AllComps)
                        {
                            if (item is Comp_Materia materia)
                            {
                                materiacomps.Add(materia);
                            }
                        }
                    }
                }
            }

            if (pawn.apparel != null)
            {
                foreach (Apparel thing in pawn.apparel.WornApparel)
                {
                    foreach (var item in thing.AllComps)
                    {
                        if (item is Comp_Materia materia)
                        {
                            materiacomps.Add(materia);
                        }
                    }
                }

            }

            return materiacomps;
        }
        public static List<FloatMenuOption> GenerateMateriaOptions(Comp_Materia materiaComp, Pawn usingPawn, MateriaSlot slot)
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

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Thor
{
    [StaticConstructorOnStartup]
    public static class MateriaPatchClass
    {
        static MateriaPatchClass()
        {
            var harmony = new Harmony("com.emo.materiapatches");
            harmony.PatchAll();

            harmony.Patch(
                original: AccessTools.Method(typeof(EquipmentUtility), "CanEquip",
                    new Type[] {
                    typeof(Thing),
                    typeof(Pawn),
                    typeof(string).MakeByRefType(),
                    typeof(bool)
                    }),
                prefix: new HarmonyMethod(typeof(Patch_EquipmentUtility_CanEquip), nameof(Patch_EquipmentUtility_CanEquip.Prefix))
            );

            foreach (var def in DefDatabase<ThingDef>.AllDefs.Where(d => d.IsWeapon || d.IsRangedWeapon))
            {
                if (def.comps?.OfType<CompProperties_MateriaComp>().Any() == true)
                    continue;

                if (def.comps == null)
                    def.comps = new List<CompProperties>();

                def.comps.Add(new CompProperties_MateriaComp());
            }
        }
    }

    [HarmonyPatch(typeof(BodyPartDef), "GetMaxHealth")]
    public class GetMaxHealth_Patch
    {
        [HarmonyPriority(0)]
        private static void Postfix(BodyPartDef __instance, Pawn pawn, ref float __result)
        {
            GeneExtension_PawnPartMaxHealth genePartExtension = TryGetFirstGeneExtension<GeneExtension_PawnPartMaxHealth>(pawn);
            if (genePartExtension != null)
            {
                __result *= genePartExtension.multiplier;
            }
        }
        private static List<T> TryGetAllGeneExtension<T>(Pawn pawn) where T : DefModExtension
        {
            var list = new List<T>();
            if (pawn.genes == null)
            {
                return list;
            }

            foreach (var item in pawn.genes.GenesListForReading)
            {
                if (item.def.HasModExtension<T>())
                {
                    list.Add(item.def.GetModExtension<T>());
                }
            }

            return list;
        }
        private static T TryGetFirstGeneExtension<T>(Pawn pawn) where T: DefModExtension
        {
            if (pawn.genes == null)
            {
                return null;
            }

            foreach (var item in pawn.genes.GenesListForReading)
            {
                if (item.def.HasModExtension<T>())
                {
                    return item.def.GetModExtension<T>();
                }
            }

            return null;
        }
    }
    public class Patch_EquipmentUtility_CanEquip
    {
        public static bool Prefix(Thing thing, Pawn pawn, ref string cantReason, bool checkBonded, ref bool __result)
        {
            CompSelectiveBiocodable compSelective = thing.TryGetComp<CompSelectiveBiocodable>();

            if (compSelective != null)
            {
                if (!compSelective.CanBeBiocodedFor(pawn))
                {
                    cantReason = "You cannot equip this, dont meet requirements";
                    __result = false;
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "EquipmentTrackerTick")]
    public class EquipmentTracker_Tick_Patch
    {
        static void Postfix(Pawn_EquipmentTracker __instance)
        {
            foreach (var eq in __instance.AllEquipmentListForReading)
            {
                eq.GetComp<Comp_MateriaComp>()?.EquipTick();
            }
        }
    }

    [HarmonyPatch(typeof(Verb_MeleeAttackDamage))]
    [HarmonyPatch("ApplyMeleeDamageToTarget")]
    public static class Patch_Verb_MeleeAttack_ApplyMeleeDamageToTarget
    {
        public static void Postfix(Verb_MeleeAttack __instance, LocalTargetInfo target, ref DamageWorker.DamageResult __result)
        {
            TryNotifyEquipment(__instance, target, ref __result);
        }

        public static void TryNotifyEquipment(Verb_MeleeAttack __instance, LocalTargetInfo target, ref DamageWorker.DamageResult __result)
        {
            if (__instance.EquipmentSource != null && target.Thing is Pawn targetPawn)
            {
                foreach (var item in __instance.EquipmentSource.GetComps<Comp_MateriaComp>())
                {
                    if (item != null)
                    {
                        __result = item.Notify_ApplyMeleeDamageToTarget(target, __instance.CasterPawn, __result);
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(StatWorker))]
    [HarmonyPatch("FinalizeValue")]
    public class Patch_StatWorker_FinalizeValue
    {
        public static void Prefix(StatWorker __instance, StatRequest req, StatDef ___stat, ref float val)
        {
            if (!req.HasThing) 
                return;

            // Get equipped items from pawn
            Pawn pawn = req.Thing as Pawn;
            if (pawn?.equipment?.AllEquipmentListForReading != null)
            {
                foreach (var equipment in pawn.equipment.AllEquipmentListForReading)
                {
                    foreach (var comp in equipment.GetComps<ThingComp>())
                    {
                        if (comp is IStatProvider provider)
                        {
                            // Apply offsets first
                            foreach (var offset in provider.GetStatOffsets(___stat))
                            {
                                val += offset.value;
                            }
                            // Then factors
                            foreach (var factor in provider.GetStatFactors(___stat))
                            {
                                val *= factor.value;
                            }
                        }
                    }
                }
            }
        }
    }

    //[HarmonyPatch(typeof(StatWorker))]
    //[HarmonyPatch("GetExplanationFinalizePart")]
    //public class Patch_StatWorker_GetExplanationFinalizePart
    //{
    //    public static void Postfix(StatRequest req, StatDef ___stat, ref string __result)
    //    {
    //        if (!req.HasThing) return;

    //        StringBuilder explanation = new StringBuilder(__result);
    //        bool addedHeader = false;

    //        Pawn pawn = req.Thing as Pawn;
    //        if (pawn?.equipment?.AllEquipmentListForReading != null)
    //        {
    //            foreach (var equipment in pawn.equipment.AllEquipmentListForReading)
    //            {
    //                foreach (var comp in equipment.GetComps<ThingComp>())
    //                {
    //                    if (comp is IStatProvider provider)
    //                    {
    //                        string statExplanation = provider.GetExplanation(___stat);
    //                        if (!statExplanation.NullOrEmpty())
    //                        {
    //                            if (!addedHeader)
    //                            {
    //                                explanation.AppendLine("\nMateria effects:");
    //                                addedHeader = true;
    //                            }
    //                            explanation.AppendLine(statExplanation);
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        __result = explanation.ToString();
    //    }
    //}

    [HarmonyPatch(typeof(StatWorker))]
    [HarmonyPatch("GetExplanationUnfinalized")]
    public class Patch_StatWorker_GetExplanationUnfinalized
    {
        public static void Postfix(StatRequest req, ref string __result, StatDef ___stat)
        {
            if (!req.HasThing) return;

            StringBuilder explanation = new StringBuilder(__result);
            bool addedHeader = false;

            Pawn pawn = req.Thing as Pawn;
            if (pawn?.equipment?.AllEquipmentListForReading != null)
            {
                foreach (var equipment in pawn.equipment.AllEquipmentListForReading)
                {
                    foreach (var comp in equipment.GetComps<ThingComp>())
                    {
                        if (comp is IStatProvider provider)
                        {
                            string statExplanation = provider.GetExplanation(___stat);
                            if (!statExplanation.NullOrEmpty())
                            {
                                if (!addedHeader)
                                {
                                    explanation.AppendLine($"\nMateria effects:");
                                    addedHeader = true;
                                }
                                explanation.AppendLine(statExplanation);
                            }
                        }
                    }
                }
            }

            __result = explanation.ToString();
        }
    }


    [HarmonyPatch(typeof(ITab_Pawn_Gear), "DrawThingRow")]
    public static class ITab_Pawn_Gear_DrawThingRow_Patch
    {
        public static void Prefix(ref float y, float width, Thing thing, ref Rect __state)
        {
            if (thing == null) return;
            var materiaComp = thing.TryGetComp<Comp_MateriaComp>();
            if (materiaComp == null) return;
            __state = new Rect(0f, y, width, 28f);
        }

        public static void Postfix(ITab_Pawn_Gear __instance, Rect __state, Thing thing)
        {
            if (__state == default) return;

            var materiaComp = thing.TryGetComp<Comp_MateriaComp>();
            if (materiaComp == null) return;

            Rect buttonRect = new Rect(__state.width - 76f, __state.y, 24f, 24f);

            if (Widgets.ButtonImage(buttonRect, TexButton.ToggleTweak))
            {
                var selPawnForGearProp = AccessTools.Property(typeof(ITab_Pawn_Gear), "SelPawnForGear");
                Pawn pawn = (Pawn)selPawnForGearProp.GetValue(__instance);
                Find.WindowStack.Add(new Window_MateriaSelection(thing, pawn, materiaComp));
            }

            if (Mouse.IsOver(buttonRect))
            {
                TooltipHandler.TipRegion(buttonRect, "Manage Materia");
            }
        }
    }
}

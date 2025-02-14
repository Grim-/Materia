using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
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
        }
    }

    public class GeneExtension_PawnPartMaxHealth : DefModExtension
    {
        public float multiplier = 2f;
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
                        __result = item.Notify_ApplyMeleeDamageToTarget(target, __result);
                    }
                }
            }
        }
    }


}

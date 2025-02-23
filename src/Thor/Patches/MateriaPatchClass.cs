using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;

namespace Materia
{
    [StaticConstructorOnStartup]
    public static class MateriaPatchClass
    {
        public static readonly Texture2D MateriaIcon = ContentFinder<Texture2D>.Get("UI/MateriaIcon");
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

            foreach (var def in DefDatabase<ThingDef>.AllDefs.Where(d => d.IsWeapon || d.IsRangedWeapon || d.IsApparel))
            {
                if (def.comps == null)
                    def.comps = new List<CompProperties>();

                if (def.comps?.OfType<CompProperties_MateriaComp>().Any() == true || def.HasModExtension<NoMateriaExt>())
                    continue;
                
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

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "GetGizmos")]
    public class EquipmentTracker_GetGizmos_Patch
    {
        static void Postfix(Pawn_EquipmentTracker __instance, ref IEnumerable<Gizmo> __result)
        {
            var originalGizmos = __result.ToList();

            var additionalGizmos = new List<Gizmo>();
            foreach (var eq in __instance.AllEquipmentListForReading)
            {
                if (eq is IDrawEquippedGizmos equippedGizmos)
                {
                    additionalGizmos.AddRange(equippedGizmos.GetEquippedGizmos());
                }

                foreach (var item in eq.AllComps)
                {
                    if (item is IDrawEquippedGizmos compEquippedGizmos)
                    {
                        additionalGizmos.AddRange(compEquippedGizmos.GetEquippedGizmos());
                    }
                }
    
            }

            __result = originalGizmos.Concat(additionalGizmos);
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

    [HarmonyPatch(typeof(Projectile))]
    [HarmonyPatch("Impact")]
    public class Patch_Projectile_Impact
    {
        public static void Postfix(Projectile __instance, Thing hitThing)
        {
            if (__instance != null && __instance.Launcher != null && __instance.Launcher is Pawn instigator)
            {
                var weapon = instigator.equipment?.Primary;
                if (weapon != null)
                {
                    foreach (var item in weapon.GetComps<Comp_MateriaComp>())
                    {
                        if (item != null)
                        {
                            item.Notify_ProjectileImpact(instigator, hitThing, __instance);
                        }
                    }
                }
            }
        }
    }


    [HarmonyPatch(typeof(Bullet))]
    [HarmonyPatch("Impact")]
    public class Patch_Bullet_Impact
    {
        private static DamageInfo ProcessDamageInfo(DamageInfo dinfo, Thing hitThing, Projectile __instance)
        {
            if (__instance.Launcher is Pawn attacker)
            {
                var weapon = attacker.equipment?.Primary;
                if (weapon != null)
                {
                    var comp = weapon.GetComp<Comp_MateriaComp>();
                    if (comp != null)
                    {
                        return comp.Notify_ProjectileApplyDamageToTarget(dinfo, attacker, hitThing, __instance);
                    }
                }
            }
            return dinfo;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var takeDamageMethod = AccessTools.Method(typeof(Thing), nameof(Thing.TakeDamage));
            var processMethod = AccessTools.Method(typeof(Patch_Bullet_Impact), nameof(ProcessDamageInfo));

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(takeDamageMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);  // hitThing
                    yield return new CodeInstruction(OpCodes.Ldarg_0);  // Projectile
                    yield return new CodeInstruction(OpCodes.Call, processMethod);
                }
                yield return instruction;
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
            if (materiaComp == null) 
                return;

            Rect buttonRect = new Rect(__state.width - 76f, __state.y, 24f, 24f);

            if (materiaComp.MateriaSlots != null && materiaComp.MateriaSlots.Count > 0)
            {
                if (Widgets.ButtonImage(buttonRect, MateriaPatchClass.MateriaIcon))
                {
                    var selPawnForGearProp = AccessTools.Property(typeof(ITab_Pawn_Gear), "SelPawnForGear");
                    Pawn pawn = (Pawn)selPawnForGearProp.GetValue(__instance);

                    Comp_MateriaComp.OpenMateriaWindow(thing, pawn, materiaComp);
                }

                if (Mouse.IsOver(buttonRect))
                {
                    TooltipHandler.TipRegion(buttonRect, "Manage Materia");
                }
            }
        }
    }


    [HarmonyPatch(typeof(MemoryThoughtHandler), new[] { typeof(Thought_Memory), typeof(Pawn) })]
    [HarmonyPatch("TryGainMemory")]
    public static class Patch_MemoryThoughtHandler_TryGainMemory
    {
        public static void Postfix(MemoryThoughtHandler __instance, Thought_Memory newThought, Pawn otherPawn)
        {
            Pawn pawn = __instance.pawn;
            if (pawn?.equipment?.AllEquipmentListForReading == null) 
                return;

            foreach (ThingWithComps equipment in pawn.equipment.AllEquipmentListForReading)
            {
                Comp_MateriaComp materiaComp = equipment.GetComp<Comp_MateriaComp>();
                if (materiaComp != null)
                {
                    materiaComp.Notify_OwnerThoughtGained(newThought, otherPawn);
                }
            }
        }
    }

    [HarmonyPatch(typeof(MemoryThoughtHandler), new[] { typeof(Thought_Memory)})]
    [HarmonyPatch("RemoveMemory")]
    public static class Patch_MemoryThoughtHandler_RemoveMemory
    {
        public static void Prefix(MemoryThoughtHandler __instance, Thought_Memory th)
        {
            Pawn pawn = __instance.pawn;
            if (pawn?.equipment?.AllEquipmentListForReading == null) return;

            foreach (ThingWithComps equipment in pawn.equipment.AllEquipmentListForReading)
            {
                Comp_MateriaComp materiaComp = equipment.GetComp<Comp_MateriaComp>();
                if (materiaComp != null)
                {
                    materiaComp.Notify_OwnerThoughtLost(th);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker))]
    [HarmonyPatch("AddHediff", new[] {
        typeof(Hediff),                    
        typeof(BodyPartRecord),           
        typeof(DamageInfo?),               
        typeof(DamageWorker.DamageResult)  
    })]
    public static class Patch_Pawn_HealthTracker_AddHediff
    {
        public static void Postfix(Hediff hediff, BodyPartRecord part, DamageInfo? dinfo, DamageWorker.DamageResult result, Pawn_HealthTracker __instance, Pawn ___pawn)
        {
            Pawn pawn = ___pawn;
            if (pawn == null || pawn?.equipment?.AllEquipmentListForReading == null) 
                return;


            foreach (ThingWithComps equipment in pawn.equipment.AllEquipmentListForReading)
            {
                Comp_MateriaComp materiaComp = equipment.GetComp<Comp_MateriaComp>();
                if (materiaComp != null)
                {
                    materiaComp.Notify_OwnerHediffGained(hediff, part, dinfo, result);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker))]
    [HarmonyPatch("RemoveHediff", new[] {
        typeof(Hediff)
    })]
    public static class Patch_Pawn_HealthTracker_RemoveHediff
    {
        public static void Postfix(
            Hediff hediff,
            Pawn ___pawn)
        {
            Pawn pawn = ___pawn;
            if (pawn == null || pawn?.equipment?.AllEquipmentListForReading == null)
                return;

            foreach (ThingWithComps equipment in pawn.equipment.AllEquipmentListForReading)
            {
                Comp_MateriaComp materiaComp = equipment.GetComp<Comp_MateriaComp>();
                if (materiaComp != null)
                {
                    materiaComp.Notify_OwnerHediffRemoved(hediff);
                }
            }
        }
    }

    //[StaticConstructorOnStartup]
    //public static class GraphicsPatches
    //{
    //    static GraphicsPatches()
    //    {
    //        var harmony = new Harmony("com.emo.materiagraphicspatches");
    //        harmony.PatchAll();
    //    }

    //    [HarmonyPatch(typeof(PawnRenderNode_Hair), "GraphicFor")]
    //    public static class HideHair_Patch
    //    {
    //        public static bool Prefix(Pawn pawn, ref Graphic __result)
    //        {
    //            if (pawn.health.hediffSet.HasHediff(ProteanDefOf.Protean_SuitActive))
    //            {
    //                __result = null;
    //                return false;
    //            }
    //            return true;
    //        }
    //    }
    //    [HarmonyPatch(typeof(PawnRenderNode_Beard), "GraphicFor")]
    //    public static class HideHair_Beard
    //    {
    //        public static bool Prefix(Pawn pawn, ref Graphic __result)
    //        {
    //            if (pawn.health.hediffSet.HasHediff(ProteanDefOf.Protean_SuitActive))
    //            {
    //                __result = null;
    //                return false;
    //            }
    //            return true;
    //        }
    //    }

    //    [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Head))]
    //    [HarmonyPatch("CanDrawNow")]
    //    public class Patch_PawnRenderNodeWorker_Apparel_Head_CanDrawNow
    //    {
    //        public static bool Prefix(PawnRenderNode n, PawnDrawParms parms, ref bool __result)
    //        {
    //            if (parms.pawn.health.hediffSet.HasHediff(ProteanDefOf.Protean_SuitActive))
    //            {
    //                __result = false;
    //                return false;
    //            }
    //            return true;
    //        }
    //    }
    //    [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Body))]
    //    [HarmonyPatch("CanDrawNow")]
    //    public class Patch_PawnRenderNodeWorker_Apparel_Body_CanDrawNow
    //    {
    //        public static bool Prefix(PawnRenderNode node, PawnDrawParms parms, ref bool __result)
    //        {
    //            if (parms.pawn.health.hediffSet.HasHediff(ProteanDefOf.Protean_SuitActive))
    //            {
    //                __result = false;
    //                return false;
    //            }
    //            return true;
    //        }
    //    }

    //    [HarmonyPatch(typeof(PawnRenderNode_Fur), "GraphicFor")]
    //    public static class HidePawnRenderNode_Fur
    //    {
    //        public static bool Prefix(Pawn pawn, ref Graphic __result)
    //        {
    //            if (pawn.health.hediffSet.HasHediff(ProteanDefOf.Protean_SuitActive))
    //            {
    //                __result = null;
    //                return false;
    //            }
    //            return true;
    //        }
    //    }

    //}
}

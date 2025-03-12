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

                if (def.comps?.OfType<CompProperties_Materia>().Any() == true || def.HasModExtension<NoMateriaExt>())
                    continue;
                
                def.comps.Add(new CompProperties_Materia());
            }
        }
    }

    [HarmonyPatch(typeof(Mineable))]
    [HarmonyPatch("TrySpawnYield")]
    [HarmonyPatch(new Type[] { typeof(Map), typeof(bool), typeof(Pawn) })]
    public static class Mineable_TrySpawnYield_Patch
    {
        private static Dictionary<ThingDef, float> rockRarityCache = new Dictionary<ThingDef, float>();

        [HarmonyPostfix]
        public static void Postfix(Mineable __instance, Map map, bool moteOnWaste, Pawn pawn)
        {
            if (map == null || __instance == null) return;

            ThingDef rock = __instance.def;
            IntVec3 position = __instance.Position;

            float rockRarity = GetRockRarityValue(rock);

            int miningSkill = pawn?.skills?.GetSkill(SkillDefOf.Mining)?.Level ?? 0;
            float skillBonus = 1f + (miningSkill * 0.05f);

            float baseChance = 0.05f * rockRarity * skillBonus;
            baseChance = Math.Min(baseChance, 0.3f);

            if (!Rand.Chance(baseChance)) return;

            List<MateriaDef> allMateria = DefDatabase<MateriaDef>.AllDefs
                .Where(m => m.SlotType != null)
                .ToList();

            if (allMateria.Count == 0) return;

            Dictionary<MateriaDef, float> materiaWeights = new Dictionary<MateriaDef, float>();

            foreach (MateriaDef materiaDef in allMateria)
            {
                float weight = materiaDef.commonality;

                weight /= (float)Math.Pow(4.0, materiaDef.slotLevel - 1);

                if (materiaDef.slotLevel > 1)
                {
                    weight *= rockRarity * (materiaDef.slotLevel - 1);
                }

                if (materiaDef.commonality < 0.5f || materiaDef.slotLevel > 1)
                {
                    weight *= skillBonus;
                }

                materiaWeights[materiaDef] = Math.Max(weight, 0.001f);
            }

            MateriaDef selectedMateria = null;
            float totalWeight = materiaWeights.Values.Sum();
            float roll = Rand.Value * totalWeight;
            float currentWeight = 0f;

            foreach (KeyValuePair<MateriaDef, float> pair in materiaWeights)
            {
                currentWeight += pair.Value;
                if (roll <= currentWeight)
                {
                    selectedMateria = pair.Key;
                    break;
                }
            }

            if (selectedMateria == null) return;

            Thing materia = ThingMaker.MakeThing(selectedMateria);
            materia.stackCount = 1;
            GenPlace.TryPlaceThing(materia, position, map, ThingPlaceMode.Near);
        }

        private static float GetRockRarityValue(ThingDef rockDef)
        {
            if (rockRarityCache.TryGetValue(rockDef, out float cachedValue))
            {
                return cachedValue;
            }

            float rarityValue = 1.0f;

            rarityValue *= (1.0f + (rockDef.BaseMarketValue / 10f));

            if (rockDef.building?.mineableThing != null)
            {
                rarityValue *= (1.0f + (rockDef.building.mineableThing.BaseMarketValue / 20f));
            }

            rockRarityCache[rockDef] = rarityValue;
            return rarityValue;
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






    #region Equipment Patches

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
            if (__instance.AllEquipmentListForReading != null)
            {

                foreach (var eq in __instance.AllEquipmentListForReading)
                {
                    if (eq.TryGetComp(out Comp_Materia _Materia))
                    {
                        _Materia.EquipTick();
                    }
                }
            }

        }
    }

    //Allows Weapons to call PostPreApplyDamage which is normally only called on apparel.
    [HarmonyPatch(typeof(Pawn_HealthTracker), "PreApplyDamage")]
    public class Pawn_HealthTracker_PreApplyDamage_Patch
    {
        static void Postfix(Pawn_HealthTracker __instance, Pawn ___pawn, ref DamageInfo dinfo, ref bool absorbed)
        {
            if (___pawn == null)
            {
                return;
            }

            if (___pawn.equipment != null && ___pawn.equipment.AllEquipmentListForReading != null)
            {
                foreach (var eq in ___pawn.equipment.AllEquipmentListForReading)
                {
                    if (eq.TryGetComp(out Comp_Materia _Materia))
                    {
                        _Materia.PostPreApplyDamage(ref dinfo, out absorbed);
                    }
                }
            }

        }
    }

  
    [HarmonyPatch(typeof(Pawn_ApparelTracker), "ApparelTrackerTick")]
    public class ApparelTracker_Tick_Patch
    {
        static void Postfix(Pawn_ApparelTracker __instance)
        {
            if (__instance.WornApparel != null)
            {
                foreach (var eq in __instance.WornApparel)
                {
                    if (eq.TryGetComp(out Comp_Materia _Materia))
                    {
                        _Materia.EquipTick();
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderUtility))]
    [HarmonyPatch("DrawEquipmentAndApparelExtras")]
    [HarmonyPatch(new Type[] { typeof(Pawn), typeof(Vector3), typeof(Rot4), typeof(PawnRenderFlags) })]
    public static class Patch_PawnRenderUtility_DrawEquipmentAndApparelExtras
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, Vector3 drawPos, Rot4 facing, PawnRenderFlags flags)
        {
            if (pawn != null)
            {
                List<Comp_Materia> materiaComps = pawn.GetAllEquippedOrWornMateriaComps();

                if (materiaComps != null)
                {
                    foreach (var item in materiaComps)
                    {

                        try
                        {
                            item.PostDraw();
                        }
                        catch (Exception e)
                        {
                            Log.ErrorOnce($"Exception thrown while trying to call MateriaComp PostDraw {e}", pawn.thingIDNumber * 1000);
                        }
                     
                    }
                }
            }
        }
    }


    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "GetGizmos")]
    public class EquipmentTracker_GetGizmos_Patch
    {
        static void Postfix(Pawn_EquipmentTracker __instance, ref IEnumerable<Gizmo> __result)
        {
            if (__instance.AllEquipmentListForReading != null)
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
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), "GetGizmos")]
    public class _ApparelTracker_GetGizmos_Patch
    {
        static void Postfix(Pawn_ApparelTracker __instance, ref IEnumerable<Gizmo> __result)
        {
            if (__instance.WornApparel != null)
            {
                var originalGizmos = __result.ToList();

                var additionalGizmos = new List<Gizmo>();
                foreach (var eq in __instance.WornApparel)
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
    }
    #endregion


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
                foreach (var item in __instance.EquipmentSource.GetComps<Comp_Materia>())
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
                    foreach (var item in weapon.GetComps<Comp_Materia>())
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


    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("Kill")]
    [HarmonyPatch(new Type[] { typeof(DamageInfo?), typeof(Hediff) })]
    public static class Patch_Pawn_Kill
    {
        public static void Postfix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit)
        {
            if (__instance is Pawn pawn && pawn.RaceProps.Humanlike)
            {
                var materiaComps = pawn.GetAllEquippedOrWornMateriaComps();

                if (materiaComps != null)
                {
                    materiaComps.ForEach(x => x.Notify_OwnerKilled());
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
                    var comp = weapon.GetComp<Comp_Materia>();
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

            if (pawn?.apparel?.WornApparel != null)
            {
                foreach (var equipment in pawn.apparel.WornApparel)
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
            if (pawn != null)
            {
                // Check equipment
                if (pawn.equipment?.AllEquipmentListForReading != null)
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

                // Check apparel too
                if (pawn.apparel?.WornApparel != null)
                {
                    foreach (var apparel in pawn.apparel.WornApparel)
                    {
                        foreach (var comp in apparel.GetComps<ThingComp>())
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
            var materiaComp = thing.TryGetComp<Comp_Materia>();
            if (materiaComp == null) return;
            __state = new Rect(0f, y, width, 28f);
        }

        public static void Postfix(ITab_Pawn_Gear __instance, Rect __state, Thing thing)
        {
            if (__state == default) return;

            var materiaComp = thing.TryGetComp<Comp_Materia>();
            if (materiaComp == null) 
                return;

            Rect buttonRect = new Rect(__state.width - 76f, __state.y, 24f, 24f);

            if (materiaComp.MateriaSlots != null && materiaComp.MateriaSlots.Count > 0)
            {
                if (Widgets.ButtonImage(buttonRect, MateriaPatchClass.MateriaIcon))
                {
                    var selPawnForGearProp = AccessTools.Property(typeof(ITab_Pawn_Gear), "SelPawnForGear");
                    Pawn pawn = (Pawn)selPawnForGearProp.GetValue(__instance);

                    Comp_Materia.OpenMateriaWindow(thing, pawn, materiaComp);
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
            if (pawn == null) 
                return;


            if (pawn?.equipment?.AllEquipmentListForReading != null)
            {
                foreach (ThingWithComps equipment in pawn.equipment.AllEquipmentListForReading)
                {
                    Comp_Materia materiaComp = equipment.GetComp<Comp_Materia>();
                    if (materiaComp != null)
                    {
                        materiaComp.Notify_OwnerThoughtGained(newThought, otherPawn);
                    }
                }
            }

            if (pawn?.apparel?.WornApparel != null)
            {
                foreach (ThingWithComps equipment in pawn.apparel.WornApparel)
                {
                    Comp_Materia materiaComp = equipment.GetComp<Comp_Materia>();
                    if (materiaComp != null)
                    {
                        materiaComp.Notify_OwnerThoughtGained(newThought, otherPawn);
                    }
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
            if (pawn == null) 
                return;


            if (pawn?.equipment?.AllEquipmentListForReading != null)
            {
                foreach (ThingWithComps equipment in pawn.equipment.AllEquipmentListForReading)
                {
                    Comp_Materia materiaComp = equipment.GetComp<Comp_Materia>();
                    if (materiaComp != null)
                    {
                        materiaComp.Notify_OwnerThoughtLost(th);
                    }
                }
            }

            if (pawn?.apparel?.WornApparel != null)
            {
                foreach (ThingWithComps equipment in pawn.apparel.WornApparel)
                {
                    Comp_Materia materiaComp = equipment.GetComp<Comp_Materia>();
                    if (materiaComp != null)
                    {
                        materiaComp.Notify_OwnerThoughtLost(th);
                    }
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
            if (pawn == null) 
                return;

            if (pawn?.equipment?.AllEquipmentListForReading != null)
            {
                foreach (ThingWithComps equipment in pawn.equipment.AllEquipmentListForReading)
                {
                    Comp_Materia materiaComp = equipment.GetComp<Comp_Materia>();
                    if (materiaComp != null)
                    {
                        materiaComp.Notify_OwnerHediffGained(hediff, part, dinfo, result);
                    }
                }
            }

            if (pawn?.apparel?.WornApparel != null)
            {
                foreach (ThingWithComps equipment in pawn.apparel.WornApparel)
                {
                    Comp_Materia materiaComp = equipment.GetComp<Comp_Materia>();
                    if (materiaComp != null)
                    {
                        materiaComp.Notify_OwnerHediffGained(hediff, part, dinfo, result);
                    }
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
            if (pawn == null)
                return;

            if (pawn?.equipment?.AllEquipmentListForReading != null)
            {
                foreach (ThingWithComps equipment in pawn.equipment.AllEquipmentListForReading)
                {
                    Comp_Materia materiaComp = equipment.GetComp<Comp_Materia>();
                    if (materiaComp != null)
                    {
                        materiaComp.Notify_OwnerHediffRemoved(hediff);
                    }
                }
            }

            if (pawn?.apparel?.WornApparel != null)
            {
                foreach (ThingWithComps equipment in pawn.apparel.WornApparel)
                {
                    Comp_Materia materiaComp = equipment.GetComp<Comp_Materia>();
                    if (materiaComp != null)
                    {
                        materiaComp.Notify_OwnerHediffRemoved(hediff);
                    }
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

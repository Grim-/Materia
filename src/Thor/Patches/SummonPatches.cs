using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Thor
{
    public static class SummonPatches
    {
        [HarmonyPatch(typeof(MentalStateHandler), "TryStartMentalState")]
        public static class Patch_MentalStateHandler_TryStartMentalState
        {
            public static bool Prefix(Pawn ___pawn, ref bool __result)
            {
                if (___pawn.Faction == Faction.OfPlayer && ___pawn.IsControlledSummon())
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Pawn_MindState), "StartFleeingBecauseOfPawnAction")]
        public static class Patch_Pawn_MindState_StartFleeingBecauseOfPawnAction
        {
            public static bool Prefix(Pawn ___pawn)
            {
                return !___pawn.IsControlledSummon();
            }
        }

        [HarmonyPatch(typeof(JobGiver_ConfigurableHostilityResponse), "TryGetFleeJob")]
        public static class Patch_JobGiver_ConfigurableHostilityResponse_TryGetFleeJob
        {
            public static bool Prefix(Pawn pawn, ref Job __result)
            {
                if (pawn.Faction == Faction.OfPlayer && pawn.IsControlledSummon())
                {
                    __result = null;
                    return false;
                }
                return true;
            }
        }
        //public static class Patch_Pawn_Thinker_ThinkTrees
        //{
        //    [HarmonyPatch(typeof(Pawn_Thinker), "MainThinkTree", MethodType.Getter)]
        //    public static class Patch_MainThinkTree
        //    {
        //        public static bool Prefix(Pawn_Thinker __instance, ref ThinkTreeDef __result)
        //        {
        //            return HandleThinkTreePatch(__instance, ref __result, isConstant: false);
        //        }
        //    }

        //    [HarmonyPatch(typeof(Pawn_Thinker), "ConstantThinkTree", MethodType.Getter)]
        //    public static class Patch_ConstantThinkTree
        //    {
        //        public static bool Prefix(Pawn_Thinker __instance, ref ThinkTreeDef __result)
        //        {
        //            return HandleThinkTreePatch(__instance, ref __result, isConstant: true);
        //        }
        //    }

        //    public static Dictionary<HediffDef, ThinkTreeDef> ThinkTreeMap = new Dictionary<HediffDef, ThinkTreeDef>()
        //    {
        //        { JJKDefOf.JJK_ZombieWorkSlaveHediff , JJKDefOf.ZombieWorkSlave},
        //        { JJKDefOf.JJK_Shikigami , JJKDefOf.JJK_SummonedCreature }
        //    };

        //    private static bool HandleThinkTreePatch(Pawn_Thinker __instance, ref ThinkTreeDef __result, bool isConstant)
        //    {
        //        Pawn pawn = __instance.pawn;

        //        if (pawn == null || pawn.Faction != Faction.OfPlayer)
        //            return true;


        //        if (!isConstant)
        //        {
        //            PawnExtension_ThinkTreeOverride thinkTreeOverride = pawn.kindDef.GetModExtension<PawnExtension_ThinkTreeOverride>();

        //            if (thinkTreeOverride != null && thinkTreeOverride.thinkTree != null)
        //            {
        //                if (pawn.TryGetComp(out Comp_TenShadowsSummon shadowsSummon) && shadowsSummon.Master == null)
        //                {
        //                    return true;
        //                }

        //                __result = thinkTreeOverride.thinkTree;
        //                return false;
        //            }
        //            else
        //            {
        //                foreach (var item in ThinkTreeMap)
        //                {
        //                    if (pawn == null || item.Key == null || item.Value == null)
        //                    {
        //                        continue;
        //                    }

        //                    if (pawn.health.hediffSet.GetFirstHediffOfDef(item.Key) != null)
        //                    {
        //                        __result = item.Value;
        //                        return false;
        //                    }
        //                }
        //            }

        //        }

        //        return true;
        //    }
        //}
        public static void TrainPawn(Pawn PawnToTrain, Pawn Trainer = null)
        {
            if (PawnToTrain.training != null)
            {
                foreach (var item in DefDatabase<TrainableDef>.AllDefsListForReading)
                {
                    if (PawnToTrain.training.CanAssignToTrain(item).Accepted)
                    {
                        PawnToTrain.training.SetWantedRecursive(item, true);
                        PawnToTrain.training.Train(item, Trainer, true);
                    }

                }
            }
        }

        public static bool IsControlledSummon(this Pawn pawn)
        {
            return pawn.health.hediffSet.HasHediff<Hediff_Undead>();
        }
    }
}

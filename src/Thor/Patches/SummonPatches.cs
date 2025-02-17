using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
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
        public static class Patch_Pawn_Thinker_ThinkTrees
        {
            [HarmonyPatch(typeof(Pawn_Thinker), "MainThinkTree", MethodType.Getter)]
            public static class Patch_MainThinkTree
            {
                public static bool Prefix(Pawn_Thinker __instance, ref ThinkTreeDef __result)
                {
                    return HandleThinkTreePatch(__instance, ref __result, isConstant: false);
                }
            }

            [HarmonyPatch(typeof(Pawn_Thinker), "ConstantThinkTree", MethodType.Getter)]
            public static class Patch_ConstantThinkTree
            {
                public static bool Prefix(Pawn_Thinker __instance, ref ThinkTreeDef __result)
                {
                    return HandleThinkTreePatch(__instance, ref __result, isConstant: true);
                }
            }

            public static Dictionary<HediffDef, ThinkTreeDef> ThinkTreeMap = new Dictionary<HediffDef, ThinkTreeDef>()
            {
                { ThorDefOf.DeathKnight_Undead , ThorDefOf.DeathKnight_SummonedCreature}
            };

            private static bool HandleThinkTreePatch(Pawn_Thinker __instance, ref ThinkTreeDef __result, bool isConstant)
            {
                Pawn pawn = __instance.pawn;

                if (pawn == null || pawn.Faction != Faction.OfPlayer)
                    return true;


                if (!isConstant)
                {
                    foreach (var item in ThinkTreeMap)
                    {
                        if (pawn == null || item.Key == null || item.Value == null)
                        {
                            continue;
                        }

                        if (pawn.health.hediffSet.GetFirstHediffOfDef(item.Key) != null)
                        {
                            __result = item.Value;
                            return false;
                        }
                    }

                }

                return true;
            }
        }


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




        public static bool TryMakeUndeadSummon(this Pawn pawn, Pawn Master)
        {
            Hediff_UndeadMaster master = (Hediff_UndeadMaster)Master.health.GetOrAddHediff(ThorDefOf.DeathKnight_UndeadMaster);
            Hediff_Undead undeadSummon = (Hediff_Undead)pawn.health.GetOrAddHediff(ThorDefOf.DeathKnight_Undead);
            if (master != null && undeadSummon != null)
            {
                if (undeadSummon.Master != null)
                {
                    //has a master
                    return false;
                }

                undeadSummon.SetMaster(Master);
                master.AbsorbCreature(undeadSummon.pawn);
                return true;
            }

            return false;
        }



        public static Pawn GetMaster(this Pawn pawn)
        {
            Hediff_Undead shikigami = (Hediff_Undead)pawn.health.hediffSet.GetFirstHediffOfDef(ThorDefOf.DeathKnight_Undead);
            if (shikigami != null)
            {
                return shikigami.Master;
            }

            return null;
        }

        public static bool IsMasterOf(this Pawn master, Pawn pawn)
        {
            Hediff_Undead undeadSummon = (Hediff_Undead)pawn.health.GetOrAddHediff(ThorDefOf.DeathKnight_Undead);
            if (master != null && undeadSummon != null)
            {
                if (undeadSummon.Master != null)
                {
                    //has a master
                    return undeadSummon.Master == master;
                }

                return true;
            }

            return false;
        }
        public static bool IsControlledSummon(this Pawn pawn)
        {
            return pawn.health.hediffSet.HasHediff<Hediff_Undead>();
        }
    }
}

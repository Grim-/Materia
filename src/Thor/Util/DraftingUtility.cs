using HarmonyLib;
using RimWorld;
using Verse;

namespace Thor
{
    public static class DraftingUtility
    {
        public static WorldComponent_DraftableCreatures DraftManager
        {
            get
            {
                if (Current.Game != null && Current.Game.World != null)
                {
                    return Current.Game.World.GetComponent<WorldComponent_DraftableCreatures>();
                }

                return null;
            }
        }


        public static void RegisterDraftableCreature(Pawn pawn)
        {
            if (DraftManager != null)
            {
                DraftManager.RegisterDraftableCreature(pawn);
            }
        }

        public static void UnregisterDraftableCreature(Pawn pawn)
        {
            if (DraftManager != null)
            {
                DraftManager.UnregisterDraftableCreature(pawn);
            }
        }

        public static bool IsDraftableCreature(Pawn pawn)
        {
            if (DraftManager != null)
            {
                return DraftManager.IsDraftableCreature(pawn);
            }

            return false;
        }

        public static void MakeDraftable(this Pawn pawn)
        {
            RegisterDraftableCreature(pawn);
        }
    }


    // Harmony patches
    [HarmonyPatch(typeof(FloatMenuMakerMap), "CanTakeOrder")]
    public static class FloatMenuMakerMap_CanTakeOrder_Patch
    {
        [HarmonyPostfix]
        public static void MakePawnControllable(Pawn pawn, ref bool __result)
        {
            if (DraftingUtility.IsDraftableCreature(pawn) && pawn.Faction?.IsPlayer == true)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "WorkTypeIsDisabled")]
    public static class Pawn_WorkTypeIsDisabled_Patch
    {
        [HarmonyPostfix]
        public static void DisableDoctorWork(WorkTypeDef w, Pawn __instance, ref bool __result)
        {
            if (w == WorkTypeDefOf.Doctor && DraftingUtility.IsDraftableCreature(__instance)
                && __instance.RaceProps.IsMechanoid == false)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(SchoolUtility), "CanTeachNow")]
    public static class SchoolUtility_CanTeachNow_Patch
    {
        [HarmonyPrefix]
        public static bool PreventTeaching(Pawn teacher, ref bool __result)
        {
            if (DraftingUtility.IsDraftableCreature(teacher))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn), "IsColonistPlayerControlled", MethodType.Getter)]
    public static class Pawn_IsColonistPlayerControlled_Patch
    {
        [HarmonyPostfix]
        public static void AddDraftableCreatureAsColonist(Pawn __instance, ref bool __result)
        {
            if (DraftingUtility.IsDraftableCreature(__instance))
            {
                __result = __instance.Spawned &&  __instance.Faction == Faction.OfPlayer && __instance.HostFaction == null;
            }
        }
    }
}


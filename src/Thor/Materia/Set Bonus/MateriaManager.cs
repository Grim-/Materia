using System.Collections.Generic;
using Verse;

namespace Materia
{
    /// <summary>
    /// Central manager for materia system interactions between components
    /// </summary>
    public static class MateriaManager
    {
        private static Dictionary<Pawn, List<Comp_MateriaSetBonus>> pawnSetBonusComps = new Dictionary<Pawn, List<Comp_MateriaSetBonus>>();

        /// <summary>
        /// Register a set bonus component to receive updates when materia changes
        /// </summary>
        public static void RegisterSetBonusComp(Comp_MateriaSetBonus comp)
        {
            if (comp?.EquippedPawn == null)
                return;

            Pawn pawn = comp.EquippedPawn;

            if (!pawnSetBonusComps.ContainsKey(pawn))
                pawnSetBonusComps[pawn] = new List<Comp_MateriaSetBonus>();

            if (!pawnSetBonusComps[pawn].Contains(comp))
                pawnSetBonusComps[pawn].Add(comp);
        }

        /// <summary>
        /// Unregister a set bonus component
        /// </summary>
        public static void UnregisterSetBonusComp(Comp_MateriaSetBonus comp)
        {
            if (comp?.EquippedPawn == null)
                return;

            Pawn pawn = comp.EquippedPawn;

            if (pawnSetBonusComps.ContainsKey(pawn))
                pawnSetBonusComps[pawn].Remove(comp);
        }

        /// <summary>
        /// Called by Comp_Materia whenever materia is equipped or unequipped
        /// This will update all set bonus components on the pawn
        /// </summary>
        public static void NotifyMateriaChanged(Pawn pawn)
        {
            if (pawn == null || !pawnSetBonusComps.ContainsKey(pawn))
                return;

            foreach (var comp in pawnSetBonusComps[pawn])
            {
                comp.CheckSetBonuses();
            }
        }
    }
}

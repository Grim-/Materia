using RimWorld.Planet;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Thor
{
    public class WorldComponent_DraftableCreatures : WorldComponent
    {
        private HashSet<Pawn> draftableCreatures = new HashSet<Pawn>();

        public WorldComponent_DraftableCreatures(World world) : base(world)
        {

        }

        public void RegisterDraftableCreature(Pawn pawn)
        {
            if (pawn != null)
            {
                if (!draftableCreatures.Contains(pawn))
                {
                    draftableCreatures.Add(pawn);
                }

                EnsureDraftComponents(pawn);
            }
        }

        public void UnregisterDraftableCreature(Pawn pawn)
        {
            if (pawn != null)
            {
                draftableCreatures.Remove(pawn);
            }
        }

        public bool IsDraftableCreature(Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }
            return draftableCreatures.Contains(pawn);
        }

        private void EnsureDraftComponents(Pawn pawn)
        {
            if (pawn.drafter == null)
            {
                pawn.drafter = new Pawn_DraftController(pawn);
            }
            if (pawn.equipment == null)
            {
                pawn.equipment = new Pawn_EquipmentTracker(pawn);
            }

            if (pawn.abilities == null)
            {
                pawn.abilities = new Pawn_AbilityTracker(pawn);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref draftableCreatures, "draftableCreatures", LookMode.Reference);
        }
    }
}


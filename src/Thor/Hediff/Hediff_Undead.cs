using Verse;

namespace Thor
{
    public class Hediff_Undead : HediffWithComps
    {
        private Pawn referencedPawn;
        public Pawn Master => referencedPawn;
        public override string Label => base.Label;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref referencedPawn, "referencedPawn");
            DraftingUtility.MakeDraftable(pawn);
        }

        public override string DebugString()
        {
            string baseString = base.DebugString();
            if (referencedPawn != null)
            {
                baseString += $"\nMaster Pawn: {referencedPawn.Name}";
            }
            else
            {
                baseString += "\nNo Master";
            }
            return baseString;
        }

        public void SetMaster(Pawn pawn)
        {
            referencedPawn = pawn;
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
        }
    }
}

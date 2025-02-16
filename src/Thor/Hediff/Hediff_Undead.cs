using RimWorld;
using System.Collections.Generic;
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

        public override void PostTick()
        {
            base.PostTick();

            if (referencedPawn != null && referencedPawn.Dead)
            {
                this.pawn.Kill(null);
            }
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

    //public class Hediff_UndeadMaster : HediffWithComps
    //{
    //    private const int CursedSpiritLimit = 5;
    //    private List<Pawn> storedCursedSpirits = new List<Pawn>();
    //    private List<Pawn> activeCursedSpirits = new List<Pawn>();

    //    public List<Pawn> GetAbsorbedCreatures()
    //    {
    //        return storedCursedSpirits;
    //    }
    //    public List<Pawn> GetActiveCreatures()
    //    {
    //        return new List<Pawn>(activeCursedSpirits);
    //    }
    //    public bool IsCreatureActive(Pawn absorbedPawn)
    //    {
    //        return activeCursedSpirits.Any(p => p == absorbedPawn);
    //    }

    //    public int GetAbsorbedCreatureCount(Pawn Pawn)
    //    {
    //        return storedCursedSpirits.FindAll(x => x.kindDef == pawn.kindDef).Count;
    //    }

    //    public void RemoveSummon(Pawn pawn, bool removeStoredPawn = true)
    //    {
    //        if (IsCreatureActive(pawn))
    //        {
    //            UnsummonCreature(pawn);
    //        }

    //        if (removeStoredPawn)
    //        {
    //            storedCursedSpirits.Remove(pawn);
    //        }
    //    }

    //    public Pawn GetActiveSummonOfPawn(Pawn absorbedPawn)
    //    {
    //        return activeCursedSpirits.FirstOrDefault(p => p == absorbedPawn);
    //    }

    //    public bool UnsummonCreature(Pawn pawn)
    //    {
    //        if (activeCursedSpirits.Remove(pawn))
    //        {
    //            if (pawn.Spawned && !pawn.Destroyed)
    //            {
    //                pawn.DeSpawn();
    //            }

    //            return true;
    //        }
    //        return false;
    //    }

    //    public bool SummonCreature(Pawn absorbedPawn)
    //    {
    //        if (storedCursedSpirits.Contains(absorbedPawn))
    //        {
    //            Pawn summonedPawn = absorbedPawn;
    //            //Log.Message($"Summoning pawn: {summonedPawn.LabelShort}, Faction: {summonedPawn.Faction}, Draftable: {summonedPawn.drafter != null}");

    //            GenSpawn.Spawn(summonedPawn, pawn.Position, pawn.Map);
    //            //Log.Message($"After spawn: Faction: {summonedPawn.Faction}, Draftable: {summonedPawn.drafter != null}");



    //            if (summonedPawn.Faction != Faction.OfPlayer)
    //            {
    //                summonedPawn.SetFaction(Faction.OfPlayer, this.pawn);
    //            }


    //            Hediff_Undead shikigami = (Hediff_Undead)summonedPawn.health.GetOrAddHediff(JJKDefOf.JJK_Shikigami);
    //            shikigami.SetMaster(pawn);
    //            if (summonedPawn.abilities == null)
    //            {
    //                summonedPawn.abilities = new Pawn_AbilityTracker(summonedPawn);
    //            }
    //            .TrainPawn(summonedPawn, this.pawn);
    //            //summonedPawn.abilities.GainAbility(JJKDefOf.JJK_CastLightningStrike);
    //            activeCursedSpirits.Add(summonedPawn);
    //            DraftingUtility.MakeDraftable(summonedPawn);

    //            // Log.Message($"After setup: Faction: {summonedPawn.Faction}, Draftable: {summonedPawn.drafter != null}");
    //            return true;
    //        }
    //        return false;
    //    }

    //    public bool HasAbsorbedCreature(Pawn absorbedPawn)
    //    {
    //        return storedCursedSpirits.Contains(absorbedPawn);
    //    }

    //    public void DeleteAbsorbedCreature(Pawn absorbedPawn)
    //    {
    //        if (storedCursedSpirits.Remove(absorbedPawn))
    //        {
    //            Pawn activeSummon = GetActiveSummonOfPawn(absorbedPawn);
    //            if (activeSummon != null)
    //            {
    //                UnsummonCreature(activeSummon);
    //            }
    //        }
    //    }

    //    public bool CanAbsorbNewCreature()
    //    {
    //        return storedCursedSpirits.Count < CursedSpiritLimit;
    //    }

    //    public void AbsorbCreature(Pawn targetPawn)
    //    {
    //        if (CanAbsorbNewCreature())
    //        {
    //            storedCursedSpirits.Add(targetPawn);
    //        }
    //    }

    //    public override string Description => base.Description + $"\r\nThis pawn has absorbed {storedCursedSpirits.Count} cursed spirits.";

    //    public override void ExposeData()
    //    {
    //        base.ExposeData();
    //        Scribe_Collections.Look(ref storedCursedSpirits, "storedCursedSpirits", LookMode.Deep);
    //        Scribe_Collections.Look(ref activeCursedSpirits, "activeCursedSpirits", LookMode.Reference);
    //    }

    //    public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
    //    {
    //        base.Notify_PawnDied(dinfo, culprit);
    //    }


    //    private void ReleaseCursedSpirit(Pawn cursedSpirit)
    //    {
    //        if (!cursedSpirit.Spawned)
    //        {
    //            GenSpawn.Spawn(cursedSpirit, pawn.Position, this.pawn.MapHeld);
    //        }

    //        // Remove the Shikigami hediff
    //        Hediff shikigamiHediff = cursedSpirit.health.hediffSet.GetFirstHediffOfDef(JJKDefOf.JJK_Shikigami);
    //        if (shikigamiHediff != null)
    //        {
    //            cursedSpirit.health.RemoveHediff(shikigamiHediff);
    //        }

    //        // Set to neutral faction or another appropriate faction

    //        if (cursedSpirit.Faction != Faction.OfPirates)
    //        {
    //            cursedSpirit.SetFaction(Faction.OfPirates);
    //        }


    //        // Optional: Make the released spirit go berserk
    //        MentalStateDef berserk = DefDatabase<MentalStateDef>.GetNamed("Berserk");
    //        if (berserk != null)
    //        {
    //            cursedSpirit.mindState.mentalStateHandler.TryStartMentalState(berserk);
    //        }
    //    }
    //}
}

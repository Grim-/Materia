using RimWorld;
using UnityEngine;
using Verse;

namespace Thor
{
    public class Hediff_Undead : HediffWithComps
    {
        UndeadHediffDef Def => (UndeadHediffDef)def;

        private Pawn referencedPawn;
        public Pawn Master => referencedPawn;
        public override string Label => base.Label;

        public bool CalledToArms = false;

        public void SetMaster(Pawn pawn)
        {
            referencedPawn = pawn;
            ApplyVisual();
        }

        private void ApplyVisual()
        {
            this.pawn.story.skinColorOverride = Color.white;
            pawn.story.HairColor = new Color(0.85f, 0.85f, 0.85f);
        }

        public override void Tick()
        {
            base.Tick();
            if (this.pawn != null && !this.pawn.Dead && !this.pawn.Destroyed && this.Def != null && this.pawn.IsHashIntervalTick(Def.regenTicks))
            {
                this.pawn.QuickHeal(Def.baseHealAmount);
                HandleNeeds();
            }
        }

        private void HandleNeeds()
        {
            foreach (var item in this.pawn.needs.AllNeeds)
            {
                item.CurLevel = item.MaxLevel;
            }

            if (this.pawn.IsSlave)
            {
                Need_Suppression suppression = this.pawn.needs.TryGetNeed<Need_Suppression>();
                if (suppression != null)
                {
                    suppression.CurLevel = suppression.MaxLevel;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref referencedPawn, "referencedPawn");
            Scribe_Values.Look(ref CalledToArms, "calledToArms");
            DraftingUtility.MakeDraftable(pawn);
        }
    }

    public class UndeadHediffDef : HediffDef
    {
        public int regenTicks = 2400;
        public float baseHealAmount = 2f;


        public UndeadHediffDef()
        {
            hediffClass = typeof(Hediff_Undead);
        }
    }
}

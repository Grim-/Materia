using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace Thor
{
    public class ITab_UndeadMaster : ITab
    {
        private Vector2 scrollPosition = Vector2.zero;
        private const float ROW_HEIGHT = 40f;
        private const float ICON_SIZE = 30f;
        private const float LABEL_WIDTH = 100f;
        private const float HEALTH_WIDTH = 80f;
        private const float BUTTON_WIDTH = 80f;
        private const float COLUMN_SPACING = 5f;
        private const float SPACING = 5f;
        private int CurrentTabIndex = 0;


        private Hediff_UndeadMaster _UndeadMaster;
        private Hediff_UndeadMaster UndeadMaster
        {
            get
            {
                if (_UndeadMaster == null)
                {
                    _UndeadMaster = this.SelPawn.health.hediffSet.GetFirstHediffOfDef(ThorDefOf.DeathKnight_UndeadMaster) as Hediff_UndeadMaster;
                }

                return _UndeadMaster;
            }
        }

        public override bool IsVisible => base.IsVisible && this.SelPawn != null && this.SelPawn.health.hediffSet.HasHediff(ThorDefOf.DeathKnight_UndeadMaster);


        public ITab_UndeadMaster()
        {
            this.labelKey = "Undead";
            this.tutorTag = "Undead";
            this.size = new Vector2(500f, 450f);
        }

        protected override void FillTab()
        {
            Rect rect = new Rect(0f, 0f, this.size.x, this.size.y).ContractedBy(10f);
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, 5000f);
            Pawn pawn = (Pawn)this.SelPawn;
   

            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(viewRect);

            if (pawn != null && UndeadMaster != null)
            {
                var absorbedCreatures = UndeadMaster.GetStored();
                listingStandard.Label($"Total Absorbed Creatures: {absorbedCreatures.Count}");

                if (listingStandard.ButtonText("Summon All"))
                {
                    foreach (var item in absorbedCreatures.ToArray())
                    {
                        UndeadMaster.SummonCreature(item);
                    }
                }

                if (listingStandard.ButtonText("Dismiss All"))
                {
                    foreach (var item in UndeadMaster.GetActiveCreatures())
                    {
                        UndeadMaster.UnsummonCreature(item);
                    }
                }


                if (listingStandard.ButtonText("Debug Generate 10"))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Pawn newPawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                            kind: PawnKindDefOf.Colonist,
                            mustBeCapableOfViolence: true,
                            allowDead: false
                        ));

                        if (newPawn != null)
                        {                      
                            UndeadMaster.AbsorbCreature(newPawn);
                        }
                    }
                }

                listingStandard.GapLine();

                var groupedCreatures = absorbedCreatures
                    .GroupBy(c => c)
                    .OrderByDescending(g => g.Count());

                foreach (var group in groupedCreatures)
                {
                    DrawRow(pawn, group.Key, listingStandard);
                }
            }
            else
            {
                listingStandard.Label("No data available");
            }

            listingStandard.End();
            Widgets.EndScrollView();
        }

        private void DrawRow(Pawn pawn,Pawn absorbedCreature, Listing_Standard listingStandard)
        {
            Rect rowRect = listingStandard.GetRect(ROW_HEIGHT);
            var layout = new RowLayoutManager(rowRect);
            Rect iconRect = layout.NextRect(ICON_SIZE, COLUMN_SPACING);
            Rect labelRect = layout.NextRect(LABEL_WIDTH, COLUMN_SPACING);
            Rect healthRect = layout.NextRect(HEALTH_WIDTH, COLUMN_SPACING);
            Rect summonButtonRect = layout.NextRect(BUTTON_WIDTH, COLUMN_SPACING);
            Rect removeButtonRect = layout.NextRect(BUTTON_WIDTH, COLUMN_SPACING);
            Rect calledToArmsRect = layout.NextRect(BUTTON_WIDTH);



            Hediff_Undead undead = absorbedCreature.health.hediffSet.GetFirstHediff<Hediff_Undead>();

            Widgets.DrawTextureFitted(iconRect, absorbedCreature.kindDef.race.uiIcon, 1f);
            Widgets.HyperlinkWithIcon(iconRect, new Dialog_InfoCard.Hyperlink(absorbedCreature.def));
            Widgets.Label(labelRect, $"{absorbedCreature.Label}");
            Widgets.FillableBar(healthRect, absorbedCreature.health.summaryHealth.SummaryHealthPercent);

            if (UndeadMaster.IsCreatureActive(absorbedCreature))
            {
                if (Widgets.ButtonText(summonButtonRect, "UnSummon"))
                {
                    UndeadMaster.UnsummonCreature(absorbedCreature);
                }
            }
            else
            {
                if (Widgets.ButtonText(summonButtonRect, "Summon"))
                {
                    UndeadMaster.SummonCreature(absorbedCreature);
                }
            }


            if (undead != null)
            {
                Widgets.Checkbox(new Vector2(calledToArmsRect.x, calledToArmsRect.y), ref undead.CalledToArms);
            }


            if (Widgets.ButtonText(removeButtonRect, "Remove"))
            {
                UndeadMaster.DeleteAbsorbedCreature(absorbedCreature);
            }
        }

    }
}

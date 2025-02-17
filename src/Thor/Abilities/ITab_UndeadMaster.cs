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

        public ITab_UndeadMaster()
        {
            this.labelKey = "TabCursedSpirit";
            this.tutorTag = "CursedSpirit";
            this.size = new Vector2(450f, 450f);
        }

        protected override void FillTab()
        {
            Rect rect = new Rect(0f, 0f, this.size.x, this.size.y).ContractedBy(10f);
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, 1000f);
            Pawn pawn = (Pawn)this.SelPawn;
            Hediff_UndeadMaster cursedSpiritManipulator = pawn.health.hediffSet.GetFirstHediffOfDef(ThorDefOf.DeathKnight_UndeadMaster) as Hediff_UndeadMaster;

            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(viewRect);

            if (pawn != null && cursedSpiritManipulator != null)
            {
                var absorbedCreatures = cursedSpiritManipulator.GetStored();
                listingStandard.Label($"Total Absorbed Creatures: {absorbedCreatures.Count}");
                listingStandard.GapLine();

                var groupedCreatures = absorbedCreatures
                    .GroupBy(c => c)
                    .OrderByDescending(g => g.Count());

                foreach (var group in groupedCreatures)
                {
                    DrawRow(pawn, cursedSpiritManipulator, group.Key, listingStandard);
                }
            }
            else
            {
                listingStandard.Label("No data available");
            }

            listingStandard.End();
            Widgets.EndScrollView();
        }

        private void DrawRow(Pawn pawn, Hediff_UndeadMaster cursedSpiritManipulator, Pawn absorbedCreature, Listing_Standard listingStandard)
        {
            Rect rowRect = listingStandard.GetRect(ROW_HEIGHT);
            var layout = new RowLayoutManager(rowRect);
            Rect iconRect = layout.NextRect(ICON_SIZE, COLUMN_SPACING);
            Rect labelRect = layout.NextRect(LABEL_WIDTH, COLUMN_SPACING);
            Rect healthRect = layout.NextRect(HEALTH_WIDTH, COLUMN_SPACING);
            Rect summonButtonRect = layout.NextRect(BUTTON_WIDTH, COLUMN_SPACING);
            Rect removeButtonRect = layout.NextRect(BUTTON_WIDTH);

            Widgets.DrawTextureFitted(iconRect, absorbedCreature.kindDef.race.uiIcon, 1f);
            Widgets.HyperlinkWithIcon(iconRect, new Dialog_InfoCard.Hyperlink(absorbedCreature.def));
            Widgets.Label(labelRect, $"{absorbedCreature.Label}");
            Widgets.FillableBar(healthRect, absorbedCreature.health.summaryHealth.SummaryHealthPercent);

            if (cursedSpiritManipulator.IsCreatureActive(absorbedCreature))
            {
                if (Widgets.ButtonText(summonButtonRect, "UnSummon"))
                {
                    cursedSpiritManipulator.UnsummonCreature(absorbedCreature);
                }
            }
            else
            {
                if (Widgets.ButtonText(summonButtonRect, "Summon"))
                {
                    cursedSpiritManipulator.SummonCreature(absorbedCreature);
                }
            }

            if (Widgets.ButtonText(removeButtonRect, "Remove"))
            {
                cursedSpiritManipulator.DeleteAbsorbedCreature(absorbedCreature);
            }
        }

        public override bool IsVisible => base.IsVisible && this.SelPawn != null && this.SelPawn.health.hediffSet.HasHediff(ThorDefOf.DeathKnight_UndeadMaster);
    }
}

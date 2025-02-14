using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Thor
{
    public class Window_MateriaSelection : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        private readonly Thing parentThing;
        private readonly Comp_MateriaComp materiaComp;
        private static readonly Vector2 WinSize = new Vector2(600f, 400f);

        private const float SLOT_SIZE = 120f;
        private const float SLOT_PADDING = 10f;
        private const float SECTION_PADDING = 20f;
        private const float SLOTS_PER_ROW = 4;
        private const float TOP_MARGIN = 15f; // Added margin below title

        private const float LABEL_HEIGHT = 40f;
        private const float TITLE_HEIGHT = 35f;
        private Color TITLE_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        private Color FOOTER_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        private Color SLOT_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        private const float INNER_PADDING = 5f;

        public override Vector2 InitialSize => WinSize;

        public Window_MateriaSelection(Thing thing, Comp_MateriaComp comp)
        {
            this.parentThing = thing;
            this.materiaComp = comp;
            this.doCloseX = true;
            this.doCloseButton = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
        }

        private void DrawTitle(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect titleRect = new Rect(0f, 0f, inRect.width, TITLE_HEIGHT);
            GUI.DrawTexture(titleRect, SolidColorMaterials.NewSolidColorTexture(TITLE_COLOR));

            string titleText = $"Materia Slots - {parentThing.Label}";
            Widgets.Label(titleRect, titleText);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void TryDrawIcon(Rect iconRect, MateriaInstance materia)
        {
            if (materia?.def != null)
            {
                Texture2D iconTexture = ContentFinder<Texture2D>.Get(materia.def.texPath);
                if (iconTexture != null)
                {
                    Widgets.DrawTextureFitted(iconRect, iconTexture, 0.8f);
                }
            }
        }

        private void DrawMateriaSlot(Rect slotRect, MateriaSlot slot)
        {
            GUI.color = Color.white;
            Widgets.DrawBox(slotRect, 1, SolidColorMaterials.NewSolidColorTexture(Color.grey));

            Rect innerRect = slotRect.ContractedBy(INNER_PADDING);
            float labelHeight = LABEL_HEIGHT;

            Rect iconRect = new Rect(
                innerRect.x,
                innerRect.y,
                innerRect.width,
                innerRect.height - labelHeight
            );

            if (slot.IsOccupied)
            {
                TryDrawIcon(iconRect, slot.SlottedMateria);
            }
            else
            {
                GUI.DrawTexture(iconRect, SolidColorMaterials.NewSolidColorTexture(SLOT_COLOR));
            }


            Rect footerRect = new Rect(
                slotRect.x,
                slotRect.y + slotRect.height - labelHeight,
                slotRect.width,
                labelHeight
            );
            GUI.DrawTexture(footerRect, SolidColorMaterials.NewSolidColorTexture(FOOTER_COLOR));

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(footerRect, slot.SlotType?.label ?? "Any");

            if (Mouse.IsOver(slotRect))
            {
                Widgets.DrawHighlight(slotRect);

                if (slot.IsOccupied)
                {
                    string desc = slot.SlottedMateria.def.description;
                    TooltipHandler.TipRegion(slotRect,
                        $"{slot.SlottedMateria.def.label}\n\n{desc}\n\nRight-click to remove");

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
                    {
                        slot.UnequipMateria();
                        Event.current.Use();
                    }
                }
                else
                {
                    TooltipHandler.TipRegion(slotRect,
                        $"Empty {slot.SlotType?.label ?? "materia"} slot\nClick to equip materia");

                    if (Widgets.ButtonInvisible(slotRect))
                    {
                        Find.WindowStack.Add(new FloatMenu(GenerateMateriaOptions(slot)));
                    }
                }
            }

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }

        private List<FloatMenuOption> GenerateMateriaOptions(MateriaSlot slot)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            foreach (MateriaDef materiaDef in DefDatabase<MateriaDef>.AllDefs)
            {
                if (slot.CanAccept(materiaDef))
                {
                    options.Add(new FloatMenuOption(materiaDef.label, () =>
                    {
                        slot.EquipMateria(materiaDef);
                    }));
                }
            }

            if (options.Count == 0)
            {
                options.Add(new FloatMenuOption("No compatible materia available", null));
            }

            return options;
        }

        public override void DoWindowContents(Rect inRect)
        {
            DrawTitle(inRect);

            float totalHeight = TITLE_HEIGHT + TOP_MARGIN;
            var slots = materiaComp.MateriaSlots;
            if (slots != null && slots.Count > 0)
            {
                totalHeight += Mathf.Ceil(slots.Count / SLOTS_PER_ROW) * (SLOT_SIZE + SLOT_PADDING);
                totalHeight += SECTION_PADDING;
            }

            Rect outRect = new Rect(0f, TITLE_HEIGHT, inRect.width, inRect.height - TITLE_HEIGHT);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, totalHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            float curY = TOP_MARGIN;
            float curX = 0f;

            if (slots != null)
            {
                foreach (MateriaSlot slot in slots)
                {
                    if (curX + SLOT_SIZE > viewRect.width)
                    {
                        curX = 0f;
                        curY += SLOT_SIZE + SLOT_PADDING;
                    }

                    Rect slotRect = new Rect(curX, curY, SLOT_SIZE, SLOT_SIZE);
                    DrawMateriaSlot(slotRect, slot);
                    curX += SLOT_SIZE + SLOT_PADDING;
                }
            }

            Widgets.EndScrollView();
        }
    }
}
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using Verse;

//namespace Thor
//{
//    [StaticConstructorOnStartup]
//    public class Window_MateriaSelection : Window
//    {
//        private Vector2 scrollPosition = Vector2.zero;
//        private readonly Thing parentThing;
//        private readonly Comp_MateriaComp materiaComp;
//        private readonly Pawn usingPawn;
//        private static readonly Vector2 WinSize = new Vector2(600f, 400f);

//        private const float SLOT_SIZE = 120f;
//        private const float SLOT_PADDING = 10f;
//        private const float SECTION_PADDING = 20f;
//        private const float SLOTS_PER_ROW = 4;
//        private const float TOP_MARGIN = 15f;
//        private const float DEV_BUTTON_HEIGHT = 30f;
//        private const float DEV_BUTTON_WIDTH = 100f;

//        private const float LABEL_HEIGHT = 40f;
//        private const float TITLE_HEIGHT = 35f;
//        private Color TITLE_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);
//        private Color FOOTER_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);
//        private Color SLOT_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);
//        private const float INNER_PADDING = 5f;

//        public override Vector2 InitialSize => WinSize;



//        protected Texture2D SlotTex = ContentFinder<Texture2D>.Get("UI/RuneSlot");


//        public Window_MateriaSelection(Thing thing, Pawn Pawn, Comp_MateriaComp comp)
//        {
//            this.parentThing = thing;
//            this.materiaComp = comp;
//            this.doCloseX = true;
//            this.doCloseButton = true;
//            this.absorbInputAroundWindow = true;
//            this.forcePause = true;
//            this.usingPawn = Pawn;
//        }

//        private void DrawTitle(Rect inRect)
//        {
//            Text.Font = GameFont.Medium;
//            Text.Anchor = TextAnchor.MiddleCenter;
//            Rect titleRect = new Rect(0f, 0f, inRect.width, TITLE_HEIGHT);
//            GUI.DrawTexture(titleRect, SolidColorMaterials.NewSolidColorTexture(TITLE_COLOR));

//            string titleText = $"Materia Slots - {parentThing.Label}";
//            Widgets.Label(titleRect, titleText);
//            Text.Anchor = TextAnchor.UpperLeft;

//            if (Prefs.DevMode)
//            {
//                Rect buttonRect = new Rect(
//                    inRect.width - DEV_BUTTON_WIDTH - 10f,
//                    5f,
//                    DEV_BUTTON_WIDTH,
//                    DEV_BUTTON_HEIGHT
//                );

//                if (Widgets.ButtonText(buttonRect, "Add Materia Slot"))
//                {
//                    List<FloatMenuOption> menuOptions = new List<FloatMenuOption>();
//                    foreach (var item in DefDatabase<MateriaTypeDef>.AllDefsListForReading)
//                    {
//                        menuOptions.Add(new FloatMenuOption($"Add {item.defName} slot", () =>
//                        {
//                            AddMateriaSlot(item);
//                        }));
//                    }
//                    Find.WindowStack.Add(new FloatMenu(menuOptions));
//                }
//            }
//        }

//        private void AddMateriaSlot(MateriaTypeDef slotType)
//        {
//            if (materiaComp != null)
//            {
//                materiaComp.AddMateriaSlot(slotType);
//            }
//        }

//        private void TryDrawIcon(Rect iconRect, MateriaInstance materia)
//        {
//            if (materia?.def != null)
//            {
//                Texture2D iconTexture = ContentFinder<Texture2D>.Get(materia.def.texPath);
//                if (iconTexture != null)
//                {
//                    Widgets.DrawTextureFitted(iconRect, iconTexture, 0.8f);
//                }
//            }
//        }
//        private void DrawMateriaSlot(Rect slotRect, MateriaSlot slot)
//        {
//            Color slotTypeColor = slot.SlotType?.materiaTextColor ?? Color.grey;

//            GUI.color = Color.white;
//            Widgets.DrawBox(slotRect, 2, SolidColorMaterials.NewSolidColorTexture(slotTypeColor));

//            Color bgColor = new Color(
//                slotTypeColor.r * 0.3f,
//                slotTypeColor.g * 0.3f,
//                slotTypeColor.b * 0.3f,
//                0.2f
//            );
//            GUI.DrawTexture(slotRect, SolidColorMaterials.NewSolidColorTexture(bgColor));


//            Rect innerRect = slotRect.ContractedBy(INNER_PADDING);
//            float labelHeight = LABEL_HEIGHT;


//            Rect iconRect = new Rect(
//                innerRect.x,
//                innerRect.y,
//                innerRect.width,
//                innerRect.height - labelHeight
//            );

//            // Draw icon or empty slot
//            if (slot.IsOccupied)
//            {
//                TryDrawIcon(iconRect, slot.SlottedMateria);
//                GUI.color = new Color(slotTypeColor.r, slotTypeColor.g, slotTypeColor.b, 0.3f);
//                Widgets.DrawBox(iconRect.ExpandedBy(2f), 1);
//                GUI.color = Color.white;
//            }
//            else
//            {
//                GUI.DrawTexture(iconRect, SolidColorMaterials.NewSolidColorTexture(SLOT_COLOR));
//            }


//            Rect footerRect = new Rect(
//                slotRect.x,
//                slotRect.y + slotRect.height - labelHeight,
//                slotRect.width,
//                labelHeight
//            );


//            Color footerColor = new Color(
//                slotTypeColor.r * 0.8f,
//                slotTypeColor.g * 0.8f,
//                slotTypeColor.b * 0.8f,
//                0.3f
//            );
//            GUI.DrawTexture(footerRect, SolidColorMaterials.NewSolidColorTexture(footerColor));

//            // Draw slot type label
//            Text.Font = GameFont.Small;
//            Text.Anchor = TextAnchor.MiddleCenter;
//            GUI.color = slotTypeColor;
//            Widgets.Label(footerRect, slot.SlotType?.label ?? "Any");
//            GUI.color = Color.white;

//            if (Mouse.IsOver(slotRect))
//            {
//                Color highlightColor = new Color(
//                    slotTypeColor.r,
//                    slotTypeColor.g,
//                    slotTypeColor.b,
//                    0.4f
//                );
//                Widgets.DrawBox(slotRect, 3, SolidColorMaterials.NewSolidColorTexture(highlightColor));

//                if (slot.IsOccupied)
//                {
//                    string desc = slot.SlottedMateria.def.description;
//                    TooltipHandler.TipRegion(slotRect,
//                        $"{slot.SlottedMateria.def.label}\n\n{desc}\n\nRight-click to remove");

//                    if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
//                    {
//                        slot.UnequipMateria();
//                        Event.current.Use();
//                    }
//                }
//                else
//                {
//                    TooltipHandler.TipRegion(slotRect,
//                        $"Empty {slot.SlotType?.label ?? "materia"} slot\nClick to equip materia");

//                    if (Widgets.ButtonInvisible(slotRect))
//                    {
//                        Find.WindowStack.Add(new FloatMenu(GenerateMateriaOptions(slot)));
//                    }
//                }
//            }

//            Text.Anchor = TextAnchor.UpperLeft;
//            GUI.color = Color.white;
//        }


//        private List<FloatMenuOption> GenerateMateriaOptions(MateriaSlot slot)
//        {
//            List<FloatMenuOption> options = new List<FloatMenuOption>();

//            if (Prefs.DevMode)
//            {
//                foreach (MateriaDef materiaDef in DefDatabase<MateriaDef>.AllDefs)
//                {
//                    if (slot.CanAccept(materiaDef))
//                    {
//                        options.Add(new FloatMenuOption(materiaDef.label, () =>
//                        {
//                            slot.EquipMateria(materiaDef);
//                        }));
//                    }
//                }
//            }
//            else
//            {
//                foreach (Thing thing in usingPawn.EquippedWornOrInventoryThings)
//                {
//                    if (thing.def is MateriaDef materiaDef && slot.CanAccept(materiaDef))
//                    {
//                        options.Add(new FloatMenuOption(materiaDef.GetColouredLabel(), () =>
//                        {
//                            slot.EquipMateria(materiaDef);
//                            usingPawn.inventory.RemoveCount(thing.def, 1);
//                        }));
//                    }
//                }
//            }

//            if (options.Count == 0)
//            {
//                options.Add(new FloatMenuOption("No compatible materia available", null));
//            }

//            return options;
//        }

//        public override void DoWindowContents(Rect inRect)
//        {
//            DrawTitle(inRect);

//            float totalHeight = TITLE_HEIGHT + TOP_MARGIN;
//            var slots = materiaComp.MateriaSlots;
//            if (slots != null && slots.Count > 0)
//            {
//                totalHeight += Mathf.Ceil(slots.Count / SLOTS_PER_ROW) * (SLOT_SIZE + SLOT_PADDING);
//                totalHeight += SECTION_PADDING;
//            }

//            Rect outRect = new Rect(0f, TITLE_HEIGHT, inRect.width, inRect.height - TITLE_HEIGHT);
//            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, totalHeight);

//            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

//            float curY = TOP_MARGIN;
//            float curX = 0f;

//            if (slots != null)
//            {
//                foreach (MateriaSlot slot in slots)
//                {
//                    if (curX + SLOT_SIZE > viewRect.width)
//                    {
//                        curX = 0f;
//                        curY += SLOT_SIZE + SLOT_PADDING;
//                    }

//                    Rect slotRect = new Rect(curX, curY, SLOT_SIZE, SLOT_SIZE);
//                    DrawMateriaSlot(slotRect, slot);
//                    curX += SLOT_SIZE + SLOT_PADDING;
//                }
//            }

//            Widgets.EndScrollView();
//        }
//    }
//}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Thor
{
    [StaticConstructorOnStartup]
    public class Window_MateriaSelection : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        private readonly Thing parentThing;
        private readonly Comp_MateriaComp materiaComp;
        private readonly Pawn usingPawn;
        private static readonly Vector2 WinSize = new Vector2(600f, 400f);

        private const float SLOT_SIZE = 100f;
        private const float SLOT_PADDING = 10f;
        private const float SECTION_PADDING = 20f;
        private const float SLOTS_PER_ROW = 4;
        private const float TOP_MARGIN = 15f;
        private const float DEV_BUTTON_HEIGHT = 30f;
        private const float DEV_BUTTON_WIDTH = 100f;

        private const float LABEL_HEIGHT = 40f;
        private const float TITLE_HEIGHT = 35f;
        private Color TITLE_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        private Color FOOTER_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        private Color SLOT_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        private const float INNER_PADDING = 5f;

        public override Vector2 InitialSize => WinSize;

        protected Texture2D SlotTex = ContentFinder<Texture2D>.Get("UI/RuneSlot");

        public Window_MateriaSelection(Thing thing, Pawn Pawn, Comp_MateriaComp comp)
        {
            this.parentThing = thing;
            this.materiaComp = comp;
            this.doCloseX = true;
            this.doCloseButton = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
            this.usingPawn = Pawn;
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

            if (Prefs.DevMode)
            {
                Rect buttonRect = new Rect(
                    inRect.width - DEV_BUTTON_WIDTH - 10f,
                    5f,
                    DEV_BUTTON_WIDTH,
                    DEV_BUTTON_HEIGHT
                );

                if (Widgets.ButtonText(buttonRect, "Add Materia Slot"))
                {
                    List<FloatMenuOption> menuOptions = new List<FloatMenuOption>();
                    foreach (var item in DefDatabase<MateriaTypeDef>.AllDefsListForReading)
                    {
                        menuOptions.Add(new FloatMenuOption($"Add {item.defName} slot", () =>
                        {
                            AddMateriaSlot(item);
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(menuOptions));
                }
            }
        }

        private void AddMateriaSlot(MateriaTypeDef slotType)
        {
            if (materiaComp != null)
            {
                materiaComp.AddMateriaSlot(slotType);
            }
        }

        private void TryDrawIcon(Rect iconRect, MateriaInstance materia)
        {
            if (materia?.def != null)
            {
                Texture2D iconTexture = materia.def.IconTex;
                if (iconTexture != null)
                {
                    Widgets.DrawTextureFitted(iconRect, iconTexture, 1f);
                }
            }
        }

        private void DrawMateriaSlot(Rect slotRect, MateriaSlot slot)
        {
            Color slotTypeColor = slot.SlotType?.materiaTextColor ?? Color.grey;

            GUI.color = slotTypeColor;
            GUI.DrawTexture(slotRect, SlotTex);
            GUI.color = Color.white;

            Rect innerRect = slotRect.ContractedBy(INNER_PADDING);
            float labelHeight = LABEL_HEIGHT;

            Rect iconRect = new Rect(
                slotRect.x + (slotRect.width * 0.1f),
                slotRect.y + (slotRect.height * 0.1f),
                slotRect.width * 0.8f,
                slotRect.height * 0.8f
            );

  
            if (slot.IsOccupied)
            {
                TryDrawIcon(iconRect, slot.SlottedMateria);
            }


            Rect footerRect = new Rect(
                slotRect.x,
                slotRect.y + slotRect.height,
                slotRect.width,
                labelHeight
            );


            Color footerColor = new Color(
                slotTypeColor.r,
                slotTypeColor.g,
                slotTypeColor.b,
                0.5f
            );
            GUI.DrawTexture(footerRect, SolidColorMaterials.NewSolidColorTexture(footerColor));

            // Draw slot type label
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = Color.white;
            Widgets.Label(footerRect, slot.SlotType?.label ?? "Any");

            if (Mouse.IsOver(slotRect))
            {
                Color highlightColor = new Color(
                    slotTypeColor.r,
                    slotTypeColor.g,
                    slotTypeColor.b,
                    0.4f
                );
                Widgets.DrawBox(slotRect, 3, SolidColorMaterials.NewSolidColorTexture(highlightColor));

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

            if (Prefs.DevMode)
            {
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
            }
            else
            {
                foreach (Thing thing in usingPawn.EquippedWornOrInventoryThings)
                {
                    if (thing.def is MateriaDef materiaDef && slot.CanAccept(materiaDef))
                    {
                        options.Add(new FloatMenuOption(materiaDef.GetColouredLabel(), () =>
                        {
                            slot.EquipMateria(materiaDef);
                            usingPawn.inventory.RemoveCount(thing.def, 1);
                        }));
                    }
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
                totalHeight += Mathf.Ceil(slots.Count / SLOTS_PER_ROW) * (SLOT_SIZE + LABEL_HEIGHT + SLOT_PADDING);
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
                        curY += SLOT_SIZE + LABEL_HEIGHT + SLOT_PADDING;
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
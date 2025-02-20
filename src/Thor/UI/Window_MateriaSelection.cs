using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Materia
{
    //[StaticConstructorOnStartup]
    //public class Window_MateriaSelection : Window
    //{
    //    private Vector2 scrollPosition = Vector2.zero;
    //    private readonly Thing parentThing;
    //    private readonly Comp_MateriaComp materiaComp;
    //    private readonly Pawn usingPawn;
    //    private static readonly Vector2 WinSize = new Vector2(600f, 400f);

    //    private const float SLOT_SIZE = 90f;
    //    private const float SLOT_PADDING = 10f;
    //    private const float SECTION_PADDING = 20f;
    //    private const float SLOTS_PER_ROW = 4;
    //    private const float TOP_MARGIN = 15f;
    //    private const float DEV_BUTTON_HEIGHT = 30f;
    //    private const float DEV_BUTTON_WIDTH = 100f;

    //    private const float LABEL_HEIGHT = 40f;
    //    private const float TITLE_HEIGHT = 35f;
    //    private Color TITLE_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);
    //    private Color FOOTER_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);
    //    private Color SLOT_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);
    //    private const float INNER_PADDING = 5f;

    //    public override Vector2 InitialSize => WinSize;

    //    protected Texture2D SlotTex = ContentFinder<Texture2D>.Get("UI/RuneSlot");
    //    protected Texture2D SlotConnectionTex = ContentFinder<Texture2D>.Get("UI/connection_lineglow");
    //    private static readonly Texture2D LockedTex = ContentFinder<Texture2D>.Get("UI/Lock_Closed");
    //    private static readonly Texture2D UnlockedTex = ContentFinder<Texture2D>.Get("UI/Lock_Open");
    //    private MateriaDisplayWorker displayWorker;



    //    public DisplaySettings DisplaySettings = new DisplaySettings();



    //    public Window_MateriaSelection(Thing thing, Pawn pawn, Comp_MateriaComp comp, Type preferredDisplayName = null)
    //    {
    //        this.parentThing = thing;
    //        this.materiaComp = comp;
    //        this.doCloseX = true;
    //        this.doCloseButton = true;
    //        this.absorbInputAroundWindow = true;
    //        this.forcePause = true;
    //        this.usingPawn = pawn;

    //        if (preferredDisplayName != null)
    //        {
    //            this.displayWorker = MateriaDisplayFactory.GetDisplay(preferredDisplayName);
    //        }
    //        else
    //        {
    //            this.displayWorker = MateriaDisplayFactory.GetBestDisplayForSlots(comp.MateriaSlots);
    //        }
    //    }

    //    private void DrawTitle(Rect inRect)
    //    {
    //        Text.Font = GameFont.Medium;
    //        Text.Anchor = TextAnchor.MiddleCenter;
    //        Rect titleRect = new Rect(0f, 0f, inRect.width, TITLE_HEIGHT);
    //        GUI.DrawTexture(titleRect, SolidColorMaterials.NewSolidColorTexture(TITLE_COLOR));

    //        string titleText = $"Materia Slots - {parentThing.Label}";
    //        Widgets.LabelFit(titleRect, titleText);
    //        Text.Anchor = TextAnchor.UpperLeft;

    //        if (Prefs.DevMode)
    //        {
    //            Rect buttonRect = new Rect(
    //                inRect.width - DEV_BUTTON_WIDTH - 10f,
    //                5f,
    //                DEV_BUTTON_WIDTH,
    //                DEV_BUTTON_HEIGHT
    //            );

    //            if (Widgets.ButtonText(buttonRect, "Add Materia Slot"))
    //            {
    //                List<FloatMenuOption> menuOptions = new List<FloatMenuOption>();
    //                foreach (var item in DefDatabase<MateriaTypeDef>.AllDefsListForReading)
    //                {
    //                    menuOptions.Add(new FloatMenuOption($"Add {item.defName} slot", () =>
    //                    {
    //                        AddMateriaSlot(item);
    //                    }));
    //                }
    //                Find.WindowStack.Add(new FloatMenu(menuOptions));
    //            }
    //        }
    //    }

    //    private void AddMateriaSlot(MateriaTypeDef slotType)
    //    {
    //        if (materiaComp != null)
    //        {
    //            materiaComp.AddMateriaSlot(slotType);
    //        }
    //    }

    //    private void TryDrawIcon(Rect iconRect, MateriaInstance materia)
    //    {
    //        if (materia?.def != null)
    //        {
    //            Texture2D iconTexture = materia.def.IconTex;
    //            if (iconTexture != null)
    //            {
    //                Widgets.DrawTextureFitted(iconRect, iconTexture, 1f);
    //            }
    //        }
    //    }

    //    private void DrawMateriaSlot(Rect slotRect, MateriaSlot slot)
    //    {
    //        Color slotTypeColor = slot.SlotType?.materiaTextColor ?? Color.grey;

    //        float lockSize = 16f;
    //        Rect lockRect = new Rect(
    //            slotRect.x + slotRect.width - lockSize,
    //            slotRect.y,
    //            lockSize,
    //            lockSize
    //        );


    //        if (Prefs.DevMode)
    //        {
    //            if (Widgets.ButtonImage(lockRect, slot.IsSlotLocked ? LockedTex : UnlockedTex))
    //            {
    //                slot.SetLockStatus(!slot.IsSlotLocked);
    //            }

    //            if (Mouse.IsOver(lockRect))
    //            {
    //                TooltipHandler.TipRegion(lockRect,
    //                    slot.IsSlotLocked ? "Click to unlock (Dev)" : "Click to lock (Dev)");
    //            }
    //        }
    //        else
    //        {
    //            if (slot.IsSlotLocked)
    //            {
    //                GUI.DrawTexture(lockRect, LockedTex);
    //            }
    //            else
    //            {
    //                GUI.DrawTexture(lockRect, UnlockedTex);
    //            }

    //            if (Mouse.IsOver(lockRect))
    //            {
    //                TooltipHandler.TipRegion(lockRect,
    //                    slot.IsSlotLocked ? "Locked" : "Unlocked");
    //            }
    //        }


    //        GUI.color = slotTypeColor;
    //        GUI.DrawTexture(slotRect, SlotTex);
    //        GUI.color = Color.white;



    //        Rect innerRect = slotRect.ContractedBy(INNER_PADDING);
    //        float labelHeight = LABEL_HEIGHT;

    //        Rect iconRect = new Rect(
    //            slotRect.x + (slotRect.width * 0.1f),
    //            slotRect.y + (slotRect.height * 0.1f),
    //            slotRect.width * 0.8f,
    //            slotRect.height * 0.8f
    //        );


    //        if (slot.IsOccupied)
    //        {
    //            TryDrawIcon(iconRect, slot.SlottedMateria);
    //        }
    //        Rect footerRect = new Rect(
    //            slotRect.x,
    //            slotRect.y + slotRect.height,
    //            slotRect.width,
    //            labelHeight
    //        );


    //        Color footerColor = new Color(
    //            slotTypeColor.r,
    //            slotTypeColor.g,
    //            slotTypeColor.b,
    //            0.5f
    //        );
    //        GUI.DrawTexture(footerRect, SolidColorMaterials.NewSolidColorTexture(footerColor));

    //        // Draw slot type label
    //        Text.Font = GameFont.Small;
    //        Text.Anchor = TextAnchor.MiddleCenter;
    //        GUI.color = Color.white;


    //        if (slot.IsOccupied)
    //        {
    //            Widgets.Label(footerRect, $"{slot.SlottedMateria.def.LabelCap}");
    //        }
    //        else
    //        {
    //            Widgets.Label(footerRect, slot.SlotType != null ? slot.SlotType.label + $" L:{slot.SlotLevel}" : $"Any L:{slot.SlotLevel}");
    //        }


    //        if (Mouse.IsOver(slotRect))
    //        {
    //            Color highlightColor = new Color(
    //                slotTypeColor.r,
    //                slotTypeColor.g,
    //                slotTypeColor.b,
    //                0.4f
    //            );
    //            Widgets.DrawBox(slotRect, 3, SolidColorMaterials.NewSolidColorTexture(highlightColor));

    //            if (slot.IsOccupied)
    //            {
    //                string desc = slot.SlottedMateria.def.description;
    //                TooltipHandler.TipRegion(slotRect,
    //                    $"{slot.SlottedMateria.def.label}\n\n{desc}\n\nRight-click to remove");

    //                if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && !slot.IsSlotLocked)
    //                {
    //                    materiaComp.UnequipMateria(slot);
    //                    Event.current.Use();
    //                }
    //            }
    //            else
    //            {
    //                TooltipHandler.TipRegion(slotRect,
    //                    $"Empty {slot.SlotType?.label ?? "materia"} slot\nClick to equip materia");

    //                if (Widgets.ButtonInvisible(slotRect))
    //                {
    //                    Find.WindowStack.Add(new FloatMenu(GenerateMateriaOptions(slot)));
    //                }
    //            }
    //        }

    //        Text.Anchor = TextAnchor.UpperLeft;
    //        GUI.color = Color.white;
    //    }


    //    public override void DoWindowContents(Rect inRect)
    //    {
    //        DrawTitle(inRect);

    //        float totalHeight = TITLE_HEIGHT + TOP_MARGIN;
    //        var slots = materiaComp.MateriaSlots;

    //        Rect outRect = new Rect(0f, TITLE_HEIGHT, inRect.width, inRect.height - TITLE_HEIGHT);
    //        //Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, totalHeight);

    //        //Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

    //        // Use display worker to position slots
    //        //Rect contentRect = new Rect(0f, TOP_MARGIN, viewRect.width, viewRect.height - TOP_MARGIN);
    //        Dictionary<MateriaSlot, Rect> slotPositions = displayWorker.PositionSlots(slots, outRect, SLOT_SIZE, SLOT_PADDING);

    //        // Draw each slot at its positioned location
    //        foreach (var slotEntry in slotPositions)
    //        {
    //            MateriaSlot slot = slotEntry.Key;
    //            Rect slotRect = slotEntry.Value;

    //            // Let the display worker handle custom drawing if implemented
    //            DrawMateriaSlot(slotRect, slot);
    //        }

    //        // Draw custom toolbar elements if any
    //        displayWorker.DrawToolBar(new Rect(0, outRect.height - DEV_BUTTON_HEIGHT - 10f, outRect.width, DEV_BUTTON_HEIGHT));

    //        //Widgets.EndScrollView();
    //    }

    //    private List<FloatMenuOption> GenerateMateriaOptions(MateriaSlot slot)
    //    {
    //        List<FloatMenuOption> options = new List<FloatMenuOption>();

    //        if (Prefs.DevMode)
    //        {
    //            foreach (MateriaDef materiaDef in DefDatabase<MateriaDef>.AllDefs)
    //            {
    //                var cacheMatdef = materiaDef;
    //                if (slot.CanAccept(cacheMatdef))
    //                {
    //                    options.Add(new FloatMenuOption(cacheMatdef.label, () =>
    //                    {          
    //                        var cacheSlot = slot;
    //                        materiaComp.EquipMateria(cacheMatdef, cacheSlot);
    //                    }));
    //                }
    //            }
    //        }
    //        else
    //        {
    //            foreach (Thing thing in usingPawn.EquippedWornOrInventoryThings)
    //            {
    //                if (thing.def is MateriaDef materiaDef && slot.CanAccept(materiaDef))
    //                {
    //                    var cacheMatdef = materiaDef;
    //                    options.Add(new FloatMenuOption(materiaDef.GetColouredLabel() + $" Level {materiaDef.slotLevel}", () =>
    //                    {
    //                        var cacheSlot = slot;
    //                        materiaComp.EquipMateria(cacheMatdef, cacheSlot);
    //                        usingPawn.inventory.RemoveCount(thing.def, 1);
    //                    }));
    //                }
    //            }
    //        }

    //        if (options.Count == 0)
    //        {
    //            options.Add(new FloatMenuOption("No compatible materia available", null));
    //        }

    //        return options;
    //    }

    //}


    public class DisplaySettings
    {
        public float margin = 4f;
        public float slotSize = 65f;
        public float topMargin = 15f;
        public float labelHeight = 40f;
        public float titleHeight = 35f;
        public float slotPadding = 10f;

        public float circleRadiusMultiplier = 1.2f;
    }

    [StaticConstructorOnStartup]
    public class Window_MateriaSelection : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        private readonly Thing parentThing;
        private readonly Comp_MateriaComp materiaComp;
        private readonly Pawn usingPawn;
        private Vector2 currentWindowSize = new Vector2(600f, 400f);
        private bool windowSizeInitialized = false;

        // Public method to force size recalculation
        public void RequestSizeRecalculation()
        {
            windowSizeInitialized = false;
        }

        // Removed constants in favor of DisplaySettings
        private const float DEV_BUTTON_HEIGHT = 30f;
        private const float DEV_BUTTON_WIDTH = 100f;
        private const float MIN_WINDOW_WIDTH = 300f;
        private const float MIN_WINDOW_HEIGHT = 200f;
        private const float WINDOW_MARGIN = 50f;

        private Color TITLE_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        private Color FOOTER_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        private Color SLOT_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);

        public override Vector2 InitialSize => currentWindowSize;

        protected Texture2D SlotTex = ContentFinder<Texture2D>.Get("UI/RuneSlot");
        protected Texture2D SlotConnectionTex = ContentFinder<Texture2D>.Get("UI/connection_lineglow");
        private static readonly Texture2D LockedTex = ContentFinder<Texture2D>.Get("UI/Lock_Closed");
        private static readonly Texture2D UnlockedTex = ContentFinder<Texture2D>.Get("UI/Lock_Open");
        private MateriaDisplayWorker displayWorker;

        public DisplaySettings DisplaySettings = new DisplaySettings();

        public Window_MateriaSelection(Thing thing, Pawn pawn, Comp_MateriaComp comp, Type preferredDisplayName = null)
        {
            this.parentThing = thing;
            this.materiaComp = comp;
            this.doCloseX = true;
            this.doCloseButton = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
            this.usingPawn = pawn;

            if (preferredDisplayName != null)
            {
                this.displayWorker = MateriaDisplayFactory.GetDisplay(preferredDisplayName);
            }
            else
            {
                this.displayWorker = MateriaDisplayFactory.GetDisplay(typeof(HorizontalMateriaDisplay));
            }
        }

        private void DrawTitle(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect titleRect = new Rect(0f, 0f, inRect.width, DisplaySettings.titleHeight);
            GUI.DrawTexture(titleRect, SolidColorMaterials.NewSolidColorTexture(TITLE_COLOR));

            string titleText = $"Materia Slots - {parentThing.Label}";
            Widgets.LabelFit(titleRect, titleText);
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

                // Settings button
                Rect settingsButtonRect = new Rect(
                    inRect.width - DEV_BUTTON_WIDTH - 10f - DEV_BUTTON_WIDTH - 10f,
                    5f,
                    DEV_BUTTON_WIDTH,
                    DEV_BUTTON_HEIGHT
                );

                if (Widgets.ButtonText(settingsButtonRect, "UI Settings"))
                {
                    Find.WindowStack.Add(new Window_DisplaySettings(this));
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

            float lockSize = 16f;
            Rect lockRect = new Rect(
                slotRect.x + slotRect.width - lockSize,
                slotRect.y,
                lockSize,
                lockSize
            );

            if (Prefs.DevMode)
            {
                if (Widgets.ButtonImage(lockRect, slot.IsSlotLocked ? LockedTex : UnlockedTex))
                {
                    slot.SetLockStatus(!slot.IsSlotLocked);
                }

                if (Mouse.IsOver(lockRect))
                {
                    TooltipHandler.TipRegion(lockRect,
                        slot.IsSlotLocked ? "Click to unlock (Dev)" : "Click to lock (Dev)");
                }
            }
            else
            {
                if (slot.IsSlotLocked)
                {
                    GUI.DrawTexture(lockRect, LockedTex);
                }
                else
                {
                    GUI.DrawTexture(lockRect, UnlockedTex);
                }

                if (Mouse.IsOver(lockRect))
                {
                    TooltipHandler.TipRegion(lockRect,
                        slot.IsSlotLocked ? "Locked" : "Unlocked");
                }
            }

            GUI.color = slotTypeColor;
            GUI.DrawTexture(slotRect, SlotTex);
            GUI.color = Color.white;

            Rect innerRect = slotRect.ContractedBy(DisplaySettings.margin);

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
                DisplaySettings.labelHeight
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

            if (slot.IsOccupied)
            {
                Widgets.Label(footerRect, $"{slot.SlottedMateria.def.LabelCap}");
            }
            else
            {
                Widgets.Label(footerRect, slot.SlotType != null ? slot.SlotType.label + $" L:{slot.SlotLevel}" : $"Any L:{slot.SlotLevel}");
            }

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

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && !slot.IsSlotLocked)
                    {
                        materiaComp.UnequipMateria(slot);
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

        public override void DoWindowContents(Rect inRect)
        {
            DrawTitle(inRect);

            float totalHeight = DisplaySettings.titleHeight + DisplaySettings.topMargin;
            var slots = materiaComp.MateriaSlots;

            Rect outRect = new Rect(0f, DisplaySettings.titleHeight, inRect.width, inRect.height - DisplaySettings.titleHeight);

            // Use display worker to position slots
            Dictionary<MateriaSlot, Rect> slotPositions = displayWorker.PositionSlots(this, slots, outRect, DisplaySettings.slotSize, DisplaySettings.slotPadding);

            // Calculate optimal window size based on slot positions
            if (!windowSizeInitialized || ShouldResizeWindow(slotPositions))
            {
                Vector2 optimalSize = CalculateOptimalWindowSize(slotPositions);
                if (optimalSize != currentWindowSize)
                {
                    ResizeWindow(optimalSize);
                }
                windowSizeInitialized = true;
            }

            // Draw each slot at its positioned location
            foreach (var slotEntry in slotPositions)
            {
                MateriaSlot slot = slotEntry.Key;
                Rect slotRect = slotEntry.Value;

                // Let the display worker handle custom drawing if implemented
                DrawMateriaSlot(slotRect, slot);
            }

            // Draw custom toolbar elements if any
            displayWorker.DrawToolBar(new Rect(0, outRect.height - DEV_BUTTON_HEIGHT - 10f, outRect.width, DEV_BUTTON_HEIGHT));
        }

        private bool ShouldResizeWindow(Dictionary<MateriaSlot, Rect> slotPositions)
        {
            // Resize when settings change or slots are added/removed
            if (slotPositions.Count != materiaComp.MateriaSlots.Count)
                return true;

            // Check if any slot is outside the current window bounds
            foreach (var slotRect in slotPositions.Values)
            {
                if (slotRect.xMax + DisplaySettings.slotPadding > windowRect.width - WINDOW_MARGIN ||
                    slotRect.yMax + DisplaySettings.labelHeight + DisplaySettings.slotPadding > windowRect.height - WINDOW_MARGIN)
                {
                    return true;
                }
            }

            return false;
        }

        private Vector2 CalculateOptimalWindowSize(Dictionary<MateriaSlot, Rect> slotPositions)
        {
            if (slotPositions.Count == 0)
                return new Vector2(MIN_WINDOW_WIDTH, MIN_WINDOW_HEIGHT);

            float maxX = 0f;
            float maxY = 0f;

            // Find the rightmost and bottommost edges of all slots
            foreach (var slotRect in slotPositions.Values)
            {
                maxX = Mathf.Max(maxX, slotRect.xMax);
                maxY = Mathf.Max(maxY, slotRect.yMax);
            }

            // Add space for the label height, padding, and margin
            maxY += DisplaySettings.labelHeight + DisplaySettings.slotPadding + WINDOW_MARGIN;
            maxX += DisplaySettings.slotPadding + WINDOW_MARGIN;

            // Account for title height
            maxY += DisplaySettings.titleHeight;

            // Add space for toolbar
            maxY += DEV_BUTTON_HEIGHT + 20f;

            // Ensure minimum dimensions
            maxX = Mathf.Max(maxX, MIN_WINDOW_WIDTH);
            maxY = Mathf.Max(maxY, MIN_WINDOW_HEIGHT);

            return new Vector2(maxX, maxY);
        }

        private void ResizeWindow(Vector2 newSize)
        {
            currentWindowSize = newSize;

            // Update the window rectangle while keeping the window centered
            float oldX = windowRect.x;
            float oldY = windowRect.y;
            float oldWidth = windowRect.width;
            float oldHeight = windowRect.height;

            float newX = oldX - (newSize.x - oldWidth) / 2f;
            float newY = oldY - (newSize.y - oldHeight) / 2f;

            // Keep window on screen
            newX = Mathf.Clamp(newX, 0, UI.screenWidth - newSize.x);
            newY = Mathf.Clamp(newY, 0, UI.screenHeight - newSize.y);

            windowRect = new Rect(newX, newY, newSize.x, newSize.y);
        }

        private List<FloatMenuOption> GenerateMateriaOptions(MateriaSlot slot)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            if (Prefs.DevMode)
            {
                foreach (MateriaDef materiaDef in DefDatabase<MateriaDef>.AllDefs)
                {
                    var cacheMatdef = materiaDef;
                    if (slot.CanAccept(cacheMatdef))
                    {
                        options.Add(new FloatMenuOption(cacheMatdef.label, () =>
                        {
                            var cacheSlot = slot;
                            materiaComp.EquipMateria(cacheMatdef, cacheSlot);
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
                        var cacheMatdef = materiaDef;
                        options.Add(new FloatMenuOption(materiaDef.GetColouredLabel() + $" Level {materiaDef.slotLevel}", () =>
                        {
                            var cacheSlot = slot;
                            materiaComp.EquipMateria(cacheMatdef, cacheSlot);
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
    }
}
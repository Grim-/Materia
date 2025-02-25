using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Materia
{
    public class DisplaySettings
    {
        public float margin = 4f;
        public Vector2 slotSize = new Vector2(83f, 83f);
        public Vector2 slotPadding = new Vector2(25f, 50f);

        public float topMargin = 15f;
        public float labelHeight = 40f;
        public float titleHeight = 35f;


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
        private const float DEV_BUTTON_HEIGHT = 30f;
        private const float DEV_BUTTON_WIDTH = 30f;
        private const float MIN_WINDOW_WIDTH = 300f;
        private const float MIN_WINDOW_HEIGHT = 200f;
        private const float WINDOW_MARGIN = 50f;

        private bool AutoSizingEnabled = true;

        private Color TITLE_COLOR = new Color(0.3f, 0.3f, 0.3f, 0f);
        public override Vector2 InitialSize => currentWindowSize;

        private static readonly Texture2D SlotTex = ContentFinder<Texture2D>.Get("UI/RuneSlot");
        private static readonly Texture2D SlotConnectionTex = ContentFinder<Texture2D>.Get("UI/connection_lineglow");
        private static readonly Texture2D LockedTex = ContentFinder<Texture2D>.Get("UI/Lock_Closed");
        private static readonly Texture2D UnlockedTex = ContentFinder<Texture2D>.Get("UI/Lock_Open");
        private static readonly Texture2D WindowBG = ContentFinder<Texture2D>.Get("UI/MateriaWindowBG");
       // private static readonly Texture2D FooterBG = SolidColorMaterials.NewSolidColorTexture(footerColor);
        private MateriaDisplayWorker displayWorker;

        public DisplaySettings DisplaySettings = new DisplaySettings();

        public Window_MateriaSelection(Thing thing, Pawn pawn, Comp_MateriaComp comp, Type preferredDisplayName = null)
        {
            this.parentThing = thing;
            this.materiaComp = comp;
            this.doCloseX = true;
            this.doCloseButton = false;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
            this.usingPawn = pawn;
            this.doWindowBackground = false;
            this.drawShadow = false;
            if (preferredDisplayName != null)
            {
                this.displayWorker = MateriaDisplayFactory.GetDisplay(preferredDisplayName);
            }
            else
            {
                this.displayWorker = MateriaDisplayFactory.GetDisplay(typeof(CircularMateriaDisplay));
            }
        }

        public void RequestSizeRecalculation()
        {
            windowSizeInitialized = false;
        }

        private void AddMateriaSlot(MateriaTypeDef slotType)
        {
            if (materiaComp != null)
            {
                materiaComp.AddMateriaSlot(slotType);
            }
        }


        public override void DoWindowContents(Rect inRect)
        {
            GUI.DrawTexture(inRect, WindowBG);
            DrawTitle(inRect);

            float totalHeight = DisplaySettings.titleHeight + DisplaySettings.topMargin;
            var slots = materiaComp.MateriaSlots;
            Rect outRect = new Rect(0f, DisplaySettings.titleHeight, inRect.width, inRect.height - DisplaySettings.titleHeight - DisplaySettings.topMargin);

            // Use display worker to position slots
            Dictionary<MateriaSlot, Rect> slotPositions = displayWorker.PositionSlots(this, slots, outRect, DisplaySettings.slotSize, DisplaySettings.slotPadding);

   
            DrawMateriaSlots(slotPositions);
            displayWorker.DrawToolBar(new Rect(0, outRect.height - DEV_BUTTON_HEIGHT - 10f, outRect.width, DEV_BUTTON_HEIGHT));
        }
        private void DrawTitle(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect titleRect = new Rect(0f, 0f, inRect.width, DisplaySettings.titleHeight);
            Widgets.DrawBoxSolid(titleRect, Color.white * 0.3f);

            string titleText = $"{parentThing.LabelShort}";
            Widgets.LabelFit(titleRect, titleText);
            Text.Anchor = TextAnchor.UpperLeft;

            DrawDevButtons(inRect);
        }

        private void DrawMateriaSlots(Dictionary<MateriaSlot, Rect> slotPositions)
        {
            HandleAutoResizing(slotPositions);

            foreach (var slotEntry in slotPositions)
            {
                MateriaSlot slot = slotEntry.Key;
                Rect slotRect = slotEntry.Value;
                DrawMateriaSlot(slotRect, slot);
            }
        }
        private void HandleAutoResizing(Dictionary<MateriaSlot, Rect> slotPositions)
        {

            if (AutoSizingEnabled)
            {
                if (!windowSizeInitialized || ShouldResizeWindow(slotPositions))
                {
                    Vector2 optimalSize = CalculateOptimalWindowSize(slotPositions);
                    if (optimalSize != currentWindowSize)
                    {
                        ResizeWindow(optimalSize);
                    }
                    windowSizeInitialized = true;
                }

            }
        }
        private void DrawDevButtons(Rect inRect)
        {
            if (Prefs.DevMode)
            {
                Rect buttonRect = new Rect(
                  inRect.width - DEV_BUTTON_WIDTH - 10f,
                      5f,
                      DEV_BUTTON_WIDTH,
                      DEV_BUTTON_HEIGHT
                  );
                Text.Font = GameFont.Small;
                if (Widgets.ButtonImage(buttonRect, TexButton.Add))
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

                if (Widgets.ButtonImage(settingsButtonRect, TexButton.OpenInspectSettings))
                {
                    Find.WindowStack.Add(new Window_DisplaySettings(this));
                }
                Text.Font = GameFont.Medium;
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
            Color slotTypeColor = Color.white;
            if (slot.IsSlotLocked)
                slotTypeColor = Color.grey;
            else
                slotTypeColor = slot.SlotType?.materiaTextColor ?? Color.white;

            float lockSize = 16f;
            Rect lockRect = new Rect(
                slotRect.x + slotRect.width - lockSize,
                slotRect.y,
                lockSize,
                lockSize
            );

           
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

            float footerWidth = slotRect.width + 20f;
            float footerX = slotRect.x - 10f;

            Rect footerRect = new Rect(
                footerX,
                slotRect.y + slotRect.height + 9f,
                footerWidth,
                DisplaySettings.labelHeight
            );

            Color footerColor = new Color(
                slotTypeColor.r,
                slotTypeColor.g,
                slotTypeColor.b,
                0.5f
            );

            HandleSlotClick(slot, slotRect, slotTypeColor);
            Widgets.DrawBoxSolidWithOutline(footerRect, footerColor  * 0.3f, footerColor);
            DrawSlotLabel(slot, footerRect);
            DrawSlotLock(slot, lockRect, slotTypeColor);
        }

        private void DrawSlotLabel(MateriaSlot slot, Rect footerRect)
        {
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = Color.white;

            if (slot.IsOccupied)
            {
                Widgets.Label(footerRect, $"{slot.SlottedMateria.def.LabelCap}");
            }
            else
            {
                Widgets.Label(footerRect, $"Empty - Level {slot.SlotLevel}");
            }

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }
        private void DrawSlotLock(MateriaSlot slot, Rect lockRect, Color slotTypeColor)
        {
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
        }
        private void HandleSlotClick(MateriaSlot slot, Rect slotRect, Color slotTypeColor)
        {
            if (Mouse.IsOver(slotRect))
            {
                Color highlightColor = new Color(
                    slotTypeColor.r,
                    slotTypeColor.g,
                    slotTypeColor.b,
                    0.4f
                );
                Widgets.DrawBoxSolidWithOutline(slotRect, Color.clear, highlightColor);

                if (slot.IsOccupied)
                {
                    string lockedStatus = slot.IsSlotLocked ? "Locked" : "";
                    string desc = slot.SlottedMateria.def.description;
                    TooltipHandler.TipRegion(slotRect,
                        $"{slot.SlottedMateria.def.GetColouredLabel()}\n\n{desc}\n\nRight-click to remove\n\n{lockedStatus}");

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && !slot.IsSlotLocked)
                    {
                        materiaComp.UnequipMateria(slot);
                        Event.current.Use();
                    }
                }
                else
                {
                    TooltipHandler.TipRegion(slotRect,
                        $"Empty {slot.SlotType?.label ?? "materia"} slot\nClick to equip materia from inventory.");

                    if (Widgets.ButtonInvisible(slotRect))
                    {
                        Find.WindowStack.Add(new FloatMenu(MateriaUtil.GenerateMateriaOptions(materiaComp, usingPawn, slot)));
                    }
                }
            }
        }

        private bool ShouldResizeWindow(Dictionary<MateriaSlot, Rect> slotPositions)
        {
            if (slotPositions.Count != materiaComp.MateriaSlots.Count)
                return true;

            foreach (var slotRect in slotPositions.Values)
            {
                if (slotRect.xMax + DisplaySettings.slotPadding.x > windowRect.width - WINDOW_MARGIN ||
                    slotRect.yMax + DisplaySettings.labelHeight + DisplaySettings.slotPadding.y > windowRect.height - WINDOW_MARGIN)
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

            foreach (var slotRect in slotPositions.Values)
            {
                maxX = Mathf.Max(maxX, slotRect.xMax);
                maxY = Mathf.Max(maxY, slotRect.yMax);
            }

            maxY += DisplaySettings.labelHeight + DisplaySettings.slotPadding.y + WINDOW_MARGIN;
            maxX += DisplaySettings.slotPadding.x + WINDOW_MARGIN;       
            maxY += DEV_BUTTON_HEIGHT;

            maxX = Mathf.Max(maxX, MIN_WINDOW_WIDTH);
            maxY = Mathf.Max(maxY, MIN_WINDOW_HEIGHT);

            return new Vector2(maxX, maxY);
        }
        private void ResizeWindow(Vector2 newSize)
        {
            currentWindowSize = newSize;
            float oldX = windowRect.x;
            float oldY = windowRect.y;
            float oldWidth = windowRect.width;
            float oldHeight = windowRect.height;

            float newX = oldX - (newSize.x - oldWidth) / 2f;
            float newY = oldY - (newSize.y - oldHeight) / 2f;

            newX = Mathf.Clamp(newX, 0, UI.screenWidth - newSize.x);
            newY = Mathf.Clamp(newY, 0, UI.screenHeight - newSize.y);
            windowRect = new Rect(newX, newY, newSize.x, newSize.y);
        }
    }
}
using UnityEngine;
using Verse;

namespace Materia
{
    public class Window_DisplaySettings : Window
    {
        private readonly Window_MateriaSelection parentWindow;
        private static readonly Vector2 WinSize = new Vector2(550f, 450f);
        private DisplaySettings tempSettings;

        private const float LABEL_WIDTH = 120f;
        private const float CONTROL_WIDTH = 200f;
        private const float ROW_HEIGHT = 30f;
        private const float ROW_SPACING = 10f;
        private const float PADDING = 20f;
        private const float TITLE_HEIGHT = 35f;
        private readonly Color TITLE_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);

        public override Vector2 InitialSize => WinSize;

        private Vector2 scrollPosition;

        public Window_DisplaySettings(Window_MateriaSelection parent)
        {
            this.parentWindow = parent;
            this.doCloseX = true;
            this.doCloseButton = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;

            // Create a copy of the parent's DisplaySettings to work with
            this.tempSettings = new DisplaySettings
            {
                margin = parent.DisplaySettings.margin,
                slotSize = parent.DisplaySettings.slotSize,
                topMargin = parent.DisplaySettings.topMargin,
                labelHeight = parent.DisplaySettings.labelHeight,
                titleHeight = parent.DisplaySettings.titleHeight,
                slotPadding = parent.DisplaySettings.slotPadding
            };
        }

        private void DrawTitle(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect titleRect = new Rect(0f, 0f, inRect.width, TITLE_HEIGHT);
            GUI.DrawTexture(titleRect, SolidColorMaterials.NewSolidColorTexture(TITLE_COLOR));

            Widgets.LabelFit(titleRect, "UI Settings");

            Text.Anchor = TextAnchor.UpperLeft;
        }

        public override void DoWindowContents(Rect inRect)
        {
            DrawTitle(inRect);

            float currentY = TITLE_HEIGHT + PADDING;


            Rect minusTitleRect = new Rect(inRect.x, currentY, inRect.width - Margin, inRect.height);
            Rect viewRect = new Rect(0, currentY, inRect.width - Margin, 1000f);

            Widgets.BeginScrollView(minusTitleRect, ref scrollPosition, viewRect);

            // Display each setting with a label and a slider
            DrawSettingRow(inRect, "Slot Size X", ref tempSettings.slotSize.x, 30f, 150f, ref currentY);
            DrawSettingRow(inRect, "Slot Size Y", ref tempSettings.slotSize.y, 30f, 150f, ref currentY);
            DrawSettingRow(inRect, "Slot Padding X", ref tempSettings.slotPadding.x, 0f, 30f, ref currentY);
            DrawSettingRow(inRect, "Slot Padding Y", ref tempSettings.slotPadding.y, 0f, 30f, ref currentY);

            DrawSettingRow(inRect, "Margin", ref tempSettings.margin, 0f, 10f, ref currentY);
            DrawSettingRow(inRect, "Top Margin", ref tempSettings.topMargin, 0f, 50f, ref currentY);
            DrawSettingRow(inRect, "Label Height", ref tempSettings.labelHeight, 20f, 60f, ref currentY);
            DrawSettingRow(inRect, "Title Height", ref tempSettings.titleHeight, 20f, 60f, ref currentY);

            currentY += ROW_SPACING;

            Widgets.EndScrollView();

            Rect buttonsRect = new Rect(PADDING, currentY, inRect.width - (PADDING * 2), ROW_HEIGHT);

            // Save button
            Rect saveButtonRect = new Rect(
                buttonsRect.x,
                buttonsRect.y,
                (buttonsRect.width / 2) - 5f,
                buttonsRect.height
            );

            if (Widgets.ButtonText(saveButtonRect, "Save Settings"))
            {
                ApplySettings();
                Close();
            }

            // Cancel button
            Rect cancelButtonRect = new Rect(
                saveButtonRect.xMax + 10f,
                buttonsRect.y,
                (buttonsRect.width / 2) - 5f,
                buttonsRect.height
            );

            if (Widgets.ButtonText(cancelButtonRect, "Cancel"))
            {
                Close();
            }

            // Reset to defaults button
            Rect resetButtonRect = new Rect(
                PADDING,
                buttonsRect.yMax + ROW_SPACING,
                inRect.width - (PADDING * 2),
                ROW_HEIGHT
            );

            if (Widgets.ButtonText(resetButtonRect, "Reset to Defaults"))
            {
                ResetToDefaults();
            }
        }

        private void DrawSettingRow(Rect inRect, string label, ref float value, float min, float max, ref float currentY)
        {
            Rect rowRect = new Rect(PADDING, currentY, inRect.width - (PADDING * 2), ROW_HEIGHT);

            // Label
            Rect labelRect = new Rect(rowRect.x, rowRect.y, LABEL_WIDTH, rowRect.height);
            Widgets.Label(labelRect, label);

            // Slider
            Rect sliderRect = new Rect(labelRect.xMax + 10f, rowRect.y, CONTROL_WIDTH - 50f, rowRect.height);
            value = Widgets.HorizontalSlider(sliderRect, value, min, max, false);

            // Value display
            Rect valueRect = new Rect(sliderRect.xMax + 5f, rowRect.y, 45f, rowRect.height);
            Widgets.Label(valueRect, value.ToString("F1"));

            currentY += ROW_HEIGHT + ROW_SPACING;
        }

        private void ApplySettings()
        {
            // Check if any size-affecting settings have changed
            bool sizeChanged = parentWindow.DisplaySettings.slotSize != tempSettings.slotSize ||
                               parentWindow.DisplaySettings.slotPadding != tempSettings.slotPadding ||
                               parentWindow.DisplaySettings.titleHeight != tempSettings.titleHeight ||
                               parentWindow.DisplaySettings.labelHeight != tempSettings.labelHeight;

            // Update the parent window's settings with our temporary ones
            parentWindow.DisplaySettings.margin = tempSettings.margin;
            parentWindow.DisplaySettings.slotSize = tempSettings.slotSize;
            parentWindow.DisplaySettings.topMargin = tempSettings.topMargin;
            parentWindow.DisplaySettings.labelHeight = tempSettings.labelHeight;
            parentWindow.DisplaySettings.titleHeight = tempSettings.titleHeight;
            parentWindow.DisplaySettings.slotPadding = tempSettings.slotPadding;

            // Force a resize on the next frame if size-affecting settings changed
            if (sizeChanged)
            {
                // Use the public method to request size recalculation
                parentWindow.RequestSizeRecalculation();
            }
        }

        private void ResetToDefaults()
        {
            // Reset to default values
            tempSettings = new DisplaySettings();
        }
    }
}
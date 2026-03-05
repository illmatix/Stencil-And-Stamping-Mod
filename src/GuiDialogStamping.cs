using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace StencilAndStamping
{
    public class GuiDialogStamping : GuiDialog
    {
        private IPlayer player;
        private ItemSlot stampSlot;
        private BlockSelection blockSel;

        private int gridSize;
        private bool[] cellPattern;
        private string[] cellColors;
        private string[] availableColors;
        private int selectedColorIndex = 0;

        public override string ToggleKeyCombinationCode => null;

        public GuiDialogStamping(ICoreClientAPI capi, IPlayer player, ItemSlot stampSlot, BlockSelection blockSel)
            : base(capi)
        {
            this.player = player;
            this.stampSlot = stampSlot;
            this.blockSel = blockSel;

            var stamp = stampSlot.Itemstack.Item as ItemStamp;
            gridSize = stamp.GetGridSize(stampSlot.Itemstack);
            cellPattern = stamp.GetCellPattern(stampSlot.Itemstack) ?? new bool[gridSize * gridSize];
            cellColors = new string[gridSize * gridSize];

            // Find available ink colors from player inventory
            availableColors = FindAvailableInkColors();

            ComposeDialog();
        }

        private string[] FindAvailableInkColors()
        {
            var colors = new List<string>();
            var hotbar = player.InventoryManager.GetHotbarInventory();
            if (hotbar != null)
            {
                foreach (var slot in hotbar)
                {
                    if (slot?.Itemstack?.Item is ItemInkPot inkPot)
                    {
                        string color = inkPot.GetInkColor(slot.Itemstack);
                        if (!colors.Contains(color))
                            colors.Add(color);
                    }
                }
            }
            return colors.ToArray();
        }

        private void ComposeDialog()
        {
            int cellPx = 40;
            int gridPx = gridSize * cellPx;
            int paletteWidth = 120;
            int dialogWidth = gridPx + paletteWidth + 50;
            int dialogHeight = Math.Max(gridPx + 80, 220);

            ElementBounds bgBounds = ElementBounds.Fixed(0, 0, dialogWidth, dialogHeight);
            ElementBounds dialogBounds = bgBounds.ForkBoundingParent(10, 40, 10, 10);

            var composer = capi.Gui
                .CreateCompo("stamping-design", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("Stamp Colors", OnTitleBarClose)
                .BeginChildElements(bgBounds);

            // --- Grid cells (left side) ---
            double gridStartX = 15;
            double gridStartY = 10;

            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    int idx = row * gridSize + col;
                    double x = gridStartX + col * cellPx;
                    double y = gridStartY + row * cellPx;

                    ElementBounds btnBounds = ElementBounds.Fixed(x, y, cellPx - 2, cellPx - 2);
                    string key = "cell-" + idx;

                    if (cellPattern[idx])
                    {
                        string label = string.IsNullOrEmpty(cellColors[idx]) ? "." : cellColors[idx].Substring(0, Math.Min(3, cellColors[idx].Length));
                        composer.AddSmallButton(
                            label,
                            () => OnCellClicked(idx),
                            btnBounds,
                            EnumButtonStyle.Normal,
                            key
                        );
                    }
                    else
                    {
                        // Inactive cell — show as static text
                        composer.AddStaticText(
                            "",
                            CairoFont.WhiteSmallText(),
                            btnBounds
                        );
                    }
                }
            }

            // --- Color palette (right side) ---
            double palX = gridStartX + gridPx + 15;
            double palY = gridStartY;

            composer.AddStaticText(
                "Ink Colors:",
                CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(palX, palY, 110, 20)
            );
            palY += 24;

            if (availableColors.Length == 0)
            {
                composer.AddStaticText(
                    "No ink in\nhotbar!",
                    CairoFont.WhiteSmallText(),
                    ElementBounds.Fixed(palX, palY, 110, 40)
                );
            }
            else
            {
                for (int i = 0; i < availableColors.Length; i++)
                {
                    int capturedI = i;
                    string prefix = selectedColorIndex == i ? "> " : "  ";
                    composer.AddSmallButton(
                        prefix + availableColors[i],
                        () => OnColorSelected(capturedI),
                        ElementBounds.Fixed(palX, palY + i * 28, 110, 24),
                        EnumButtonStyle.Normal,
                        "color-" + i
                    );
                }
            }

            // Clear + confirm at bottom
            double bottomY = Math.Max(palY + availableColors.Length * 28 + 10, gridStartY + gridPx + 10);

            composer.AddSmallButton(
                "Clear All",
                OnClearAll,
                ElementBounds.Fixed(palX, bottomY, 110, 24)
            );
            composer.AddSmallButton(
                "Stamp!",
                OnConfirm,
                ElementBounds.Fixed(palX, bottomY + 32, 110, 30),
                EnumButtonStyle.Normal,
                "confirm"
            );

            composer.EndChildElements();
            SingleComposer = composer.Compose();
        }

        private bool OnCellClicked(int idx)
        {
            if (!cellPattern[idx]) return true;

            if (availableColors.Length == 0) return true;

            // Toggle: if already this color, clear it; otherwise set it
            string currentColor = cellColors[idx];
            string selectedColor = availableColors[selectedColorIndex];

            if (currentColor == selectedColor)
                cellColors[idx] = "";
            else
                cellColors[idx] = selectedColor;

            ComposeDialog();
            return true;
        }

        private bool OnColorSelected(int index)
        {
            selectedColorIndex = index;
            ComposeDialog();
            return true;
        }

        private bool OnClearAll()
        {
            for (int i = 0; i < cellColors.Length; i++)
                cellColors[i] = "";
            ComposeDialog();
            return true;
        }

        private bool OnConfirm()
        {
            // Check at least one cell has color
            bool anyColored = false;
            foreach (string c in cellColors)
            {
                if (!string.IsNullOrEmpty(c)) { anyColored = true; break; }
            }

            if (!anyColored)
            {
                capi.TriggerIngameError(this, "nocolors", "Assign at least one ink color.");
                return true;
            }

            // Send to server
            BlockPos overlayPos = blockSel.Position.AddCopy(blockSel.Face);
            StencilAndStampingMod.ClientChannel?.SendPacket(new StampDesignPacket
            {
                TargetPos = overlayPos,
                Face = blockSel.Face.Code,
                GridSize = gridSize,
                CellColors = cellColors,
                CellBorders = (stampSlot.Itemstack.Item as ItemStamp)?.HasCellBorders(stampSlot.Itemstack) ?? false,
                EdgeBorder = (stampSlot.Itemstack.Item as ItemStamp)?.HasEdgeBorder(stampSlot.Itemstack) ?? false
            });

            TryClose();
            return true;
        }

        private void OnTitleBarClose()
        {
            TryClose();
        }
    }
}

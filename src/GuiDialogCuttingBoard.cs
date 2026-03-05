using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace StencilAndStamping
{
    public class GuiDialogCuttingBoard : GuiDialog
    {
        private BlockPos boardPos;
        private IPlayer player;

        private int gridSize = 3;
        private bool[] cellPattern;
        private bool cellBorders = false;
        private bool edgeBorder = false;

        public override string ToggleKeyCombinationCode => null;

        public GuiDialogCuttingBoard(ICoreClientAPI capi, BlockPos boardPos, IPlayer player)
            : base(capi)
        {
            this.boardPos = boardPos;
            this.player = player;
            cellPattern = new bool[gridSize * gridSize];
            ComposeDialog();
        }

        private void ComposeDialog()
        {
            int cellCount = gridSize * gridSize;
            if (cellPattern == null || cellPattern.Length != cellCount)
                cellPattern = new bool[cellCount];

            int cellPx = 36;
            int gridPx = gridSize * cellPx;
            int controlsWidth = 200;
            int dialogWidth = gridPx + controlsWidth + 40;
            int dialogHeight = Math.Max(gridPx + 60, 260);

            ElementBounds bgBounds = ElementBounds.Fixed(0, 0, dialogWidth, dialogHeight);
            ElementBounds dialogBounds = bgBounds.ForkBoundingParent(10, 40, 10, 10);

            var composer = capi.Gui
                .CreateCompo("cuttingboard-design", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("Stencil Design", OnTitleBarClose)
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
                    bool active = cellPattern[idx];

                    composer.AddToggleButton(
                        active ? "X" : "",
                        CairoFont.WhiteSmallText(),
                        OnCellToggle,
                        btnBounds,
                        key
                    );
                }
            }

            // --- Controls (right side) ---
            double ctrlX = gridStartX + gridPx + 15;
            double ctrlY = gridStartY;

            // Grid size label
            composer.AddStaticText(
                "Grid Size:",
                CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(ctrlX, ctrlY, 120, 20)
            );
            ctrlY += 25;

            // Grid size buttons
            for (int size = 2; size <= 5; size++)
            {
                int capturedSize = size;
                string label = gridSize == size ? "[" + size + "x" + size + "]" : size + "x" + size;

                composer.AddSmallButton(
                    label,
                    () => OnGridSizeSelected(capturedSize),
                    ElementBounds.Fixed(ctrlX + (size - 2) * 42, ctrlY, 38, 24),
                    EnumButtonStyle.Normal,
                    "size-" + size
                );
            }
            ctrlY += 40;

            // Cell borders toggle
            composer.AddToggleButton(
                "Cell Borders",
                CairoFont.WhiteSmallText(),
                OnCellBordersToggle,
                ElementBounds.Fixed(ctrlX, ctrlY, 160, 24),
                "cellborders"
            );
            ctrlY += 32;

            // Edge border toggle
            composer.AddToggleButton(
                "Edge Border",
                CairoFont.WhiteSmallText(),
                OnEdgeBorderToggle,
                ElementBounds.Fixed(ctrlX, ctrlY, 160, 24),
                "edgeborder"
            );
            ctrlY += 32;

            // Fill all / clear all
            composer.AddSmallButton(
                "Fill All",
                OnFillAll,
                ElementBounds.Fixed(ctrlX, ctrlY, 75, 24)
            );
            composer.AddSmallButton(
                "Clear",
                OnClearAll,
                ElementBounds.Fixed(ctrlX + 80, ctrlY, 75, 24)
            );
            ctrlY += 40;

            // Confirm button
            composer.AddSmallButton(
                "Cut Stencil",
                OnConfirm,
                ElementBounds.Fixed(ctrlX, ctrlY, 160, 30),
                EnumButtonStyle.Normal,
                "confirm"
            );

            composer.EndChildElements();
            SingleComposer = composer.Compose();

            // Set initial toggle states
            for (int i = 0; i < cellPattern.Length; i++)
            {
                SingleComposer.GetToggleButton("cell-" + i)?.SetValue(cellPattern[i]);
            }
            SingleComposer.GetToggleButton("cellborders")?.SetValue(cellBorders);
            SingleComposer.GetToggleButton("edgeborder")?.SetValue(edgeBorder);
        }

        private void OnCellToggle(bool state)
        {
            // Read all cell states from the toggle buttons
            for (int i = 0; i < cellPattern.Length; i++)
            {
                var btn = SingleComposer.GetToggleButton("cell-" + i);
                if (btn != null)
                    cellPattern[i] = btn.On;
            }
        }

        private bool OnGridSizeSelected(int newSize)
        {
            if (newSize == gridSize) return true;
            gridSize = newSize;
            cellPattern = new bool[gridSize * gridSize];
            ComposeDialog();
            return true;
        }

        private void OnCellBordersToggle(bool state)
        {
            cellBorders = state;
        }

        private void OnEdgeBorderToggle(bool state)
        {
            edgeBorder = state;
        }

        private bool OnFillAll()
        {
            for (int i = 0; i < cellPattern.Length; i++)
            {
                cellPattern[i] = true;
                SingleComposer.GetToggleButton("cell-" + i)?.SetValue(true);
            }
            return true;
        }

        private bool OnClearAll()
        {
            for (int i = 0; i < cellPattern.Length; i++)
            {
                cellPattern[i] = false;
                SingleComposer.GetToggleButton("cell-" + i)?.SetValue(false);
            }
            return true;
        }

        private bool OnConfirm()
        {
            // Read final cell states
            for (int i = 0; i < cellPattern.Length; i++)
            {
                var btn = SingleComposer.GetToggleButton("cell-" + i);
                if (btn != null)
                    cellPattern[i] = btn.On;
            }

            // Check that at least one cell is active
            bool anyFilled = false;
            foreach (bool cell in cellPattern)
            {
                if (cell) { anyFilled = true; break; }
            }

            if (!anyFilled)
            {
                capi.TriggerIngameError(this, "nocells", "Select at least one cell.");
                return true;
            }

            // Check player is still holding a blank stencil
            ItemSlot activeSlot = player.InventoryManager.ActiveHotbarSlot;
            if (activeSlot == null || activeSlot.Empty || activeSlot.Itemstack?.Item is not ItemStencil)
            {
                capi.TriggerIngameError(this, "nostencil", "Hold a blank stencil.");
                return true;
            }

            // Check hotbar has a knife
            bool hasKnife = false;
            var hotbar = player.InventoryManager.GetOwnInventory(GlobalConstants.hotBarInvClassName);
            if (hotbar != null)
            {
                foreach (ItemSlot slot in hotbar)
                {
                    if (slot == null || slot.Empty) continue;
                    string code = slot.Itemstack?.Collectible?.Code?.Path ?? "";
                    if (code.Contains("knife"))
                    {
                        hasKnife = true;
                        break;
                    }
                }
            }

            if (!hasKnife)
            {
                capi.TriggerIngameError(this, "noknife", "You need a knife in your hotbar.");
                return true;
            }

            // Send design to server via custom network message
            StencilAndStampingMod.ClientChannel?.SendPacket(new CuttingBoardDesignPacket
            {
                BoardPos = boardPos,
                GridSize = gridSize,
                CellPattern = cellPattern,
                CellBorders = cellBorders,
                EdgeBorder = edgeBorder
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

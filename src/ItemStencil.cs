using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace StencilAndStamping
{
    public class ItemStencil : Item
    {
        public int GetGridSize(ItemStack stack)
        {
            return stack.Attributes.GetInt("gridSize", 0);
        }

        public bool[] GetCellPattern(ItemStack stack)
        {
            byte[] bytes = stack.Attributes.GetBytes("cellPattern");
            if (bytes == null) return null;

            bool[] pattern = new bool[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
                pattern[i] = bytes[i] != 0;
            return pattern;
        }

        public bool HasCellBorders(ItemStack stack)
        {
            return stack.Attributes.GetBool("cellBorders", false);
        }

        public bool HasEdgeBorder(ItemStack stack)
        {
            return stack.Attributes.GetBool("edgeBorder", false);
        }

        public bool IsDesigned(ItemStack stack)
        {
            return GetGridSize(stack) > 0;
        }

        public static void ApplyDesign(ItemStack stack, int gridSize, bool[] cellPattern, bool cellBorders, bool edgeBorder)
        {
            stack.Attributes.SetInt("gridSize", gridSize);

            byte[] bytes = new byte[cellPattern.Length];
            for (int i = 0; i < cellPattern.Length; i++)
                bytes[i] = cellPattern[i] ? (byte)1 : (byte)0;
            stack.Attributes.SetBytes("cellPattern", bytes);

            stack.Attributes.SetBool("cellBorders", cellBorders);
            stack.Attributes.SetBool("edgeBorder", edgeBorder);
        }

        // --- Cached color layout (set after first use) ---

        public string[] GetCachedColors(ItemStack stack)
        {
            string raw = stack.Attributes.GetString("cachedColors");
            if (raw == null) return null;
            return raw.Split(',');
        }

        public void SetCachedColors(ItemStack stack, string[] colors)
        {
            stack.Attributes.SetString("cachedColors", string.Join(",", colors));
        }

        public bool HasCachedColors(ItemStack stack)
        {
            return stack.Attributes.HasAttribute("cachedColors");
        }

        public void ClearCachedColors(ItemStack stack)
        {
            stack.Attributes.RemoveAttribute("cachedColors");
        }

        // --- Interaction: apply stencil to a surface ---

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (blockSel == null) return;
            if (!IsDesigned(slot.Itemstack)) return;

            var world = byEntity.World;
            var player = (byEntity as EntityPlayer)?.Player;
            if (player == null) return;

            // Cutting board resets cached colors
            Block targetBlock = world.BlockAccessor.GetBlock(blockSel.Position);
            if (targetBlock is BlockCuttingBoard)
            {
                if (world.Side == EnumAppSide.Server && HasCachedColors(slot.Itemstack))
                {
                    ClearCachedColors(slot.Itemstack);
                    slot.MarkDirty();
                }
                handling = EnumHandHandling.PreventDefault;
                return;
            }

            if (!IsValidSurface(targetBlock)) return;

            BlockPos overlayPos = blockSel.Position.AddCopy(blockSel.Face);
            Block existingBlock = world.BlockAccessor.GetBlock(overlayPos);
            if (existingBlock is BlockStampedOverlay)
            {
                var be = world.BlockAccessor.GetBlockEntity(overlayPos) as BlockEntityStampedOverlay;
                if (be != null && !be.CanAddLayer())
                {
                    if (world.Side == EnumAppSide.Client)
                    {
                        (world.Api as ICoreClientAPI)?
                            .TriggerIngameError(this, "maxlayers", "Maximum layers reached on this face.");
                    }
                    handling = EnumHandHandling.PreventDefault;
                    return;
                }
            }
            else if (existingBlock.Id != 0)
            {
                return;
            }

            // Cached colors → re-apply directly
            if (HasCachedColors(slot.Itemstack))
            {
                if (world.Side == EnumAppSide.Server)
                {
                    string[] colors = GetCachedColors(slot.Itemstack);
                    ApplyStencil(world, player, slot, overlayPos, blockSel.Face, colors);
                }
                handling = EnumHandHandling.PreventDefault;
                return;
            }

            // No cached colors → open color assignment GUI
            if (world.Side == EnumAppSide.Client)
            {
                var capi = world.Api as ICoreClientAPI;
                var dialog = new GuiDialogStamping(capi, player, slot, blockSel);
                dialog.TryOpen();
            }

            handling = EnumHandHandling.PreventDefault;
        }

        public static bool IsValidSurface(Block block)
        {
            if (block == null) return false;
            var mat = block.BlockMaterial;
            return mat == EnumBlockMaterial.Stone
                || mat == EnumBlockMaterial.Ceramic
                || mat == EnumBlockMaterial.Wood
                || mat == EnumBlockMaterial.Soil;
        }

        public static void ApplyStencil(IWorldAccessor world, IPlayer player, ItemSlot stencilSlot,
            BlockPos overlayPos, BlockFacing face, string[] cellColors)
        {
            var stencil = stencilSlot.Itemstack.Item as ItemStencil;
            if (stencil == null) return;

            int gridSize = stencil.GetGridSize(stencilSlot.Itemstack);
            bool cellBorders = stencil.HasCellBorders(stencilSlot.Itemstack);
            bool edgeBorder = stencil.HasEdgeBorder(stencilSlot.Itemstack);

            int filledCells = 0;
            foreach (string c in cellColors)
            {
                if (!string.IsNullOrEmpty(c)) filledCells++;
            }
            if (filledCells == 0) return;

            Block existingBlock = world.BlockAccessor.GetBlock(overlayPos);
            BlockEntityStampedOverlay be;

            if (existingBlock is BlockStampedOverlay)
            {
                be = world.BlockAccessor.GetBlockEntity(overlayPos) as BlockEntityStampedOverlay;
                if (be == null || !be.CanAddLayer()) return;
            }
            else
            {
                Block overlayBlock = world.GetBlock(new AssetLocation("stencilandstamping:stampedoverlay"));
                if (overlayBlock == null) return;
                world.BlockAccessor.SetBlock(overlayBlock.Id, overlayPos);
                be = world.BlockAccessor.GetBlockEntity(overlayPos) as BlockEntityStampedOverlay;
                if (be == null) return;
                be.Face = face.Code;
            }

            double placedDay = world.Calendar.TotalDays;
            be.AddLayer(new StampLayer(gridSize, cellColors, cellBorders, edgeBorder, placedDay));

            stencil.SetCachedColors(stencilSlot.Itemstack, cellColors);

            stencilSlot.Itemstack.Collectible.DamageItem(world, player.Entity, stencilSlot);

            world.PlaySoundAt(
                new AssetLocation("sounds/effect/squish1"),
                overlayPos.X + 0.5, overlayPos.Y + 0.5, overlayPos.Z + 0.5,
                player, true, 12f
            );
        }

        // --- Tooltip ---

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            var stack = inSlot.Itemstack;
            int gridSize = GetGridSize(stack);

            if (gridSize > 0)
            {
                dsc.AppendLine(Lang.Get("stencilandstamping:stencil-grid", gridSize, gridSize));

                int filled = 0;
                bool[] pattern = GetCellPattern(stack);
                if (pattern != null)
                {
                    foreach (bool cell in pattern)
                        if (cell) filled++;
                }
                dsc.AppendLine(Lang.Get("stencilandstamping:stencil-filled", filled, gridSize * gridSize));

                if (HasCellBorders(stack))
                    dsc.AppendLine(Lang.Get("stencilandstamping:stencil-cellborders"));
                if (HasEdgeBorder(stack))
                    dsc.AppendLine(Lang.Get("stencilandstamping:stencil-edgeborder"));

                if (HasCachedColors(stack))
                    dsc.AppendLine(Lang.Get("stencilandstamping:stencil-cached"));
            }
            else
            {
                dsc.AppendLine(Lang.Get("stencilandstamping:stencil-blank"));
            }
        }
    }
}

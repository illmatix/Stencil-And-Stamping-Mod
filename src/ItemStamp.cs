using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace StencilAndStamping
{
    public class ItemStamp : Item
    {
        // --- Design attributes (copied from stencil at assembly) ---

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

        /// <summary>
        /// Copy design attributes from a stencil onto this stamp.
        /// Called during crafting assembly.
        /// </summary>
        public static void CopyDesignFromStencil(ItemStack stencilStack, ItemStack stampStack)
        {
            stampStack.Attributes.SetInt("gridSize", stencilStack.Attributes.GetInt("gridSize", 0));

            byte[] pattern = stencilStack.Attributes.GetBytes("cellPattern");
            if (pattern != null)
                stampStack.Attributes.SetBytes("cellPattern", pattern);

            stampStack.Attributes.SetBool("cellBorders", stencilStack.Attributes.GetBool("cellBorders", false));
            stampStack.Attributes.SetBool("edgeBorder", stencilStack.Attributes.GetBool("edgeBorder", false));
        }

        // --- Cached color layout (set after first stamp use) ---

        /// <summary>
        /// Colors are stored as a comma-delimited string (e.g. "red,,blue,,red").
        /// Empty strings represent transparent cells.
        /// </summary>
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

        // --- Crafting: inherit design from stencil ---

        public override void OnCreatedByCrafting(ItemSlot[] allInputslots, ItemSlot outputSlot, GridRecipe byRecipe)
        {
            base.OnCreatedByCrafting(allInputslots, outputSlot, byRecipe);

            foreach (ItemSlot slot in allInputslots)
            {
                if (slot?.Itemstack?.Item is ItemStencil)
                {
                    CopyDesignFromStencil(slot.Itemstack, outputSlot.Itemstack);
                    break;
                }
            }
        }

        // --- Interaction: stamp a surface ---

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (blockSel == null) return;
            if (!IsDesigned(slot.Itemstack)) return;

            var world = byEntity.World;
            var player = (byEntity as EntityPlayer)?.Player;
            if (player == null) return;

            // Don't stamp on cutting boards (that's for resetting)
            Block targetBlock = world.BlockAccessor.GetBlock(blockSel.Position);
            if (targetBlock is BlockCuttingBoard)
            {
                // Clear cached colors on cutting board interaction
                if (world.Side == EnumAppSide.Server && HasCachedColors(slot.Itemstack))
                {
                    ClearCachedColors(slot.Itemstack);
                    slot.MarkDirty();
                }
                handling = EnumHandHandling.PreventDefault;
                return;
            }

            // Validate target surface
            if (!IsValidSurface(targetBlock)) return;

            // Check if there's already an overlay — if so, check layer limit
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
                // Something else is in the way
                return;
            }

            // If we have cached colors, re-stamp directly (server side)
            if (HasCachedColors(slot.Itemstack))
            {
                if (world.Side == EnumAppSide.Server)
                {
                    string[] colors = GetCachedColors(slot.Itemstack);
                    ApplyStamp(world, player, slot, overlayPos, blockSel.Face, colors);
                }
                handling = EnumHandHandling.PreventDefault;
                return;
            }

            // No cached colors — open the stamping GUI (client only)
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
                || mat == EnumBlockMaterial.Soil; // plaster is often Soil material
        }

        public static void ApplyStamp(IWorldAccessor world, IPlayer player, ItemSlot stampSlot,
            BlockPos overlayPos, BlockFacing face, string[] cellColors)
        {
            var stamp = stampSlot.Itemstack.Item as ItemStamp;
            if (stamp == null) return;

            int gridSize = stamp.GetGridSize(stampSlot.Itemstack);
            bool cellBorders = stamp.HasCellBorders(stampSlot.Itemstack);
            bool edgeBorder = stamp.HasEdgeBorder(stampSlot.Itemstack);

            // Count filled cells for ink consumption
            int filledCells = 0;
            foreach (string c in cellColors)
            {
                if (!string.IsNullOrEmpty(c)) filledCells++;
            }
            if (filledCells == 0) return;

            // Place or get existing overlay block
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

            // Add the layer
            double placedDay = world.Calendar.TotalDays;
            be.AddLayer(new StampLayer(gridSize, cellColors, cellBorders, edgeBorder, placedDay));

            // Cache the colors on the stamp for re-use
            stamp.SetCachedColors(stampSlot.Itemstack, cellColors);

            // Damage the stamp
            stampSlot.Itemstack.Collectible.DamageItem(world, player.Entity, stampSlot);

            // Sound
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
                    dsc.AppendLine(Lang.Get("stencilandstamping:stamp-cached"));
            }
            else
            {
                dsc.AppendLine(Lang.Get("stencilandstamping:stamp-nodesign"));
            }
        }
    }
}

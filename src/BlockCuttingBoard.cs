using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace StencilAndStamping
{
    public class BlockCuttingBoard : Block
    {
        public override bool OnBlockInteractStart(
            IWorldAccessor world,
            IPlayer byPlayer,
            BlockSelection blockSel)
        {
            ItemSlot activeSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (activeSlot == null || activeSlot.Empty)
                return base.OnBlockInteractStart(world, byPlayer, blockSel);

            // Check if player is holding a blank stencil
            if (activeSlot.Itemstack?.Item is ItemStencil stencil)
            {
                if (stencil.IsDesigned(activeSlot.Itemstack))
                {
                    if (world.Side == EnumAppSide.Client)
                    {
                        (world.Api as ICoreClientAPI)?
                            .TriggerIngameError(this, "alreadydesigned",
                                "This stencil already has a design.");
                    }
                    return true;
                }

                // Open the design GUI (client side only)
                if (world.Side == EnumAppSide.Client)
                {
                    var capi = world.Api as ICoreClientAPI;
                    var dialog = new GuiDialogCuttingBoard(capi, blockSel.Position, byPlayer);
                    dialog.TryOpen();
                }

                return true;
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            return "Cutting Board\nRight-click with a blank stencil to design a pattern.";
        }
    }
}

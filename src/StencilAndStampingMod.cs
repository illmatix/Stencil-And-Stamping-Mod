using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace StencilAndStamping
{
    public class StencilAndStampingMod : ModSystem
    {
        public static StencilAndStampingConfig Config { get; private set; }
        public static IClientNetworkChannel ClientChannel { get; private set; }

        private const string NetworkChannelId = "stencilandstamping";

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            Config = api.LoadModConfig<StencilAndStampingConfig>("StencilAndStampingConfig.json");
            if (Config == null)
            {
                Config = new StencilAndStampingConfig();
                api.StoreModConfig(Config, "StencilAndStampingConfig.json");
            }

            api.RegisterItemClass("ItemInkPot", typeof(ItemInkPot));
            api.RegisterItemClass("ItemStencil", typeof(ItemStencil));
            api.RegisterItemClass("ItemStamp", typeof(ItemStamp));
            api.RegisterBlockClass("BlockCuttingBoard", typeof(BlockCuttingBoard));
            api.RegisterBlockEntityClass("BECuttingBoard", typeof(BlockEntityCuttingBoard));
            api.RegisterBlockClass("BlockStampedOverlay", typeof(BlockStampedOverlay));
            api.RegisterBlockEntityClass("BEStampedOverlay", typeof(BlockEntityStampedOverlay));
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            ClientChannel = capi.Network
                .RegisterChannel(NetworkChannelId)
                .RegisterMessageType<CuttingBoardDesignPacket>()
                .RegisterMessageType<StampDesignPacket>();
        }

        public override void StartServerSide(ICoreServerAPI sapi)
        {
            sapi.Network
                .RegisterChannel(NetworkChannelId)
                .RegisterMessageType<CuttingBoardDesignPacket>()
                .RegisterMessageType<StampDesignPacket>()
                .SetMessageHandler<CuttingBoardDesignPacket>(OnDesignPacketReceived)
                .SetMessageHandler<StampDesignPacket>(OnStampPacketReceived);
        }

        private void OnDesignPacketReceived(IServerPlayer fromPlayer, CuttingBoardDesignPacket packet)
        {
            var world = fromPlayer.Entity.World;
            var boardPos = packet.BoardPos;

            // Validate the block at position is a cutting board
            Block block = world.BlockAccessor.GetBlock(boardPos);
            if (block is not BlockCuttingBoard) return;

            // Validate grid size
            if (packet.GridSize < 2 || packet.GridSize > 5) return;
            if (packet.CellPattern == null || packet.CellPattern.Length != packet.GridSize * packet.GridSize) return;

            // Check at least one cell is filled
            bool anyFilled = false;
            foreach (bool cell in packet.CellPattern)
            {
                if (cell) { anyFilled = true; break; }
            }
            if (!anyFilled) return;

            // Check player is holding a blank stencil
            ItemSlot activeSlot = fromPlayer.InventoryManager.ActiveHotbarSlot;
            if (activeSlot == null || activeSlot.Empty) return;
            if (activeSlot.Itemstack?.Item is not ItemStencil stencil) return;
            if (stencil.IsDesigned(activeSlot.Itemstack)) return;

            // Check hotbar has a knife
            ItemSlot knifeSlot = null;
            var hotbar = fromPlayer.InventoryManager.GetOwnInventory(GlobalConstants.hotBarInvClassName);
            if (hotbar != null)
            {
                foreach (ItemSlot slot in hotbar)
                {
                    if (slot == null || slot.Empty) continue;
                    string code = slot.Itemstack?.Collectible?.Code?.Path ?? "";
                    if (code.Contains("knife"))
                    {
                        knifeSlot = slot;
                        break;
                    }
                }
            }
            if (knifeSlot == null) return;

            // Apply the design to the stencil
            ItemStencil.ApplyDesign(
                activeSlot.Itemstack,
                packet.GridSize,
                packet.CellPattern,
                packet.CellBorders,
                packet.EdgeBorder
            );
            activeSlot.MarkDirty();

            // Damage the knife
            knifeSlot.Itemstack.Collectible.DamageItem(world, fromPlayer.Entity, knifeSlot);

            // Sound feedback
            world.PlaySoundAt(
                new AssetLocation("sounds/effect/scissorscut"),
                boardPos.X + 0.5, boardPos.Y + 0.5, boardPos.Z + 0.5,
                fromPlayer, true, 12f
            );
        }

        private void OnStampPacketReceived(IServerPlayer fromPlayer, StampDesignPacket packet)
        {
            var world = fromPlayer.Entity.World;

            // Validate
            if (packet.GridSize < 2 || packet.GridSize > 5) return;
            if (packet.CellColors == null || packet.CellColors.Length != packet.GridSize * packet.GridSize) return;

            bool anyColored = false;
            foreach (string c in packet.CellColors)
            {
                if (!string.IsNullOrEmpty(c)) { anyColored = true; break; }
            }
            if (!anyColored) return;

            // Check player is holding a designed stencil
            ItemSlot activeSlot = fromPlayer.InventoryManager.ActiveHotbarSlot;
            if (activeSlot == null || activeSlot.Empty) return;
            if (activeSlot.Itemstack?.Item is not ItemStencil) return;

            var overlayPos = packet.TargetPos;
            var face = BlockFacing.FromCode(packet.Face);
            if (face == null) return;

            ItemStencil.ApplyStencil(world, fromPlayer, activeSlot, overlayPos, face, packet.CellColors);
        }
    }
}

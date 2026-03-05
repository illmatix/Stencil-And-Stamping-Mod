using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace StencilAndStamping
{
    public class BlockStampedOverlay : Block
    {
        /// <summary>
        /// Right-click to scrape off the top layer.
        /// </summary>
        public override bool OnBlockInteractStart(
            IWorldAccessor world,
            IPlayer byPlayer,
            BlockSelection blockSel)
        {
            if (world.Side != EnumAppSide.Server)
                return true;

            var be = world.BlockAccessor.GetBlockEntity(blockSel.Position)
                as BlockEntityStampedOverlay;
            if (be == null) return true;

            bool empty = be.RemoveTopLayer();
            if (empty)
            {
                world.BlockAccessor.SetBlock(0, blockSel.Position);
            }

            world.PlaySoundAt(
                new AssetLocation("sounds/effect/scrape"),
                blockSel.Position.X + 0.5, blockSel.Position.Y + 0.5, blockSel.Position.Z + 0.5,
                byPlayer, true, 12f
            );

            return true;
        }

        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            var be = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityStampedOverlay;
            if (be != null)
            {
                int count = be.Layers.Count;
                int max = StencilAndStampingMod.Config.MaxLayersPerFace;
                return $"Stamped Design\nLayers: {count}/{max}\nFace: {be.Face}\nRight-click to scrape off top layer.";
            }
            return base.GetPlacedBlockInfo(world, pos, forPlayer);
        }
    }
}

using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace StencilAndStamping
{
    public class BlockEntityCuttingBoard : BlockEntity
    {
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolve)
        {
            base.FromTreeAttributes(tree, worldForResolve);
        }
    }
}

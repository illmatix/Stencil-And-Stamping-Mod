using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

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
            }
            else
            {
                dsc.AppendLine(Lang.Get("stencilandstamping:stencil-blank"));
            }
        }
    }
}

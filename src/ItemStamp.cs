using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

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

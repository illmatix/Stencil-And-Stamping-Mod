using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace StencilAndStamping
{
    public class ItemInkPot : Item
    {
        public string GetInkColor(ItemStack stack)
        {
            return Variant["color"];
        }

        public int GetRemainingUses(ItemStack stack)
        {
            return stack.Attributes.GetInt("remainingUses", GetMaxUses());
        }

        public int GetMaxUses()
        {
            return StencilAndStampingMod.Config.InkUsesPerPot;
        }

        public bool ConsumeUse(IPlayer player, ItemStack stack)
        {
            int remaining = GetRemainingUses(stack);
            if (remaining <= 0) return false;

            remaining--;
            stack.Attributes.SetInt("remainingUses", remaining);

            if (remaining <= 0)
            {
                stack.StackSize--;
                if (stack.StackSize <= 0)
                {
                    player.InventoryManager.ActiveHotbarSlot.Itemstack = null;
                }
                player.InventoryManager.ActiveHotbarSlot.MarkDirty();
            }

            return true;
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            int remaining = GetRemainingUses(inSlot.Itemstack);
            int max = GetMaxUses();
            dsc.AppendLine(Lang.Get("stencilandstamping:inkpot-uses", remaining, max));
        }
    }
}

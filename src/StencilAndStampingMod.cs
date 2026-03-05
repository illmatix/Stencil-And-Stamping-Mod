using Vintagestory.API.Common;

namespace StencilAndStamping
{
    public class StencilAndStampingMod : ModSystem
    {
        public static StencilAndStampingConfig Config { get; private set; }

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
        }
    }
}

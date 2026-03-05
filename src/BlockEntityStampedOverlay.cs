using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace StencilAndStamping
{
    public class BlockEntityStampedOverlay : BlockEntity
    {
        public string Face { get; set; } = "north";
        public List<StampLayer> Layers { get; set; } = new List<StampLayer>();

        private long weatheringTickId;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if (api.Side == EnumAppSide.Server && StencilAndStampingMod.Config.EnableWeathering)
            {
                // Check weathering every 60 seconds (in-game)
                weatheringTickId = api.Event.RegisterGameTickListener(OnWeatheringTick, 60000);
            }
        }

        public bool CanAddLayer()
        {
            return Layers.Count < StencilAndStampingMod.Config.MaxLayersPerFace;
        }

        public void AddLayer(StampLayer layer)
        {
            Layers.Add(layer);
            MarkDirty(true);
        }

        /// <summary>
        /// Removes the topmost layer. Returns true if the overlay is now empty
        /// and should be destroyed.
        /// </summary>
        public bool RemoveTopLayer()
        {
            if (Layers.Count == 0) return true;
            Layers.RemoveAt(Layers.Count - 1);
            MarkDirty(true);
            return Layers.Count == 0;
        }

        private void OnWeatheringTick(float dt)
        {
            if (!StencilAndStampingMod.Config.EnableWeathering) return;
            if (Layers.Count == 0) return;

            // Only weather if exposed to sky
            if (Api.World.BlockAccessor.GetRainMapHeightAt(Pos.X, Pos.Z) > Pos.Y)
                return;

            double currentDay = Api.World.Calendar.TotalDays;
            int weatheringDays = StencilAndStampingMod.Config.WeatheringDays;
            bool changed = false;

            // Remove expired layers (oldest first, but check all)
            for (int i = Layers.Count - 1; i >= 0; i--)
            {
                if (currentDay - Layers[i].PlacedDay >= weatheringDays)
                {
                    Layers.RemoveAt(i);
                    changed = true;
                }
            }

            if (changed)
            {
                if (Layers.Count == 0)
                {
                    Api.World.BlockAccessor.SetBlock(0, Pos);
                }
                else
                {
                    MarkDirty(true);
                }
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetString("face", Face);
            tree.SetInt("layerCount", Layers.Count);

            for (int i = 0; i < Layers.Count; i++)
            {
                string prefix = "layer" + i + "_";
                var layer = Layers[i];
                tree.SetInt(prefix + "gridSize", layer.GridSize);
                tree.SetString(prefix + "cellColors", string.Join(",", layer.CellColors));
                tree.SetBool(prefix + "cellBorders", layer.CellBorders);
                tree.SetBool(prefix + "edgeBorder", layer.EdgeBorder);
                tree.SetDouble(prefix + "placedDay", layer.PlacedDay);
            }
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolve)
        {
            base.FromTreeAttributes(tree, worldForResolve);
            Face = tree.GetString("face", "north");
            int layerCount = tree.GetInt("layerCount", 0);

            Layers.Clear();
            for (int i = 0; i < layerCount; i++)
            {
                string prefix = "layer" + i + "_";
                int gridSize = tree.GetInt(prefix + "gridSize", 3);
                string colorsRaw = tree.GetString(prefix + "cellColors", "");
                string[] cellColors = colorsRaw.Length > 0 ? colorsRaw.Split(',') : new string[gridSize * gridSize];
                bool cellBorders = tree.GetBool(prefix + "cellBorders", false);
                bool edgeBorder = tree.GetBool(prefix + "edgeBorder", false);
                double placedDay = tree.GetDouble(prefix + "placedDay", 0);

                Layers.Add(new StampLayer(gridSize, cellColors, cellBorders, edgeBorder, placedDay));
            }
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            if (weatheringTickId != 0)
            {
                Api.Event.UnregisterGameTickListener(weatheringTickId);
            }
        }
    }
}

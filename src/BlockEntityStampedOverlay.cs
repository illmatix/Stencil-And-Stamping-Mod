using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace StencilAndStamping
{
    public class BlockEntityStampedOverlay : BlockEntity
    {
        public string Face { get; set; } = "north";
        public List<StampLayer> Layers { get; set; } = new List<StampLayer>();

        private long weatheringTickId;
        private MeshData mesh;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if (api.Side == EnumAppSide.Server && StencilAndStampingMod.Config.EnableWeathering)
            {
                weatheringTickId = api.Event.RegisterGameTickListener(OnWeatheringTick, 60000);
            }

            if (api.Side == EnumAppSide.Client)
            {
                GenMesh();
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

            if (Api.World.BlockAccessor.GetRainMapHeightAt(Pos.X, Pos.Z) > Pos.Y)
                return;

            double currentDay = Api.World.Calendar.TotalDays;
            int weatheringDays = StencilAndStampingMod.Config.WeatheringDays;
            bool changed = false;

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

        // --- Custom mesh rendering ---

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (mesh != null)
            {
                mesher.AddMeshData(mesh);
                return true;
            }
            return false;
        }

        private void GenMesh()
        {
            if (Api?.Side != EnumAppSide.Client) return;
            if (Layers.Count == 0) { mesh = null; return; }

            mesh = new MeshData(24, 36);

            BlockFacing face = BlockFacing.FromCode(Face);
            if (face == null) face = BlockFacing.NORTH;

            for (int li = 0; li < Layers.Count; li++)
            {
                var layer = Layers[li];
                float layerOffset = 0.002f + li * 0.002f;

                for (int row = 0; row < layer.GridSize; row++)
                {
                    for (int col = 0; col < layer.GridSize; col++)
                    {
                        int idx = row * layer.GridSize + col;
                        if (idx >= layer.CellColors.Length) continue;

                        string color = layer.CellColors[idx];
                        if (string.IsNullOrEmpty(color)) continue;

                        int rgba = GetColorRgba(color);
                        AddCellQuad(face, layer.GridSize, row, col, layerOffset, rgba);
                    }
                }

                if (layer.CellBorders)
                {
                    AddGridBorders(face, layer.GridSize, layerOffset + 0.001f, unchecked((int)0xFF404040));
                }
                if (layer.EdgeBorder)
                {
                    AddEdgeBorder(face, layer.GridSize, layerOffset + 0.001f, unchecked((int)0xFF202020));
                }
            }
        }

        private void AddCellQuad(BlockFacing face, int gridSize, int row, int col, float offset, int rgba)
        {
            float cellSize = 1f / gridSize;
            float u0 = col * cellSize;
            float v0 = row * cellSize;
            float u1 = u0 + cellSize;
            float v1 = v0 + cellSize;

            // Shrink slightly to avoid z-fighting between adjacent cells
            float pad = 0.001f;
            u0 += pad; v0 += pad;
            u1 -= pad; v1 -= pad;

            AddFaceQuad(face, u0, v0, u1, v1, offset, rgba);
        }

        private void AddFaceQuad(BlockFacing face, float u0, float v0, float u1, float v1, float offset, int color)
        {
            float[][] verts = GetFaceVertices(face, u0, v0, u1, v1, offset);
            int baseIdx = mesh.VerticesCount;

            for (int i = 0; i < 4; i++)
            {
                mesh.AddVertex(verts[i][0], verts[i][1], verts[i][2], 0, 0, color);
            }

            mesh.AddIndex(baseIdx);
            mesh.AddIndex(baseIdx + 1);
            mesh.AddIndex(baseIdx + 2);
            mesh.AddIndex(baseIdx);
            mesh.AddIndex(baseIdx + 2);
            mesh.AddIndex(baseIdx + 3);
        }

        private float[][] GetFaceVertices(BlockFacing face, float u0, float v0, float u1, float v1, float offset)
        {
            // Returns 4 vertices [x,y,z] for a quad on the given face
            // Face normals point outward from the target block, overlay sits just outside
            if (face == BlockFacing.NORTH)
            {
                float z = offset;
                return new[] {
                    new[] { 1 - u1, 1 - v1, z },
                    new[] { 1 - u0, 1 - v1, z },
                    new[] { 1 - u0, 1 - v0, z },
                    new[] { 1 - u1, 1 - v0, z }
                };
            }
            if (face == BlockFacing.SOUTH)
            {
                float z = 1 - offset;
                return new[] {
                    new[] { u0, 1 - v1, z },
                    new[] { u1, 1 - v1, z },
                    new[] { u1, 1 - v0, z },
                    new[] { u0, 1 - v0, z }
                };
            }
            if (face == BlockFacing.WEST)
            {
                float x = offset;
                return new[] {
                    new[] { x, 1 - v1, u0 },
                    new[] { x, 1 - v1, u1 },
                    new[] { x, 1 - v0, u1 },
                    new[] { x, 1 - v0, u0 }
                };
            }
            if (face == BlockFacing.EAST)
            {
                float x = 1 - offset;
                return new[] {
                    new[] { x, 1 - v1, 1 - u1 },
                    new[] { x, 1 - v1, 1 - u0 },
                    new[] { x, 1 - v0, 1 - u0 },
                    new[] { x, 1 - v0, 1 - u1 }
                };
            }
            if (face == BlockFacing.UP)
            {
                float y = 1 - offset;
                return new[] {
                    new[] { u0, y, v0 },
                    new[] { u1, y, v0 },
                    new[] { u1, y, v1 },
                    new[] { u0, y, v1 }
                };
            }
            // DOWN
            {
                float y = offset;
                return new[] {
                    new[] { u0, y, 1 - v1 },
                    new[] { u1, y, 1 - v1 },
                    new[] { u1, y, 1 - v0 },
                    new[] { u0, y, 1 - v0 }
                };
            }
        }

        private void AddGridBorders(BlockFacing face, int gridSize, float offset, int color)
        {
            float lineWidth = 0.01f;
            float cellSize = 1f / gridSize;

            for (int i = 1; i < gridSize; i++)
            {
                float pos = i * cellSize;
                AddFaceQuad(face, 0, pos - lineWidth, 1, pos + lineWidth, offset, color);
                AddFaceQuad(face, pos - lineWidth, 0, pos + lineWidth, 1, offset, color);
            }
        }

        private void AddEdgeBorder(BlockFacing face, int gridSize, float offset, int color)
        {
            float lineWidth = 0.02f;
            AddFaceQuad(face, 0, 0, 1, lineWidth, offset, color);
            AddFaceQuad(face, 0, 1 - lineWidth, 1, 1, offset, color);
            AddFaceQuad(face, 0, 0, lineWidth, 1, offset, color);
            AddFaceQuad(face, 1 - lineWidth, 0, 1, 1, offset, color);
        }

        private static int GetColorRgba(string colorName)
        {
            // ARGB packed int
            return colorName switch
            {
                "black"  => unchecked((int)0xFF1A1A1A),
                "white"  => unchecked((int)0xFFEDEDED),
                "red"    => unchecked((int)0xFFC03030),
                "blue"   => unchecked((int)0xFF3030C0),
                "yellow" => unchecked((int)0xFFD0D030),
                "green"  => unchecked((int)0xFF30A030),
                "brown"  => unchecked((int)0xFF8B5E3C),
                "orange" => unchecked((int)0xFFD08030),
                _        => unchecked((int)0xFFC030C0),  // fallback magenta
            };
        }

        // --- Serialization ---

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

            if (Api?.Side == EnumAppSide.Client)
            {
                GenMesh();
                MarkDirty(true);
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

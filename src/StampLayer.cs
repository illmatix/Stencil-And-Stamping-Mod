namespace StencilAndStamping
{
    /// <summary>
    /// One layer of a stamped design on a block face.
    /// Multiple layers stack back-to-front on the same face.
    /// </summary>
    public class StampLayer
    {
        public int GridSize { get; set; }
        public string[] CellColors { get; set; }
        public bool CellBorders { get; set; }
        public bool EdgeBorder { get; set; }
        public double PlacedDay { get; set; }

        public StampLayer() { }

        public StampLayer(int gridSize, string[] cellColors, bool cellBorders, bool edgeBorder, double placedDay)
        {
            GridSize = gridSize;
            CellColors = cellColors;
            CellBorders = cellBorders;
            EdgeBorder = edgeBorder;
            PlacedDay = placedDay;
        }
    }
}

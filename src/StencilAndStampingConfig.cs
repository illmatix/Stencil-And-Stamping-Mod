namespace StencilAndStamping
{
    public class StencilAndStampingConfig
    {
        // Pattern set toggles
        public bool EnableGeometricPatterns { get; set; } = true;
        public bool EnableNaturePatterns { get; set; } = true;
        public bool EnableSymbolPatterns { get; set; } = true;

        // Stamp durability
        public float StampDurabilityMultiplier { get; set; } = 1.0f;

        // Ink
        public int InkUsesPerPot { get; set; } = 20;

        // Layers
        public int MaxLayersPerFace { get; set; } = 4;

        // Weathering
        public bool EnableWeathering { get; set; } = true;
        public int WeatheringDays { get; set; } = 30;
    }
}

# Configuration

Config file: `VintagestoryData/ModConfig/StencilAndStampingConfig.json`

## Default Config

```json
{
  "EnableGeometricPatterns": true,
  "EnableNaturePatterns": true,
  "EnableSymbolPatterns": true,

  "StampDurabilityMultiplier": 1.0,
  "InkUsesPerPot": 20,

  "MaxLayersPerFace": 4,

  "EnableWeathering": true,
  "WeatheringDays": 30
}
```

## Options

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `EnableGeometricPatterns` | bool | true | Toggle geometric template set |
| `EnableNaturePatterns` | bool | true | Toggle nature template set |
| `EnableSymbolPatterns` | bool | true | Toggle symbol template set |
| `StampDurabilityMultiplier` | float | 1.0 | Scale stamp durability (2.0 = double life) |
| `InkUsesPerPot` | int | 20 | Stamps per ink pot |
| `MaxLayersPerFace` | int | 4 | Maximum stamped layers per block face |
| `EnableWeathering` | bool | true | Stamps fade over time when exposed to rain |
| `WeatheringDays` | int | 30 | In-game days before an exposed stamp fades |

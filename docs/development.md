# Development

## Setup

1. .NET 8.0 SDK + `VINTAGE_STORY` env var pointing to game install
2. `dotnet new install VintageStory.Mod.Templates`
3. Create project, replace `src/` with this mod's files
4. Reference `VintagestoryAPI.dll` + `VSSurvivalMod.dll`
5. Build → copy output + `assets/` + `modinfo.json` to `Mods/`

Quick test: Drop entire folder with `.cs` files into `Mods/` — VS compiles at runtime.

## Textures Needed

### Items
- Stencil textures per material: `stencil-{leather,copper,tinbronze,iron}.png`
- Stamp tool texture: `stamp.png` (handle + stencil head)
- Ink pot textures per color: `inkpot-{black,white,red,blue,yellow,green,brown,orange}.png`

### Block Overlays
- Pattern overlay textures in `textures/block/patterns/`:
  - One `.png` per pattern (e.g., `diamond.png`, `vine.png`, `arrow.png`)
  - Semi-transparent PNGs — pattern on alpha, tinted by ink color at runtime

### Cutting Board
- `cuttingboard.png` — top-down view of the board surface

Tip: Pattern textures should be 16x16 or 32x32, designed as tileable where appropriate (borders, crosshatch).

## Future Ideas

- [ ] Player-drawn custom patterns (pixel editor GUI)
- [ ] Stamp roller tool for continuous border application
- [ ] Pattern mirroring and rotation when stamping
- [ ] Multi-color stamping (stamp multiple layers)
- [ ] Pottery integration (stamp clay before firing)
- [ ] Stamp collection / pattern book item
- [ ] Fade animation for weathering (gradual, not instant)

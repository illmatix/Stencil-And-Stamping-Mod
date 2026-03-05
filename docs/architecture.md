# Architecture

## Project Structure

```
StencilAndStamping/
├── modinfo.json
├── docs/                              # Documentation
│   ├── README.md                      # Table of contents
│   ├── patterns-and-materials.md
│   ├── crafting-guide.md
│   ├── configuration.md
│   ├── architecture.md
│   ├── development.md
│   ├── branching.md
│   └── roadmap.md
├── assets/stencilandstamping/
│   ├── itemtypes/
│   │   ├── stencil.json               # Stencil items (pattern × material variants)
│   │   ├── stamp.json                 # Assembled stamp tool
│   │   └── inkpot.json                # Ink pots (color variants)
│   ├── blocktypes/
│   │   ├── cuttingboard.json          # Cutting board for stencil creation
│   │   └── stampedoverlay.json        # Overlay block for stamped patterns
│   ├── recipes/
│   │   └── grid/
│   │       ├── cuttingboard.json
│   │       ├── stamp.json
│   │       └── inkpot-{color}.json
│   ├── shapes/{item,block}/           # Models
│   ├── textures/
│   │   ├── item/                      # Stencil, stamp, ink textures
│   │   └── block/patterns/            # Stamped pattern overlays
│   └── lang/en.json                   # Item names + descriptions
└── src/
    ├── StencilAndStampingMod.cs        # Entry: config, registration
    ├── StencilAndStampingConfig.cs     # Config definition
    ├── ItemStamp.cs                    # Stamp tool — ink consumption, surface application
    ├── BlockCuttingBoard.cs            # Cutting board — pattern selection GUI
    ├── BlockEntityCuttingBoard.cs      # Cutting board state
    └── BlockStampedOverlay.cs          # Rendered pattern overlay on surfaces
```

## Stamped Patterns as Overlay Blocks

When a player stamps a surface, a thin overlay block entity is placed on the target face. This approach:

- Avoids modifying the underlying block's texture
- Allows patterns to be removed (scrape off topmost layer)
- Supports multiple patterns on adjacent faces of the same block
- Supports **multiple layers** on the same face (rendered back-to-front)
- Stores layer data as a list of grid snapshots

```
BlockEntityStampedOverlay attributes:
├── face: "north"
├── layers: [                            # ordered back-to-front
│   {
│     gridSize: 2,
│     cells: ["white","white","white","white"],
│     cellBorders: false,
│     edgeBorder: false,
│     placedDay: 140
│   },
│   {
│     gridSize: 4,
│     cells: ["","","red","","","red","red","","red","red","","","","","red",""],
│     cellBorders: true,
│     edgeBorder: true,
│     placedDay: 142
│   }
│ ]
```

The first layer is rendered directly on the surface, subsequent layers render on top with transparent cells showing through to layers below.

## Stamping Flow

```
ItemStamp.OnHeldInteractStart():
├── Raycast to target block face
├── Validate surface type (stone, plaster, wood, clay)
├── Check layer count < MaxLayersPerFace
├── Open stamping GUI (grid cells + ink color assignment)
├── On confirm:
│   ├── If no overlay block exists on face → place BlockStampedOverlay
│   ├── Append new layer to overlay's layer list
│   ├── Consume 1 ink use per filled cell from matching ink pots
│   ├── Damage stamp by 1 durability
│   └── Play stamp sound + spawn ink particles
└── Cache the design on the stamp for quick re-stamping
```

## Layer Removal

Scraping a stamped surface removes the **topmost layer** only. If that was the last layer, the overlay block is removed entirely. This lets players peel back layers without losing the work underneath.

## Stamp Design Caching

After the first stamp, the color layout is stored as a tree attribute on the stamp itemstack. Subsequent uses skip the GUI and re-apply the cached design. Players can clear the cache by using the stamp on the cutting board to reset it.

## Weathering System

If enabled, an overlay block exposed to sky (no roof) ticks its age. After `WeatheringDays` in-game days, the pattern fades and the overlay removes itself. Indoor stamps last indefinitely.

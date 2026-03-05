# Patterns & Materials

## Stencil Materials

Stencils are cut from flat sheets. The material determines durability and pattern fidelity.

| Material | Durability | Notes |
|----------|------------|-------|
| Leather | Low (~50 stamps) | Cheap, early-game, slightly rough edges |
| Copper Sheet | Medium (~200 stamps) | Clean lines, mid-game |
| Tin Bronze Sheet | High (~350 stamps) | Crisp detail, late-game |
| Iron Sheet | Highest (~500 stamps) | Sharpest patterns, endgame |

## Grid Design System

When creating a stencil on the cutting board, the player selects a **grid size** — this determines how many cells the design surface is divided into. Players can start from a blank grid or load a **template** (pre-made pattern) and customize it from there.

### Grid Sizes

| Grid | Cells | Detail Level |
|------|-------|--------------|
| 2x2 | 4 | Bold, simple designs — flags, quadrants |
| 3x3 | 9 | Medium detail — letters, basic shapes |
| 4x4 | 16 | Fine detail — pixel art, intricate motifs |
| 5x5 | 25 | Maximum detail — complex designs |

Each cell in the grid can be **colored independently** using any available ink. Empty cells are left transparent (no ink applied to that area when stamped).

### Layering

A single block face can hold **multiple stamped layers**, rendered back-to-front. This enables complex designs built up from simple stamps:

1. Stamp a solid background (e.g. all cells filled white on a 2x2)
2. Stamp a detail pattern on top (e.g. a 4x4 arrow with transparent background)
3. The detail's transparent cells let the background show through

Layers are ordered by the sequence they were applied. Each layer is an independent stamp with its own grid, colors, and border settings. Players can remove individual layers (topmost first) by scraping.

The maximum number of layers per face is configurable (default: 4).

### Borders

Players can configure borders when designing their stencil:

- **Cell borders** — lines between individual grid cells (gives a tiled/mosaic look)
- **Edge border** — a frame around the outside of the entire stencil

Both are optional and toggled independently in the design GUI. Borders stamp in the same color as the nearest filled cell, or in a player-selected border color.

### Design Example (3x3 grid, edge border on)

```
┌───┬───┬───┐
│ R │   │ R │
├───┼───┼───┤
│   │ B │   │
├───┼───┼───┤
│ R │   │ R │
└───┴───┴───┘
  (cell borders + edge border)

vs.

 R     R
    B
 R     R
  (no borders)
```

Where R = red ink, B = blue ink, empty = transparent

## Pattern Templates

Templates are pre-made grid layouts that players can load in the design GUI as a starting point. Each template targets a specific grid size and pre-fills cells with a design. Players can use them as-is or modify individual cells before cutting.

### Geometric (ship with the mod)

| Template | Grid | Description |
|----------|------|-------------|
| Border Line | 4x4 | Horizontal band across the middle rows |
| Diamond | 4x4 | Diamond shape using corner/center cells |
| Chevron | 4x4 | Zigzag / arrow pattern |
| Crosshatch | 4x4 | Overlapping diagonal lines |
| Checkerboard | 4x4 | Alternating filled/empty cells |

### Nature

| Template | Grid | Description |
|----------|------|-------------|
| Leaf | 3x3 | Single leaf silhouette |
| Flower | 3x3 | Simple flower head |
| Tree | 5x5 | Stylized tree silhouette |
| Vine | 4x4 | Trailing vine with small leaves |

### Symbols

| Template | Grid | Description |
|----------|------|-------------|
| Arrow | 3x3 | Directional marker |
| Cross | 3x3 | Plus sign / crossroads |
| Circle | 5x5 | Ring / target mark |
| Rune | 4x4 | Abstract decorative symbol |

Templates define which cells are filled — the player still assigns colors per cell when stamping. A template loaded onto a 3x3 grid can't be used on a 4x4 stencil (grid size must match).

## Ink Colors

Inks are made from pigment powder mixed with a binder. Hard or mineral pigment sources must be **ground into powder** first (quern or mortar). Soft sources like berries and flowers can be mashed directly in a bowl.

| Color | Pigment Source | Preparation | Binder |
|-------|---------------|-------------|--------|
| Black | Charcoal | Grind to powder | Fat |
| White | Chalk | Grind to powder | Fat |
| Red | Red berries | Mash directly | Resin |
| Red | Cinnabar | Grind to powder | Resin |
| Blue | Lapis lazuli | Grind to powder | Resin |
| Blue | Blue clay | Mash directly | Resin |
| Yellow | Yellow flowers | Mash directly | Fat |
| Yellow | Sulfur | Grind to powder | Fat |
| Green | Malachite | Grind to powder | Resin |
| Green | Fern | Mash directly | Resin |
| Brown | Brown clay | Mash directly | Fat |
| Brown | Bark | Grind to powder | Fat |
| Orange | Orange flowers | Mash directly | Resin |
| Orange | Rust (iron oxide) | Grind to powder | Resin |

### Pigment Preparation

- **Grind (hard sources):** Charcoal, chalk, cinnabar, lapis, sulfur, malachite, bark, rust — use a quern or mortar to produce pigment powder
- **Mash (soft sources):** Berries, flowers, clay, fern — combine directly in a bowl to produce pigment paste

Both powder and paste work identically when mixed with a binder to create ink.

### Ink Properties

- Each ink pot has a set number of uses (~20 stamps per pot)
- Inks can be applied to the stamp's offhand slot
- Different surfaces may absorb ink differently (stone = crisp, wood = slightly blurred)

# Crafting Guide

## Step 1: Build a Cutting Board

Grid recipe — a flat surface for cutting stencil patterns:
```
[Plank] [Plank] [Plank]
```

## Step 2: Cut a Stencil

1. Place the cutting board as a block
2. Right-click with a leather piece or metal sheet in hand
3. A design GUI opens:
   - **Select grid size** (2x2, 3x3, 4x4, or 5x5)
   - **Start blank** or **load a template** (pre-made pattern that pre-fills cells)
   - **Edit cells** — toggle individual cells on/off to customize the design
   - **Toggle cell borders** (lines between grid cells — on/off)
   - **Toggle edge border** (frame around the outside — on/off)
4. Confirm the design — a knife in your offhand is consumed/damaged to cut the stencil

The result is a stencil item with the chosen grid layout, cell pattern, and border settings. Colors are assigned later when stamping.

## Step 3: Mix Ink

**Hard sources — grind to powder** (quern or mortar):
```
[Charcoal]  → [Black Pigment Powder]
[Lapis]     → [Blue Pigment Powder]
[Malachite] → [Green Pigment Powder]
```

**Soft sources — mash directly** (combine in bowl):
```
[Red Berries] → [Red Pigment Paste]
[Fern]        → [Green Pigment Paste]
```

**Mix ink** — combine pigment (powder or paste) + binder in a bowl:
```
[Pigment] [Binder (fat/resin)]
[         Bowl               ]
```

Produces an ink pot with ~20 uses.

## Step 4: Assemble the Stamp

Grid recipe — combine stencil + handle:
```
[Stencil]
[ Stick ]
```

The stamp inherits the stencil's grid layout, border settings, and material durability.

## Step 5: Design & Stamp a Surface

1. Hold the stamp in your **main hand**
2. **Right-click** a valid block face (stone, plaster, wood, clay)
3. A stamping GUI opens showing the grid — click each cell to assign a color from your inventory's ink pots
4. Leave cells empty for transparent areas
5. Confirm to stamp the design onto the surface

Each filled cell consumes 1 ink use from the corresponding ink pot. The stamp loses 1 durability per use.

Once a design is stamped, you can re-stamp the same design without reopening the GUI — the stamp remembers the last color layout.

## Step 6: Add More Layers (optional)

Stamp the same block face again to add another layer on top. Transparent cells in the new layer let previous layers show through.

Example workflow:
1. Stamp a **solid white 2x2** as a background
2. Stamp a **red arrow 3x3** on top — the arrow appears over the white fill
3. Stamp a **black border 4x4** on top of that — frames the whole piece

Each face supports up to 4 layers (configurable). Scraping removes the topmost layer first.

---

## Valid Surfaces

| Surface Type | Result Quality | Notes |
|--------------|---------------|-------|
| Stone | Crisp | Best results on smooth stone |
| Plaster | Crisp | Ideal decorating surface |
| Wood (planks) | Slightly soft | Grain absorbs ink unevenly |
| Clay (fired) | Crisp | Works on pottery and bricks |
| Raw clay/soil | Not stampable | Too soft / absorbent |

# Shape Graph — Smoke Sample

Four stages: **Geometry** → **Style** (`ShapeRegion2D`) → **Picture** (`Compose`) → **Bake** (`ShapeBake` at Mesh Output).

```
Star → Noise → Fill → Outline ─┐
Hex → Transform → Fill → Outline ─┼→ Compose → MeshOutput
Circle → Transform → Fill → Outline ─┘
```

Each branch keeps its own fill + outline. Compose stacks paint order; it does not boolean.

Alternate authorship path (single silhouette color): `Union(bare A,B)` → Fill → Outline → Mesh Output.

1. Import the sample from Package Manager.
2. Add `Shape Graph Smoke Demo` to a GameObject.
3. Assign an Unlit/Sprites material with vertex colors on `Procedural Shape Instance`.
4. Re-import after package updates (copied samples do not auto-sync).

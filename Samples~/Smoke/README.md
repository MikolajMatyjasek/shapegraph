# Shape Graph — Smoke Sample

**Geometry domain** (`IShape2D`): compose silhouettes with generators, modifiers, and boolean Union.  
**Mesh domain** (`ShapeMesh2D`): cross once via Fill, then Outline (and optional mesh ops), then Mesh Output.

```
Star → Noise ─┐
Hex → Transform ─┼→ Union → FillMesh → OutlineMesh → MeshOutput
Circle → Transform ─┘
```

1. Import the sample from Package Manager.
2. Add `Shape Graph Smoke Demo` to a GameObject.
3. Assign an Unlit/Sprites material with vertex colors on `Procedural Shape Instance`.
4. Re-import after package updates (copied samples do not auto-sync).

# Shape Graph 2D

Node-based procedural 2D vector mesh and collider generator for Unity 6 with deterministic randomization and live previews.

## Dual domains

1. **Geometry** (`IShape2D`) — analytic shapes, polygon ops, boolean Union/Difference.
2. **Mesh** (`ShapeMesh2D`) — Fill triangulates once; Outline/TransformMesh compose buffers; Mesh Output emits one Unity mesh.

## Extending (without forking)

1. Create an assembly definition that references `Galaretka.ShapeGraph`.
2. Implement `IShape2D` for custom geometry, and/or subclass `ShapeNode` (or `ShapeGeneratorNode` / `ShapeModifierNode` / `TerminalOutputNode`).
3. Annotate node types with `[NodeMenu("Category/Name")]` for future editor discovery via `TypeCache`.
4. Declare ports with CLR types; connections use `PortTypeRegistry` assignability (`Polygon2D` → `IShape2D` is valid; `ShapeMesh2D` is exact-only).

Runtime never depends on the Editor assembly. Bake static objects with `ShapeBaker`; bake prefabs with `Galaretka.ShapeGraph.Editor.Baking.ShapePrefabBaker`.

## Smoke sample

Import **Smoke Demo** from Package Manager:

`Star/Hex/Circle` → `Union` → `FillMesh` → `OutlineMesh` → `Mesh Output`.

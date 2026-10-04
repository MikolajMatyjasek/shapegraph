# Shape Graph 2D

Node-based procedural 2D vector mesh and collider generator for Unity 6 with deterministic randomization and live previews.

## Extending (without forking)

1. Create an assembly definition that references `Galaretka.ShapeGraph`.
2. Implement `IShape2D` for custom geometry, and/or subclass `ShapeNode` (or `ShapeGeneratorNode` / `ShapeModifierNode` / `TerminalOutputNode`).
3. Annotate node types with `[NodeMenu("Category/Name")]` for future editor discovery via `TypeCache`.
4. Declare ports with CLR types; connections use `PortTypeRegistry` assignability (`Polygon2D` → `IShape2D` is valid).

Runtime never depends on the Editor assembly. Bake static objects with `ShapeBaker`; bake prefabs with `Galaretka.ShapeGraph.Editor.Baking.ShapePrefabBaker`.

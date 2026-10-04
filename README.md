# Shape Graph 2D

Node-based procedural 2D vector mesh and collider generator for Unity 6 with deterministic randomization and live previews.

## Four stages

1. **Geometry** (`IShape2D`) — generators, modifiers, boolean Union/Difference/Intersection on bare silhouettes only.
2. **Style** (`ShapeRegion2D`) — Fill, Outline, Tint, Opacity. Declarative paint; no triangulation yet.
3. **Picture** (`ShapePicture2D`) — Compose ordered layers for multi-color authorship.
4. **Bake** — `ShapeBake` at Mesh Output (and editor previews) produces one `GeneratedMeshData`.

Boolean never accepts styled regions. Multi-color means multiple regions in a picture, not “union then hope”.

**Parameters** are a Value layer: author them in the graph editor, wire with `Values/Parameter`, override Exposed on `ProceduralShapeInstance`. **Time** + **Math** nodes animate scalars (editor Time Play / Play Mode `Time.time`).

## Node library (short)

- Generators: Regular Polygon, Star, Rectangle, Ellipse, Capsule, Ring  
- Modifiers: Transform, Offset, Inflate, Noise, Mirror, Shear, Twist, Polar Displace  
- Values: Parameter, Time, Math/*, Vector2/*  
- Style: Fill, Outline, Tint, Opacity · Combine: Union, Difference, Intersection, Compose

## Graph editor

Double-click a Shape Graph asset (or `Window → Galaretka → Shape Graph Editor`). Classic GraphView, typed ports, Undo-safe editing, and per-node pixel previews from the same bake kernel as Play Mode (isolated from the open scene).

## Extending (without forking)

1. Create an assembly definition that references `Galaretka.ShapeGraph`.
2. Implement `IShape2D` for custom geometry, and/or subclass `ShapeNode` (or `ShapeGeneratorNode` / `ShapeModifierNode` / `TerminalOutputNode`).
3. Annotate node types with `[NodeMenu("Category/Name")]` for editor discovery via `TypeCache`.
4. Declare ports with CLR types; connections use `PortTypeRegistry` (Region↛geometry; Picture exact-only; Region may feed Picture/terminal).

Runtime never depends on the Editor assembly. Bake static objects with `ShapeBaker`; bake prefabs with `Galaretka.ShapeGraph.Editor.Baking.ShapePrefabBaker`.

## Smoke sample

Import **Smoke Demo** from Package Manager:

`Star/Hex/Ellipse` → Fill → Outline → `Compose` → Mesh Output (three colors).  
Alternate: `Union(bare A,B)` → Fill → Outline → Mesh Output → **one** fill color.

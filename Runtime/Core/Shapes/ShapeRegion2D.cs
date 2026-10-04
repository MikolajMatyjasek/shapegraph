using UnityEngine;

namespace Galaretka.ShapeGraph.Core
{
    public sealed class ShapeRegion2D : IShape2D
    {
        public IShape2D Geometry;
        public Color32 FillColor;
        public bool HasOutline;
        public Color32 OutlineColor;
        public float OutlineThickness;

        public ShapeRegion2D() { }

        public ShapeRegion2D(IShape2D geometry, Color32 fillColor)
        {
            Geometry = geometry?.CloneShape();
            FillColor = fillColor;
            HasOutline = false;
            OutlineColor = new Color32(20, 24, 36, 255);
            OutlineThickness = 0f;
        }

        public Bounds GetBounds() =>
            Geometry != null ? Geometry.GetBounds() : new Bounds(Vector3.zero, Vector3.zero);

        public IShape2D CloneShape() =>
            new ShapeRegion2D
            {
                Geometry = Geometry?.CloneShape(),
                FillColor = FillColor,
                HasOutline = HasOutline,
                OutlineColor = OutlineColor,
                OutlineThickness = OutlineThickness
            };

        public Polygon2D ToPolygon() => Geometry?.ToPolygon();
    }
}

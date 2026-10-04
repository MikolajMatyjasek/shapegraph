using System;
using Galaretka.ShapeGraph.Core.Geometry;
using UnityEngine;

namespace Galaretka.ShapeGraph.Core
{
    [Serializable]
    public sealed class Ellipse2D : IAnalyticTransformable2D, IAnalyticInflatable2D
    {
        [SerializeField] 
        private Vector2 center;
        [SerializeField] 
        private float radiusX = 0.5f;
        [SerializeField] 
        private float radiusY = 0.35f;
        [SerializeField] 
        private float rotationDegrees;
        [SerializeField] 
        private int tessellationSegments = 48;

        public Vector2 Center
        {
            get => center;
            set => center = ShapeMath.IsFinite(value) ? value : Vector2.zero;
        }

        public float RadiusX
        {
            get => radiusX;
            set => radiusX = ShapeMath.ClampRadius(value);
        }

        public float RadiusY
        {
            get => radiusY;
            set => radiusY = ShapeMath.ClampRadius(value);
        }

        public float RotationDegrees
        {
            get => rotationDegrees;
            set => rotationDegrees = ShapeMath.IsFinite(value) ? value : 0f;
        }

        public int TessellationSegments
        {
            get => tessellationSegments;
            set => tessellationSegments = ShapeMath.ClampSegments(value);
        }

        public Ellipse2D() { }

        public Ellipse2D(Vector2 center, float radiusX, float radiusY, float rotationDegrees = 0f, int segments = 48)
        {
            Center = center;
            RadiusX = radiusX;
            RadiusY = radiusY;
            RotationDegrees = rotationDegrees;
            TessellationSegments = segments;
        }

        public Bounds GetBounds()
        {
            Polygon2D poly = ToPolygon();
            return poly != null ? poly.GetBounds() : new Bounds(center, Vector3.zero);
        }

        public IShape2D CloneShape() =>
            new Ellipse2D(center, radiusX, radiusY, rotationDegrees, tessellationSegments);

        public Polygon2D ToPolygon()
        {
            int segments = ShapeMath.ClampSegments(tessellationSegments);
            var poly = new Polygon2D();
            for (int i = 0; i < segments; i++)
            {
                float t = i / (float)segments;
                float angle = -t * Mathf.PI * 2f;
                Vector2 local = new Vector2(Mathf.Cos(angle) * radiusX, Mathf.Sin(angle) * radiusY);
                local = ShapeMath.Rotate(local, rotationDegrees);
                poly.Points.Add(center + local);
            }

            poly.EnsureClockwise();
            return poly;
        }

        public IShape2D TransformAnalytic(in AffineTransform2D transform)
        {
            Polygon2D poly = ToPolygon();
            PolygonOps.ApplyTransform(poly, transform);
            poly.Sanitize();
            poly.EnsureClockwise();
            return poly.IsValid ? poly : CloneShape();
        }

        public IShape2D InflateAnalytic(float factor)
        {
            float f = ShapeMath.ClampFactor(factor);
            return new Ellipse2D(center, ShapeMath.ClampRadius(radiusX * f), ShapeMath.ClampRadius(radiusY * f),
                rotationDegrees, tessellationSegments);
        }
    }
}

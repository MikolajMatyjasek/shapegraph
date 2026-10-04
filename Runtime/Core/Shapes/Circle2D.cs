using System;
using Galaretka.ShapeGraph.Core.Geometry;
using UnityEngine;

namespace Galaretka.ShapeGraph.Core
{
    [Serializable]
    public sealed class Circle2D : IAnalyticTransformable2D, IAnalyticInflatable2D, IAnalyticOffsettable2D
    {
        [SerializeField] 
        private Vector2 center;
        [SerializeField] 
        private float radius = 0.5f;
        [SerializeField] 
        private int tessellationSegments = 48;

        public Vector2 Center
        {
            get => center;
            set => center = ShapeMath.IsFinite(value) ? value : Vector2.zero;
        }

        public float Radius
        {
            get => radius;
            set => radius = ShapeMath.ClampRadius(value);
        }

        public int TessellationSegments
        {
            get => tessellationSegments;
            set => tessellationSegments = ShapeMath.ClampSegments(value);
        }

        public Circle2D() { }

        public Circle2D(Vector2 center, float radius, int tessellationSegments = 48)
        {
            Center = center;
            Radius = radius;
            TessellationSegments = tessellationSegments;
        }

        public Bounds GetBounds()
        {
            float d = radius * 2f;
            return new Bounds(new Vector3(center.x, center.y, 0f), new Vector3(d, d, 0f));
        }

        public IShape2D CloneShape() =>
            new Circle2D(center, radius, tessellationSegments);

        public Polygon2D ToPolygon()
        {
            int segments = ShapeMath.ClampSegments(tessellationSegments);
            var poly = new Polygon2D();
            for (int i = 0; i < segments; i++)
            {
                float t = i / (float)segments;
                float angle = -t * Mathf.PI * 2f;
                poly.Points.Add(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }

            poly.EnsureClockwise();
            return poly;
        }

        public IShape2D TransformAnalytic(in AffineTransform2D transform)
        {
            if (transform.HasUniformScale())
            {
                float s = transform.Scale.x;
                Vector2 newCenter = transform.Apply(center);
                return new Circle2D(newCenter, ShapeMath.ClampRadius(radius * Mathf.Abs(s)), tessellationSegments);
            }

            Polygon2D poly = ToPolygon();
            PolygonOps.ApplyTransform(poly, transform);
            return poly;
        }

        public IShape2D InflateAnalytic(float factor) =>
            new Circle2D(center, ShapeMath.ClampRadius(radius * ShapeMath.ClampFactor(factor)), tessellationSegments);

        public IShape2D OffsetAnalytic(float distance) =>
            new Circle2D(center, ShapeMath.ClampRadius(radius + distance), tessellationSegments);
    }
}

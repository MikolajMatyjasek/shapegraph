using System;
using Galaretka.ShapeGraph.Core.Geometry;
using UnityEngine;

namespace Galaretka.ShapeGraph.Core
{
    [Serializable]
    public sealed class RegularPolygon2D : IAnalyticTransformable2D, IAnalyticInflatable2D
    {
        [SerializeField] 
        private Vector2 center;
        [SerializeField] 
        private float radius = 0.5f;
        [SerializeField] 
        private int sides = 6;
        [SerializeField] 
        private float rotationDegrees;

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

        public int Sides
        {
            get => sides;
            set => sides = ShapeMath.ClampSides(value);
        }

        public float RotationDegrees
        {
            get => rotationDegrees;
            set => rotationDegrees = ShapeMath.IsFinite(value) ? value : 0f;
        }

        public RegularPolygon2D() { }

        public RegularPolygon2D(Vector2 center, float radius, int sides, float rotationDegrees = 0f)
        {
            Center = center;
            Radius = radius;
            Sides = sides;
            RotationDegrees = rotationDegrees;
        }

        public Bounds GetBounds()
        {
            float d = radius * 2f;
            return new Bounds(new Vector3(center.x, center.y, 0f), new Vector3(d, d, 0f));
        }

        public IShape2D CloneShape() =>
            new RegularPolygon2D(center, radius, sides, rotationDegrees);

        public Polygon2D ToPolygon()
        {
            int count = ShapeMath.ClampSides(sides);
            var poly = new Polygon2D();
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)count;
                float angle = ShapeMath.DegToRad(rotationDegrees) - t * Mathf.PI * 2f;
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
                return new RegularPolygon2D(
                    transform.Apply(center),
                    ShapeMath.ClampRadius(radius * Mathf.Abs(s)),
                    sides,
                    rotationDegrees + transform.RotationDegrees);
            }

            Polygon2D poly = ToPolygon();
            PolygonOps.ApplyTransform(poly, transform);
            return poly;
        }

        public IShape2D InflateAnalytic(float factor) =>
            new RegularPolygon2D(
                center,
                ShapeMath.ClampRadius(radius * ShapeMath.ClampFactor(factor)),
                sides,
                rotationDegrees);
    }
}

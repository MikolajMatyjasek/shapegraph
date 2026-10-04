using System;
using Galaretka.ShapeGraph.Core.Geometry;
using UnityEngine;

namespace Galaretka.ShapeGraph.Core
{
    [Serializable]
    public sealed class Star2D : IAnalyticTransformable2D, IAnalyticInflatable2D
    {
        [SerializeField] 
        private Vector2 center;
        [SerializeField] 
        private float outerRadius = 0.55f;
        [SerializeField] 
        private float innerRadius = 0.25f;
        [SerializeField] 
        private int points = 5;
        [SerializeField] 
        private float rotationDegrees = -90f;

        public Vector2 Center
        {
            get => center;
            set => center = ShapeMath.IsFinite(value) ? value : Vector2.zero;
        }

        public float OuterRadius
        {
            get => outerRadius;
            set => outerRadius = ShapeMath.ClampRadius(value);
        }

        public float InnerRadius
        {
            get => innerRadius;
            set => innerRadius = ShapeMath.ClampRadius(value);
        }

        public int Points
        {
            get => points;
            set => points = ShapeMath.ClampSides(value);
        }

        public float RotationDegrees
        {
            get => rotationDegrees;
            set => rotationDegrees = ShapeMath.IsFinite(value) ? value : 0f;
        }

        public Star2D() { }

        public Star2D(
            Vector2 center,
            float outerRadius,
            float innerRadius,
            int points,
            float rotationDegrees = -90f)
        {
            Center = center;
            OuterRadius = outerRadius;
            InnerRadius = innerRadius;
            Points = points;
            RotationDegrees = rotationDegrees;
            NormalizeRadii();
        }

        public void NormalizeRadii()
        {
            outerRadius = ShapeMath.ClampRadius(outerRadius);
            innerRadius = ShapeMath.ClampRadius(innerRadius);
            if (innerRadius > outerRadius)
            {
                float tmp = innerRadius;
                innerRadius = outerRadius;
                outerRadius = tmp;
            }
        }

        public Bounds GetBounds()
        {
            float r = Mathf.Max(outerRadius, innerRadius);
            float d = r * 2f;
            return new Bounds(new Vector3(center.x, center.y, 0f), new Vector3(d, d, 0f));
        }

        public IShape2D CloneShape() =>
            new Star2D(center, outerRadius, innerRadius, points, rotationDegrees);

        public Polygon2D ToPolygon()
        {
            NormalizeRadii();
            int tips = ShapeMath.ClampSides(points);
            int vertCount = tips * 2;
            var poly = new Polygon2D();

            for (int i = 0; i < vertCount; i++)
            {
                float t = i / (float)vertCount;
                float angle = ShapeMath.DegToRad(rotationDegrees) - t * Mathf.PI * 2f;
                float r = (i % 2 == 0) ? outerRadius : innerRadius;
                poly.Points.Add(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r);
            }

            poly.EnsureClockwise();
            return poly;
        }

        public IShape2D TransformAnalytic(in AffineTransform2D transform)
        {
            if (transform.HasUniformScale())
            {
                float s = Mathf.Abs(transform.Scale.x);
                return new Star2D(
                    transform.Apply(center),
                    ShapeMath.ClampRadius(outerRadius * s),
                    ShapeMath.ClampRadius(innerRadius * s),
                    points,
                    rotationDegrees + transform.RotationDegrees);
            }

            Polygon2D poly = ToPolygon();
            PolygonOps.ApplyTransform(poly, transform);
            return poly;
        }

        public IShape2D InflateAnalytic(float factor)
        {
            float f = ShapeMath.ClampFactor(factor);
            return new Star2D(
                center,
                ShapeMath.ClampRadius(outerRadius * f),
                ShapeMath.ClampRadius(innerRadius * f),
                points,
                rotationDegrees);
        }
    }
}

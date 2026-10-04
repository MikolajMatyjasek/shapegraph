using UnityEngine;

namespace Galaretka.ShapeGraph.Core.Geometry
{
    public static class PolygonOps
    {
        public static Vector2 Centroid(Polygon2D polygon)
        {
            if (polygon == null || !polygon.IsValid) return Vector2.zero;

            float twiceArea = 0f;
            float cx = 0f;
            float cy = 0f;
            int n = polygon.Count;

            for (int i = 0; i < n; i++)
            {
                Vector2 p0 = polygon.Points[i];
                Vector2 p1 = polygon.Points[(i + 1) % n];
                float cross = p0.x * p1.y - p1.x * p0.y;
                twiceArea += cross;
                cx += (p0.x + p1.x) * cross;
                cy += (p0.y + p1.y) * cross;
            }

            if (Mathf.Abs(twiceArea) < 1e-8f)
            {
                Bounds b = polygon.GetBounds();
                return new Vector2(b.center.x, b.center.y);
            }

            float inv = 1f / (3f * twiceArea);
            return new Vector2(cx * inv, cy * inv);
        }

        public static void ApplyTransform(Polygon2D polygon, in AffineTransform2D transform)
        {
            if (polygon == null || !polygon.IsValid) return;

            for (int i = 0; i < polygon.Count; i++)
            {
                polygon.Points[i] = transform.Apply(polygon.Points[i]);
            }
        }

        public static void InflateAbout(Polygon2D polygon, Vector2 pivot, float factor)
        {
            if (polygon == null || !polygon.IsValid) return;

            factor = ShapeMath.ClampFactor(factor);
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 p = polygon.Points[i];
                polygon.Points[i] = pivot + (p - pivot) * factor;
            }
        }

        public static bool TryOffset(Polygon2D input, float distance, float maxMiterLimit, out Polygon2D output)
        {
            output = null;
            if (input == null || !input.IsValid) return false;

            Polygon2D working = input.Clone();
            working.EnsureClockwise();
            working.Sanitize();
            if (!working.IsValid) return false;

            Vector2[] miters = working.CalculateMiterNormals(maxMiterLimit);
            var result = new Polygon2D();
            for (int i = 0; i < working.Count; i++)
            {
                result.Points.Add(working.Points[i] + miters[i] * distance);
            }

            result.Sanitize();
            result.EnsureClockwise();
            if (!result.IsValid) return false;

            output = result;
            return true;
        }

        public static void Displace(Polygon2D polygon, IVertexDisplacement displacement)
        {
            if (polygon == null || !polygon.IsValid || displacement == null) return;

            polygon.EnsureClockwise();
            Vector2[] miters = polygon.CalculateMiterNormals();
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 normal = miters[i].sqrMagnitude > 1e-12f
                    ? miters[i].normalized
                    : Vector2.up;
                polygon.Points[i] = displacement.Displace(i, polygon.Points[i], normal);
            }
        }
    }
}

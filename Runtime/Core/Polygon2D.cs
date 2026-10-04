using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galaretka.ShapeGraph.Core
{
    [Serializable]
    public class Polygon2D : IShape2D
    {
        [SerializeField]
        private List<Vector2> points = new();

        public List<Vector2> Points => points;
        public int Count => points.Count;
        public bool IsValid => points != null && points.Count >= 3;

        public Polygon2D() { }

        public Polygon2D(IEnumerable<Vector2> sourcePoints)
        {
            points = new List<Vector2>(sourcePoints);
        }

        public Polygon2D Clone() => new(points);

        public IShape2D CloneShape() => Clone();

        public Polygon2D ToPolygon() => Clone();

        public Bounds GetBounds()
        {
            if (!IsValid) return new Bounds(Vector3.zero, Vector3.zero);

            Vector2 min = points[0];
            Vector2 max = points[0];
            for (int i = 1; i < points.Count; i++)
            {
                min = Vector2.Min(min, points[i]);
                max = Vector2.Max(max, points[i]);
            }

            Vector2 center = (min + max) * 0.5f;
            Vector2 size = max - min;
            return new Bounds(new Vector3(center.x, center.y, 0f), new Vector3(size.x, size.y, 0f));
        }

        public float CalculateSignedArea()
        {
            if (!IsValid) return 0f;
            float area = 0f;
            int n = points.Count;
            for (int i = 0; i < n; i++)
            {
                Vector2 p1 = points[i];
                Vector2 p2 = points[(i + 1) % n];
                area += (p1.x * p2.y) - (p2.x * p1.y);
            }
            return area * 0.5f;
        }

        public void EnsureClockwise()
        {
            if (CalculateSignedArea() > 0f)
            {
                points.Reverse();
            }
        }

        public void Sanitize(float minDistanceThreshold = 0.0001f)
        {
            if (points.Count < 3) return;

            float sqrThreshold = minDistanceThreshold * minDistanceThreshold;
            for (int i = points.Count - 1; i >= 0; i--)
            {
                Vector2 pt = points[i];
                if (float.IsNaN(pt.x) || float.IsNaN(pt.y) || float.IsInfinity(pt.x) || float.IsInfinity(pt.y))
                {
                    points.RemoveAt(i);
                    continue;
                }

                int prevIdx = (i - 1 + points.Count) % points.Count;
                if ((pt - points[prevIdx]).sqrMagnitude < sqrThreshold)
                {
                    points.RemoveAt(i);
                }
            }
        }

        public Vector2[] CalculateMiterNormals(float maxMiterLength = 3f)
        {
            int n = points.Count;
            Vector2[] miters = new Vector2[n];
            if (n < 3) return miters;

            EnsureClockwise();

            for (int i = 0; i < n; i++)
            {
                Vector2 prev = points[(i - 1 + n) % n];
                Vector2 curr = points[i];
                Vector2 next = points[(i + 1) % n];

                Vector2 edgePrev = (curr - prev).normalized;
                Vector2 edgeNext = (next - curr).normalized;

                Vector2 normPrev = new Vector2(-edgePrev.y, edgePrev.x);
                Vector2 normNext = new Vector2(-edgeNext.y, edgeNext.x);

                Vector2 miter = (normPrev + normNext).normalized;
                float dot = Vector2.Dot(miter, normNext);

                float length = dot > 0.05f ? Mathf.Min(1f / dot, maxMiterLength) : maxMiterLength;
                miters[i] = miter * length;
            }

            return miters;
        }
    }
}

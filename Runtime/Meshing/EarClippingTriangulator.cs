using System.Collections.Generic;
using UnityEngine;

namespace Galaretka.ShapeGraph.Meshing
{
    public static class EarClippingTriangulator
    {
        private static readonly int[] Empty = new int[0];

        public static int[] Triangulate(IList<Vector2> points)
        {
            if (points == null || points.Count < 3) return Empty;

            int n = points.Count;
            var indices = new List<int>(n);
            for (int i = 0; i < n; i++)
            {
                indices.Add(i);
            }

            float area = SignedArea(points);
            if (area > 0f)
            {
                indices.Reverse();
            }

            var triangles = new List<int>((n - 2) * 3);
            int guard = 0;
            int maxGuard = n * n;

            while (indices.Count > 3 && guard < maxGuard)
            {
                guard++;
                bool earFound = false;

                for (int i = 0; i < indices.Count; i++)
                {
                    int iPrev = indices[(i - 1 + indices.Count) % indices.Count];
                    int iCurr = indices[i];
                    int iNext = indices[(i + 1) % indices.Count];

                    Vector2 a = points[iPrev];
                    Vector2 b = points[iCurr];
                    Vector2 c = points[iNext];

                    if (!IsConvex(a, b, c)) continue;

                    if (ContainsAnyPoint(points, indices, iPrev, iCurr, iNext, a, b, c)) continue;

                    triangles.Add(iPrev);
                    triangles.Add(iCurr);
                    triangles.Add(iNext);
                    indices.RemoveAt(i);
                    earFound = true;
                    break;
                }

                if (!earFound) return Empty;
            }

            if (indices.Count == 3)
            {
                triangles.Add(indices[0]);
                triangles.Add(indices[1]);
                triangles.Add(indices[2]);
            }
            else
            {
                return Empty;
            }

            return triangles.ToArray();
        }

        private static float SignedArea(IList<Vector2> points)
        {
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

        private static bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
        {
            float cross = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
            return cross <= 0f;
        }

        private static bool ContainsAnyPoint(
            IList<Vector2> points,
            List<int> indices,
            int iPrev,
            int iCurr,
            int iNext,
            Vector2 a,
            Vector2 b,
            Vector2 c)
        {
            for (int i = 0; i < indices.Count; i++)
            {
                int idx = indices[i];
                if (idx == iPrev || idx == iCurr || idx == iNext) continue;

                if (PointInTriangle(points[idx], a, b, c)) return true;
            }

            return false;
        }

        private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Sign(p, a, b);
            float d2 = Sign(p, b, c);
            float d3 = Sign(p, c, a);
            bool hasNeg = (d1 < 0f) || (d2 < 0f) || (d3 < 0f);
            bool hasPos = (d1 > 0f) || (d2 > 0f) || (d3 > 0f);
            return !(hasNeg && hasPos);
        }

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3) =>
            (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }
}

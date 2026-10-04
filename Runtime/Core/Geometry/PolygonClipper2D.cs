using System.Collections.Generic;
using UnityEngine;

namespace Galaretka.ShapeGraph.Core.Geometry
{
    internal static class PolygonClipper2D
    {
        private const float Epsilon = 1e-6f;

        internal enum ClipOp
        {
            Union = 0,
            Difference = 1
        }

        public static bool TryClip(Polygon2D subject, Polygon2D clip, ClipOp op, out List<Polygon2D> results)
        {
            results = new List<Polygon2D>();
            if (subject == null || !subject.IsValid || clip == null || !clip.IsValid) return false;

            Polygon2D a = Prepare(subject);
            Polygon2D b = Prepare(clip);
            if (a == null || b == null) return false;

            if (!BoundsOverlap(a, b))
            {
                if (op == ClipOp.Union)
                {
                    results.Add(a);
                    results.Add(b);
                    return true;
                }

                results.Add(a);
                return true;
            }

            bool aInsideB = AllVerticesInside(a, b);
            bool bInsideA = AllVerticesInside(b, a);

            Vertex subjectHead = BuildRing(a);
            Vertex clipHead = BuildRing(b);
            if (subjectHead == null || clipHead == null) return false;

            int intersectionCount = InsertIntersections(subjectHead, clipHead);
            if (intersectionCount == 0)
            {
                if (op == ClipOp.Union)
                {
                    if (aInsideB)
                    {
                        results.Add(b);
                    }
                    else if (bInsideA)
                    {
                        results.Add(a);
                    }
                    else
                    {
                        results.Add(a);
                        results.Add(b);
                    }

                    return results.Count > 0;
                }

                if (aInsideB) return false;
                if (bInsideA)
                {
                    Debug.LogWarning("[ShapeGraph] Difference that creates a hole is not supported; returning subject.");
                    results.Add(a);
                    return true;
                }

                results.Add(a);
                return true;
            }

            MarkEntries(subjectHead, b, invert: false);
            MarkEntries(clipHead, a, invert: op == ClipOp.Difference);
            return TraceResult(subjectHead, op, results);
        }

        private static Polygon2D Prepare(Polygon2D source)
        {
            Polygon2D poly = source.Clone();
            poly.Sanitize();
            poly.EnsureClockwise();
            return poly.IsValid ? poly : null;
        }

        private static bool BoundsOverlap(Polygon2D a, Polygon2D b)
        {
            Bounds ba = a.GetBounds();
            Bounds bb = b.GetBounds();
            ba.Expand(Epsilon * 2f);
            return ba.Intersects(bb);
        }

        private static bool AllVerticesInside(Polygon2D poly, Polygon2D container)
        {
            for (int i = 0; i < poly.Count; i++)
            {
                if (!PointInPolygon(poly.Points[i], container)) return false;
            }

            return true;
        }

        private static Vertex BuildRing(Polygon2D poly)
        {
            Vertex first = null;
            Vertex prev = null;
            for (int i = 0; i < poly.Count; i++)
            {
                var v = new Vertex(poly.Points[i]);
                if (first == null)
                {
                    first = v;
                }
                if (prev != null)
                {
                    prev.Next = v;
                    v.Prev = prev;
                }

                prev = v;
            }

            if (first == null || prev == null) return null;

            prev.Next = first;
            first.Prev = prev;
            return first;
        }

        private static int InsertIntersections(Vertex subjectHead, Vertex clipHead)
        {
            int count = 0;
            var subjectEdges = CollectSourceEdges(subjectHead);
            var clipEdges = CollectSourceEdges(clipHead);

            for (int si = 0; si < subjectEdges.Count; si++)
            {
                Vertex sStart = subjectEdges[si].start;
                Vertex sEnd = subjectEdges[si].end;
                Vector2 s0 = sStart.Position;
                Vector2 s1 = sEnd.Position;

                for (int ci = 0; ci < clipEdges.Count; ci++)
                {
                    Vertex cStart = clipEdges[ci].start;
                    Vertex cEnd = clipEdges[ci].end;
                    Vector2 c0 = cStart.Position;
                    Vector2 c1 = cEnd.Position;

                    if (!TryIntersectSegments(s0, s1, c0, c1, out Vector2 ip, out float alphaS, out float alphaC)) continue;

                    if (alphaS <= Epsilon || alphaS >= 1f - Epsilon ||
                        alphaC <= Epsilon || alphaC >= 1f - Epsilon) continue;

                    var sInt = new Vertex(ip) { IsIntersection = true, Alpha = alphaS };
                    var cInt = new Vertex(ip) { IsIntersection = true, Alpha = alphaC };
                    sInt.Neighbor = cInt;
                    cInt.Neighbor = sInt;
                    InsertBetween(sStart, sEnd, sInt);
                    InsertBetween(cStart, cEnd, cInt);
                    count++;
                }
            }

            return count;
        }

        private static List<(Vertex start, Vertex end)> CollectSourceEdges(Vertex head)
        {
            var edges = new List<(Vertex, Vertex)>();
            Vertex v = head;
            do
            {
                edges.Add((v, v.Next));
                v = v.Next;
            } while (v != head);

            return edges;
        }

        private static void InsertBetween(Vertex start, Vertex end, Vertex node)
        {
            Vertex cursor = start;
            while (cursor.Next != end && cursor.Next.IsIntersection && cursor.Next.Alpha <= node.Alpha + Epsilon)
                cursor = cursor.Next;

            node.Next = cursor.Next;
            node.Prev = cursor;
            cursor.Next.Prev = node;
            cursor.Next = node;
        }

        private static void MarkEntries(Vertex head, Polygon2D other, bool invert)
        {
            bool inside = PointInPolygon(head.Position, other);
            if (invert)
            {
                inside = !inside;
            }

            Vertex v = head.Next;
            while (v != head)
            {
                if (v.IsIntersection)
                {
                    v.IsEntry = !inside;
                    inside = !inside;
                }

                v = v.Next;
            }
        }

        private static bool TraceResult(Vertex subjectHead, ClipOp op, List<Polygon2D> results)
        {
            Vertex probe = subjectHead;
            do
            {
                if (probe.IsIntersection && !probe.Processed && IsValidStart(probe, op))
                {
                    var poly = new Polygon2D();
                    Vertex current = probe;
                    int guard = 0;
                    const int maxGuard = 100000;

                    AddUnique(poly, current.Position);

                    do
                    {
                        current.Processed = true;
                        if (current.Neighbor != null)
                        {
                            current.Neighbor.Processed = true;
                        }

                        bool walkForward = op == ClipOp.Union ? !current.IsEntry : current.IsEntry;
                        Vertex next = walkForward ? current.Next : current.Prev;

                        while (!next.IsIntersection && guard < maxGuard)
                        {
                            AddUnique(poly, next.Position);
                            next.Processed = true;
                            next = walkForward ? next.Next : next.Prev;
                            guard++;
                        }

                        AddUnique(poly, next.Position);
                        current = next.Neighbor;
                        guard++;
                    } 
                    while (current != null && current != probe && !current.Processed && guard < maxGuard);

                    poly.Sanitize();
                    poly.EnsureClockwise();
                    if (poly.IsValid && Mathf.Abs(poly.CalculateSignedArea()) > 1e-5f)
                    {
                        results.Add(poly);
                    }
                }

                probe = probe.Next;
            } 
            while (probe != subjectHead);

            return results.Count > 0;
        }

        private static bool IsValidStart(Vertex v, ClipOp op) =>
            op == ClipOp.Union ? !v.IsEntry : v.IsEntry;

        private static void AddUnique(Polygon2D poly, Vector2 point)
        {
            if (poly.Count > 0 && (poly.Points[poly.Count - 1] - point).sqrMagnitude < Epsilon * Epsilon) return;
            poly.Points.Add(point);
        }

        private static bool PointInPolygon(Vector2 point, Polygon2D polygon)
        {
            bool inside = false;
            int n = polygon.Count;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                Vector2 pi = polygon.Points[i];
                Vector2 pj = polygon.Points[j];
                bool intersect = ((pi.y > point.y) != (pj.y > point.y)) &&
                                 (point.x < (pj.x - pi.x) * (point.y - pi.y) / ((pj.y - pi.y) + Epsilon) + pi.x);
                if (intersect)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        private static bool TryIntersectSegments(
            Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1,
            out Vector2 intersection, out float alphaA, out float alphaB)
        {
            intersection = default;
            alphaA = 0f;
            alphaB = 0f;

            Vector2 r = a1 - a0;
            Vector2 s = b1 - b0;
            float den = Cross(r, s);
            if (Mathf.Abs(den) < Epsilon) return false;

            Vector2 qp = b0 - a0;
            alphaA = Cross(qp, s) / den;
            alphaB = Cross(qp, r) / den;

            if (alphaA < -Epsilon || alphaA > 1f + Epsilon || alphaB < -Epsilon || alphaB > 1f + Epsilon) return false;

            alphaA = Mathf.Clamp01(alphaA);
            alphaB = Mathf.Clamp01(alphaB);
            intersection = a0 + r * alphaA;
            return true;
        }

        private static float Cross(Vector2 u, Vector2 v) => u.x * v.y - u.y * v.x;

        private sealed class Vertex
        {
            public Vector2 Position;
            public Vertex Next;
            public Vertex Prev;
            public Vertex Neighbor;
            public bool IsIntersection;
            public bool IsEntry;
            public bool Processed;
            public float Alpha;

            public Vertex(Vector2 position) => Position = position;
        }
    }
}

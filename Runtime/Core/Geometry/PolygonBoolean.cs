using System.Collections.Generic;
using UnityEngine;

namespace Galaretka.ShapeGraph.Core.Geometry
{
    public static class PolygonBoolean
    {
        public static void CollectPolygons(IShape2D shape, List<Polygon2D> destination)
        {
            if (destination == null || shape == null) return;

            if (shape is MultiPolygon2D multi)
            {
                IReadOnlyList<Polygon2D> polys = multi.Polygons;
                for (int i = 0; i < polys.Count; i++)
                {
                    if (polys[i] != null && polys[i].IsValid)
                    {
                        destination.Add(polys[i]);
                    }
                }
                return;
            }

            Polygon2D poly = shape as Polygon2D ?? shape.ToPolygon();
            if (poly != null && poly.IsValid)
            {
                destination.Add(poly);
            }
        }

        public static bool TryUnion(IShape2D a, IShape2D b, out IShape2D result)
        {
            result = null;
            if (a == null && b == null) return false;
            if (a == null)
            {
                result = b.CloneShape();
                return result != null;
            }
            if (b == null)
            {
                result = a.CloneShape();
                return result != null;
            }

            var polys = new List<Polygon2D>(8);
            CollectPolygons(a, polys);
            CollectPolygons(b, polys);
            if (polys.Count == 0) return false;

            try
            {
                List<Polygon2D> acc = new List<Polygon2D> { polys[0].Clone() };
                for (int i = 1; i < polys.Count; i++)
                {
                    if (!TryUnionInto(acc, polys[i])) return false;
                }

                result = MultiPolygon2D.FromPolygons(acc);
                return result != null;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ShapeGraph] PolygonBoolean.Union failed: {ex.Message}");
                return false;
            }
        }

        public static bool TryUnion(Polygon2D a, Polygon2D b, out IShape2D result) =>
            TryUnion((IShape2D)a, b, out result);

        public static bool TryDifference(IShape2D subject, IShape2D clip, out IShape2D result)
        {
            result = null;
            if (subject == null) return false;
            if (clip == null)
            {
                result = subject.CloneShape();
                return result != null;
            }

            var subjects = new List<Polygon2D>(4);
            var clips = new List<Polygon2D>(4);
            CollectPolygons(subject, subjects);
            CollectPolygons(clip, clips);
            if (subjects.Count == 0) return false;
            if (clips.Count == 0)
            {
                result = subject.CloneShape();
                return true;
            }

            try
            {
                var leftovers = new List<Polygon2D>();
                for (int s = 0; s < subjects.Count; s++)
                {
                    var current = new List<Polygon2D> { subjects[s].Clone() };
                    for (int c = 0; c < clips.Count; c++)
                    {
                        var next = new List<Polygon2D>();
                        for (int i = 0; i < current.Count; i++)
                        {
                            if (PolygonClipper2D.TryClip(
                                    current[i],
                                    clips[c],
                                    PolygonClipper2D.ClipOp.Difference,
                                    out List<Polygon2D> piece) &&
                                piece.Count > 0)
                            {
                                next.AddRange(piece);
                            }
                        }
                        current = next;
                        if (current.Count == 0) break;
                    }

                    leftovers.AddRange(current);
                }

                if (leftovers.Count == 0) return false;

                result = MultiPolygon2D.FromPolygons(leftovers);
                return result != null;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ShapeGraph] PolygonBoolean.Difference failed: {ex.Message}");
                return false;
            }
        }

        public static bool TryDifference(Polygon2D subject, Polygon2D clip, out IShape2D result) =>
            TryDifference((IShape2D)subject, clip, out result);

        internal static bool TryIntersect(Polygon2D a, Polygon2D b, out IShape2D result)
        {
            result = null;
            Debug.LogWarning("[ShapeGraph] PolygonBoolean.Intersect is not supported in this release.");
            return false;
        }

        private static bool TryUnionInto(List<Polygon2D> acc, Polygon2D next)
        {
            var pending = new List<Polygon2D> { next.Clone() };
            var kept = new List<Polygon2D>();

            for (int i = 0; i < acc.Count; i++)
            {
                Polygon2D left = acc[i];
                bool merged = false;

                for (int p = 0; p < pending.Count; p++)
                {
                    if (!PolygonClipper2D.TryClip(
                            left,
                            pending[p],
                            PolygonClipper2D.ClipOp.Union,
                            out List<Polygon2D> united))
                        continue;

                    pending.RemoveAt(p);
                    pending.AddRange(united);
                    merged = true;
                    break;
                }

                if (!merged) 
                {
                    kept.Add(left);
                }
            }

            acc.Clear();
            acc.AddRange(kept);
            acc.AddRange(pending);
            return acc.Count > 0;
        }
    }
}

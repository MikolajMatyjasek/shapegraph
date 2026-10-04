using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using UnityEngine;

namespace Galaretka.ShapeGraph.Meshing
{
    public static class MeshOps
    {
        public static ShapeMesh2D BuildFill(Polygon2D polygon, Color32 fillColor)
        {
            if (polygon == null || !polygon.IsValid) return null;

            return BuildFill(new[] { polygon }, fillColor);
        }

        public static ShapeMesh2D BuildFill(IReadOnlyList<Polygon2D> polygons, Color32 fillColor)
        {
            if (polygons == null || polygons.Count == 0) return null;

            var parts = new List<ShapeMesh2D>(polygons.Count);
            for (int i = 0; i < polygons.Count; i++)
            {
                ShapeMesh2D part = BuildFillSingle(polygons[i], fillColor);
                if (part != null && part.IsValid)
                {
                    parts.Add(part);
                }
            }

            return Concatenate(parts);
        }

        public static ShapeMesh2D AppendOutline(
            ShapeMesh2D source,
            float thickness,
            Color32 outlineColor,
            float maxMiterLimit = 3f)
        {
            if (source == null || !source.IsValid) return null;

            if (thickness <= 0.0001f) return source.Clone();

            Vector2[][] contours = source.GetContours();
            if (contours.Length == 0)
            {
                Debug.LogWarning("[ShapeGraph] AppendOutline requires a valid Contour (produce mesh via Fill first).");
                return source.Clone();
            }

            ShapeMesh2D result = source.Clone();
            var outerContours = new List<Vector2[]>(contours.Length);

            for (int c = 0; c < contours.Length; c++)
            {
                Vector2[] contour = contours[c];
                if (contour == null || contour.Length < 3) continue;

                if (!TryAppendOutlineRegion(result, contour, thickness, outlineColor, maxMiterLimit, out Vector2[] outer)) continue;

                outerContours.Add(outer);
            }

            if (outerContours.Count == 0) return source.Clone();

            result.Contours = outerContours.ToArray();
            result.Contour = result.Contours[0];
            return result;
        }

        public static ShapeMesh2D Transform(ShapeMesh2D source, in AffineTransform2D transform)
        {
            if (source == null || !source.IsValid) return null;

            ShapeMesh2D result = source.Clone();
            if (transform.IsIdentity()) return result;

            for (int i = 0; i < result.Vertices.Length; i++)
            {
                Vector3 v = result.Vertices[i];
                Vector2 p = transform.Apply(new Vector2(v.x, v.y));
                result.Vertices[i] = new Vector3(p.x, p.y, v.z);
            }

            if (result.Contour != null)
            {
                for (int i = 0; i < result.Contour.Length; i++)
                {
                    result.Contour[i] = transform.Apply(result.Contour[i]);
                }
            }

            if (result.Contours != null)
            {
                for (int c = 0; c < result.Contours.Length; c++)
                {
                    Vector2[] contour = result.Contours[c];
                    if (contour == null) continue;
                    for (int i = 0; i < contour.Length; i++)
                    {
                        contour[i] = transform.Apply(contour[i]);
                    }
                }
            }

            return result;
        }

        private static ShapeMesh2D BuildFillSingle(Polygon2D polygon, Color32 fillColor)
        {
            if (polygon == null || !polygon.IsValid) return null;

            Polygon2D working = polygon.Clone();
            working.Sanitize();
            working.EnsureClockwise();
            if (!working.IsValid) return null;

            int n = working.Count;
            int[] fillIndices = EarClippingTriangulator.Triangulate(working.Points);
            if (fillIndices == null || fillIndices.Length < 3) return null;

            var vertices = new Vector3[n];
            var colors = new Color32[n];
            var contour = new Vector2[n];

            for (int i = 0; i < n; i++)
            {
                Vector2 p = working.Points[i];
                vertices[i] = new Vector3(p.x, p.y, 0f);
                colors[i] = fillColor;
                contour[i] = p;
            }

            return new ShapeMesh2D
            {
                Vertices = vertices,
                Triangles = fillIndices,
                Colors = colors,
                Contour = contour,
                Contours = new[] { contour }
            };
        }

        public static ShapeMesh2D Concatenate(IReadOnlyList<ShapeMesh2D> parts)
        {
            if (parts == null || parts.Count == 0) return null;
            if (parts.Count == 1) return parts[0];

            int vertCount = 0;
            int triCount = 0;
            var contours = new List<Vector2[]>(parts.Count);

            for (int i = 0; i < parts.Count; i++)
            {
                ShapeMesh2D p = parts[i];
                vertCount += p.Vertices.Length;
                triCount += p.Triangles.Length;
                Vector2[][] partContours = p.GetContours();
                for (int c = 0; c < partContours.Length; c++)
                {
                    if (partContours[c] != null && partContours[c].Length >= 3)
                    {
                        contours.Add(partContours[c]);
                    }
                }
            }

            if (vertCount < 3 || triCount < 3) return null;

            var vertices = new Vector3[vertCount];
            var colors = new Color32[vertCount];
            var triangles = new int[triCount];
            int vOffset = 0;
            int tOffset = 0;

            for (int i = 0; i < parts.Count; i++)
            {
                ShapeMesh2D p = parts[i];
                System.Array.Copy(p.Vertices, 0, vertices, vOffset, p.Vertices.Length);
                System.Array.Copy(p.Colors, 0, colors, vOffset, p.Colors.Length);
                for (int t = 0; t < p.Triangles.Length; t++)
                {
                    triangles[tOffset + t] = p.Triangles[t] + vOffset;
                }
                tOffset += p.Triangles.Length;
                vOffset += p.Vertices.Length;
            }

            Vector2[][] contourArr = contours.ToArray();
            return new ShapeMesh2D
            {
                Vertices = vertices,
                Triangles = triangles,
                Colors = colors,
                Contour = contourArr.Length > 0 ? contourArr[0] : null,
                Contours = contourArr
            };
        }

        private static bool TryAppendOutlineRegion(
            ShapeMesh2D mesh,
            Vector2[] contour,
            float thickness,
            Color32 outlineColor,
            float maxMiterLimit,
            out Vector2[] outerContour)
        {
            outerContour = null;

            var contourPoly = new Polygon2D(contour);
            contourPoly.Sanitize();
            contourPoly.EnsureClockwise();
            if (!contourPoly.IsValid) return false;

            int n = contourPoly.Count;
            Vector2[] miters = contourPoly.CalculateMiterNormals(maxMiterLimit);

            int srcVertCount = mesh.Vertices.Length;
            int srcTriCount = mesh.Triangles.Length;
            int outlineVertCount = n * 2;
            int outlineTriCount = n * 6;

            var vertices = new Vector3[srcVertCount + outlineVertCount];
            var colors = new Color32[srcVertCount + outlineVertCount];
            var triangles = new int[srcTriCount + outlineTriCount];
            outerContour = new Vector2[n];

            System.Array.Copy(mesh.Vertices, 0, vertices, 0, srcVertCount);
            System.Array.Copy(mesh.Colors, 0, colors, 0, srcVertCount);
            System.Array.Copy(mesh.Triangles, 0, triangles, 0, srcTriCount);

            int outlineStart = srcVertCount;
            int triOffset = srcTriCount;

            for (int i = 0; i < n; i++)
            {
                Vector2 inner = contourPoly.Points[i];
                Vector2 outer = inner + miters[i] * thickness;

                int innerIdx = outlineStart + (i * 2);
                int outerIdx = innerIdx + 1;

                vertices[innerIdx] = new Vector3(inner.x, inner.y, 0f);
                vertices[outerIdx] = new Vector3(outer.x, outer.y, 0f);
                colors[innerIdx] = outlineColor;
                colors[outerIdx] = outlineColor;
                outerContour[i] = outer;

                int nextI = (i + 1) % n;
                int nextInner = outlineStart + (nextI * 2);
                int nextOuter = nextInner + 1;

                triangles[triOffset++] = innerIdx;
                triangles[triOffset++] = outerIdx;
                triangles[triOffset++] = nextOuter;

                triangles[triOffset++] = innerIdx;
                triangles[triOffset++] = nextOuter;
                triangles[triOffset++] = nextInner;
            }

            mesh.Vertices = vertices;
            mesh.Colors = colors;
            mesh.Triangles = triangles;
            return true;
        }
    }
}

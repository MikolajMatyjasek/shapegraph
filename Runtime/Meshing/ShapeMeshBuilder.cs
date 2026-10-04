using Galaretka.ShapeGraph.Core;
using UnityEngine;

namespace Galaretka.ShapeGraph.Meshing
{
    public static class ShapeMeshBuilder
    {
        public static GeneratedMeshData BuildFilledPolygon(Polygon2D polygon, Color32 fillColor)
        {
            if (polygon == null || !polygon.IsValid) return GeneratedMeshData.Empty;

            polygon.EnsureClockwise();
            int n = polygon.Count;
            int[] indices = EarClippingTriangulator.Triangulate(polygon.Points);
            if (indices == null || indices.Length < 3) return GeneratedMeshData.Empty;

            var vertices = new Vector3[n];
            var colors = new Color32[n];
            var collider = new Vector2[n];

            for (int i = 0; i < n; i++)
            {
                Vector2 p = polygon.Points[i];
                vertices[i] = new Vector3(p.x, p.y, 0f);
                colors[i] = fillColor;
                collider[i] = p;
            }

            return new GeneratedMeshData
            {
                Vertices = vertices,
                Triangles = indices,
                Colors = colors,
                ColliderBoundary = collider
            };
        }
    }
}

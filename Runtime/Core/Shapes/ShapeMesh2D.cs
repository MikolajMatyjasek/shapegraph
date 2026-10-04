using System;
using UnityEngine;

namespace Galaretka.ShapeGraph.Core
{
    public sealed class ShapeMesh2D
    {
        public Vector3[] Vertices;
        public int[] Triangles;
        public Color32[] Colors;

        public Vector2[] Contour;

        public Vector2[][] Contours;

        public bool IsValid =>
            Vertices != null && Vertices.Length >= 3 &&
            Triangles != null && Triangles.Length >= 3 &&
            Colors != null && Colors.Length == Vertices.Length;

        public Vector2[][] GetContours()
        {
            if (Contours != null && Contours.Length > 0) return Contours;

            if (Contour != null && Contour.Length >= 3) return new[] { Contour };

            return Array.Empty<Vector2[]>();
        }

        public ShapeMesh2D Clone()
        {
            Vector2[][] clonedContours = null;
            if (Contours != null)
            {
                clonedContours = new Vector2[Contours.Length][];
                for (int i = 0; i < Contours.Length; i++)
                {
                    clonedContours[i] = Contours[i] != null ? (Vector2[])Contours[i].Clone() : null;
                }
            }

            return new ShapeMesh2D
            {
                Vertices = Vertices != null ? (Vector3[])Vertices.Clone() : null,
                Triangles = Triangles != null ? (int[])Triangles.Clone() : null,
                Colors = Colors != null ? (Color32[])Colors.Clone() : null,
                Contour = Contour != null ? (Vector2[])Contour.Clone() : null,
                Contours = clonedContours
            };
        }

        public GeneratedMeshData ToGeneratedMeshData()
        {
            if (!IsValid) return GeneratedMeshData.Empty;

            Vector2[][] contours = GetContours();
            Vector2[][] colliderPaths = null;
            if (contours.Length > 0)
            {
                colliderPaths = new Vector2[contours.Length][];
                for (int i = 0; i < contours.Length; i++)
                {
                    colliderPaths[i] = contours[i] != null ? (Vector2[])contours[i].Clone() : Array.Empty<Vector2>();
                }
            }

            return new GeneratedMeshData
            {
                Vertices = (Vector3[])Vertices.Clone(),
                Triangles = (int[])Triangles.Clone(),
                Colors = (Color32[])Colors.Clone(),
                ColliderBoundary = Contour != null && Contour.Length >= 3
                    ? (Vector2[])Contour.Clone()
                    : (colliderPaths != null && colliderPaths.Length > 0 ? (Vector2[])colliderPaths[0].Clone() : Array.Empty<Vector2>()),
                ColliderPaths = colliderPaths
            };
        }
    }
}

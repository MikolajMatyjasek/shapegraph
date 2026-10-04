using UnityEngine;

namespace Galaretka.ShapeGraph.Core
{
    public struct GeneratedMeshData
    {
        public Vector3[] Vertices;
        public int[] Triangles;
        public Color32[] Colors;
        public Vector2[] ColliderBoundary;

        public static GeneratedMeshData Empty => new()
        {
            Vertices = new Vector3[0],
            Triangles = new int[0],
            Colors = new Color32[0],
            ColliderBoundary = new Vector2[0]
        };

        public Mesh ToMesh(string meshName = "ShapeGraph_Mesh")
        {
            var mesh = new Mesh { name = meshName };
            mesh.vertices = Vertices;
            mesh.triangles = Triangles;
            mesh.colors32 = Colors;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}

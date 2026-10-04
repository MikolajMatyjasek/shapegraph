using Galaretka.ShapeGraph.Core;
using UnityEngine;

namespace Galaretka.ShapeGraph.Meshing
{
    public static class ShapeMeshBuilder
    {
        public static GeneratedMeshData BuildFilledPolygon(Polygon2D polygon, Color32 fillColor)
        {
            ShapeMesh2D mesh = MeshOps.BuildFill(polygon, fillColor);
            return mesh != null ? mesh.ToGeneratedMeshData() : GeneratedMeshData.Empty;
        }
    }
}

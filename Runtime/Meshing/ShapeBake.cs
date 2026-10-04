using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using UnityEngine;

namespace Galaretka.ShapeGraph.Meshing
{
    public static class ShapeBake
    {
        private static readonly List<Polygon2D> PolygonBuffer = new(8);

        public static GeneratedMeshData BakeRegion(ShapeRegion2D region)
        {
            if (region?.Geometry == null) return GeneratedMeshData.Empty;

            PolygonBuffer.Clear();
            ShapeGeometry.CollectPolygons(region.Geometry, PolygonBuffer);
            if (PolygonBuffer.Count == 0) return GeneratedMeshData.Empty;

            ShapeMesh2D mesh = MeshOps.BuildFill(PolygonBuffer, region.FillColor);
            if (mesh == null || !mesh.IsValid) return GeneratedMeshData.Empty;

            if (region.HasOutline && region.OutlineThickness > 0.0001f)
            {
                mesh = MeshOps.AppendOutline(mesh, region.OutlineThickness, region.OutlineColor);
                if (mesh == null || !mesh.IsValid) return GeneratedMeshData.Empty;
            }

            return mesh.ToGeneratedMeshData();
        }

        public static GeneratedMeshData BakePicture(ShapePicture2D picture)
        {
            if (picture == null || picture.Layers.Count == 0) return GeneratedMeshData.Empty;

            var parts = new List<ShapeMesh2D>(picture.Layers.Count);
            for (int i = 0; i < picture.Layers.Count; i++)
            {
                ShapeRegion2D layer = picture.Layers[i];
                GeneratedMeshData baked = BakeRegion(layer);
                if (baked.Vertices == null || baked.Vertices.Length < 3) continue;

                parts.Add(new ShapeMesh2D
                {
                    Vertices = baked.Vertices,
                    Triangles = baked.Triangles,
                    Colors = baked.Colors,
                    Contour = baked.ColliderBoundary,
                    Contours = baked.ColliderPaths
                });
            }

            ShapeMesh2D combined = MeshOps.Concatenate(parts);
            return combined != null ? combined.ToGeneratedMeshData() : GeneratedMeshData.Empty;
        }
    }
}

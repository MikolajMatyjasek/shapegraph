using System.Collections.Generic;
using UnityEngine;

namespace Galaretka.ShapeGraph.Core.Geometry
{
    public static class ShapeGeometry
    {
        public static bool IsBareGeometry(IShape2D shape) =>
            shape != null && shape is not ShapeRegion2D;

        public static IShape2D RequireBare(IShape2D shape, string context)
        {
            if (shape == null) return null;

            if (shape is ShapeRegion2D)
            {
                Debug.LogWarning(
                    $"[ShapeGraph] {context}: styled region cannot enter a geometry op. " +
                    "Apply boolean/modifiers before Style/Fill.");
                return null;
            }

            return shape;
        }

        public static void CollectPolygons(IShape2D shape, List<Polygon2D> destination)
        {
            if (destination == null || shape == null) return;

            if (shape is ShapeRegion2D region)
            {
                PolygonBoolean.CollectPolygons(region.Geometry, destination);
                return;
            }

            PolygonBoolean.CollectPolygons(shape, destination);
        }
    }
}

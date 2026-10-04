using UnityEngine;

namespace Galaretka.ShapeGraph.Core
{
    public interface IShape2D
    {
        Bounds GetBounds();

        IShape2D CloneShape();

        Polygon2D ToPolygon();
    }
}

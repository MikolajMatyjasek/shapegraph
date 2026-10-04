using UnityEngine;

namespace Galaretka.ShapeGraph.Core.Geometry
{
    public interface IVertexDisplacement
    {
        Vector2 Displace(int index, Vector2 position, Vector2 outwardNormal);
    }
}

using Galaretka.ShapeGraph.Core.Geometry;

namespace Galaretka.ShapeGraph.Core
{
    public interface IAnalyticTransformable2D : IShape2D
    {
        IShape2D TransformAnalytic(in AffineTransform2D transform);
    }
}

namespace Galaretka.ShapeGraph.Core
{
    public interface IAnalyticInflatable2D : IShape2D
    {
        IShape2D InflateAnalytic(float factor);
    }
}

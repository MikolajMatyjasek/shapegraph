namespace Galaretka.ShapeGraph.Core
{
    public interface IAnalyticOffsettable2D : IShape2D
    {
        IShape2D OffsetAnalytic(float distance);
    }
}

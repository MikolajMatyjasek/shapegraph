namespace Galaretka.ShapeGraph.Core
{
    /// <summary>
    /// Shape that supports isotropic inflate about its natural center.
    /// </summary>
    public interface IAnalyticInflatable2D : IShape2D
    {
        IShape2D InflateAnalytic(float factor);
    }
}

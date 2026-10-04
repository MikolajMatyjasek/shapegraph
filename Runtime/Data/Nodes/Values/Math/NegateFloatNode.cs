using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Typing;

namespace Galaretka.ShapeGraph.Data.Nodes.Values.Math
{
    [NodeMenu("Values/Math/Negate")]
    public sealed class NegateFloatNode : UnaryFloatMathNode
    {
        protected override float Evaluate(float input) => -input;
    }
}

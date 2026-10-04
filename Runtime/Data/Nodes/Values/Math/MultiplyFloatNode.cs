using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Typing;

namespace Galaretka.ShapeGraph.Data.Nodes.Values.Math
{
    [NodeMenu("Values/Math/Multiply")]
    public sealed class MultiplyFloatNode : BinaryFloatMathNode
    {
        protected override float Evaluate(float aValue, float bValue) => aValue * bValue;
    }
}

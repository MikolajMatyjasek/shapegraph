using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Typing;

namespace Galaretka.ShapeGraph.Data.Nodes.Values.Math
{
    [NodeMenu("Values/Math/Subtract")]
    public sealed class SubtractFloatNode : BinaryFloatMathNode
    {
        protected override float Evaluate(float aValue, float bValue) => aValue - bValue;
    }
}

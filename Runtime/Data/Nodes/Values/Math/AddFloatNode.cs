using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Typing;

namespace Galaretka.ShapeGraph.Data.Nodes.Values.Math
{
    [NodeMenu("Values/Math/Add")]
    public sealed class AddFloatNode : BinaryFloatMathNode
    {
        protected override float Evaluate(float aValue, float bValue) => aValue + bValue;
    }
}

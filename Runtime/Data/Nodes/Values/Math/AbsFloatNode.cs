using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Values.Math
{
    [NodeMenu("Values/Math/Abs")]
    public sealed class AbsFloatNode : UnaryFloatMathNode
    {
        protected override float Evaluate(float input) => Mathf.Abs(input);
    }
}

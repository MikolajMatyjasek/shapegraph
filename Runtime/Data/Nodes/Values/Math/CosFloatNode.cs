using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Values.Math
{
    [NodeMenu("Values/Math/Cos")]
    public sealed class CosFloatNode : UnaryFloatMathNode
    {
        protected override float Evaluate(float input) => Mathf.Cos(input);
    }
}

using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Values.Math
{
    [NodeMenu("Values/Math/Sin")]
    public sealed class SinFloatNode : UnaryFloatMathNode
    {
        protected override float Evaluate(float input) => Mathf.Sin(input);
    }
}

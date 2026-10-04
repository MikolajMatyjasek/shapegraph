using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Values.Math
{
    [NodeMenu("Values/Math/Max")]
    public sealed class MaxFloatNode : BinaryFloatMathNode
    {
        protected override float Evaluate(float aValue, float bValue) => Mathf.Max(aValue, bValue);
    }
}

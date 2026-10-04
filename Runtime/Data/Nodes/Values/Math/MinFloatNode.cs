using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Values.Math
{
    [NodeMenu("Values/Math/Min")]
    public sealed class MinFloatNode : BinaryFloatMathNode
    {
        protected override float Evaluate(float aValue, float bValue) => Mathf.Min(aValue, bValue);
    }
}

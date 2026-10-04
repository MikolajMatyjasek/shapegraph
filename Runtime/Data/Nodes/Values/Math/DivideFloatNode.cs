using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Values.Math
{
    [NodeMenu("Values/Math/Divide")]
    public sealed class DivideFloatNode : BinaryFloatMathNode
    {
        protected override float Evaluate(float aValue, float bValue)
        {
            if (Mathf.Abs(bValue) < 1e-8f)
            {
                Debug.LogWarning("[ShapeGraph] Divide by zero; returning 0.");
                return 0f;
            }

            return aValue / bValue;
        }
    }
}

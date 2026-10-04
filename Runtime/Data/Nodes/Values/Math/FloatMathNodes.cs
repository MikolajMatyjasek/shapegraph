using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Values.Math
{
    [NodeMenu("Values/Math/Add")]
    public sealed class AddFloatNode : BinaryFloatMathNode
    {
        protected override float Evaluate(float aValue, float bValue) => aValue + bValue;
    }

    [NodeMenu("Values/Math/Subtract")]
    public sealed class SubtractFloatNode : BinaryFloatMathNode
    {
        protected override float Evaluate(float aValue, float bValue) => aValue - bValue;
    }

    [NodeMenu("Values/Math/Multiply")]
    public sealed class MultiplyFloatNode : BinaryFloatMathNode
    {
        protected override float Evaluate(float aValue, float bValue) => aValue * bValue;
    }

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

    [NodeMenu("Values/Math/Min")]
    public sealed class MinFloatNode : BinaryFloatMathNode
    {
        protected override float Evaluate(float aValue, float bValue) => Mathf.Min(aValue, bValue);
    }

    [NodeMenu("Values/Math/Max")]
    public sealed class MaxFloatNode : BinaryFloatMathNode
    {
        protected override float Evaluate(float aValue, float bValue) => Mathf.Max(aValue, bValue);
    }

    [NodeMenu("Values/Math/Negate")]
    public sealed class NegateFloatNode : UnaryFloatMathNode
    {
        protected override float Evaluate(float input) => -input;
    }

    [NodeMenu("Values/Math/Abs")]
    public sealed class AbsFloatNode : UnaryFloatMathNode
    {
        protected override float Evaluate(float input) => Mathf.Abs(input);
    }

    [NodeMenu("Values/Math/Sin")]
    public sealed class SinFloatNode : UnaryFloatMathNode
    {
        protected override float Evaluate(float input) => Mathf.Sin(input);
    }

    [NodeMenu("Values/Math/Cos")]
    public sealed class CosFloatNode : UnaryFloatMathNode
    {
        protected override float Evaluate(float input) => Mathf.Cos(input);
    }

    [NodeMenu("Values/Math/Clamp")]
    public sealed class ClampFloatNode : ShapeNode
    {
        public static readonly PortId ValueIn = new("Value");
        public static readonly PortId MinIn = new("Min");
        public static readonly PortId MaxIn = new("Max");
        public static readonly PortId ValueOut = new("Value Out");

        [SerializeField] 
        private float value;
        [SerializeField] 
        private float min;
        [SerializeField] 
        private float max = 1f;

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(ValueIn, "Value", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(MinIn, "Min", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(MaxIn, "Max", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(ValueOut, "Value Out", PortDirection.Output, typeof(float)));
        }

        protected override ParameterValue ComputeValue(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != ValueOut) return default;
            float v = ResolveFloat(ValueIn, context, graph, value);
            float lo = ResolveFloat(MinIn, context, graph, min);
            float hi = ResolveFloat(MaxIn, context, graph, max);
            return ParameterValue.FromFloat(Mathf.Clamp(v, Mathf.Min(lo, hi), Mathf.Max(lo, hi)));
        }
    }

    [NodeMenu("Values/Math/Lerp")]
    public sealed class LerpFloatNode : ShapeNode
    {
        public static readonly PortId AIn = new("A");
        public static readonly PortId BIn = new("B");
        public static readonly PortId TIn = new("T");
        public static readonly PortId ValueOut = new("Value Out");

        [SerializeField] 
        private float a;
        [SerializeField] 
        private float b = 1f;
        [SerializeField] 
        private float t;

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(AIn, "A", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(BIn, "B", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(TIn, "T", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(ValueOut, "Value Out", PortDirection.Output, typeof(float)));
        }

        protected override ParameterValue ComputeValue(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != ValueOut) return default;
            float ra = ResolveFloat(AIn, context, graph, a);
            float rb = ResolveFloat(BIn, context, graph, b);
            float rt = ResolveFloat(TIn, context, graph, t);
            return ParameterValue.FromFloat(Mathf.Lerp(ra, rb, rt));
        }
    }

    [NodeMenu("Values/Math/Remap")]
    public sealed class RemapFloatNode : ShapeNode
    {
        public static readonly PortId ValueIn = new("Value");
        public static readonly PortId InMinIn = new("In Min");
        public static readonly PortId InMaxIn = new("In Max");
        public static readonly PortId OutMinIn = new("Out Min");
        public static readonly PortId OutMaxIn = new("Out Max");
        public static readonly PortId ValueOut = new("Value Out");

        [SerializeField] 
        private float value;
        [SerializeField] 
        private float inMin = -1f;
        [SerializeField] 
        private float inMax = 1f;
        [SerializeField] 
        private float outMin;
        [SerializeField] 
        private float outMax = 1f;

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(ValueIn, "Value", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(InMinIn, "In Min", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(InMaxIn, "In Max", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(OutMinIn, "Out Min", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(OutMaxIn, "Out Max", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(ValueOut, "Value Out", PortDirection.Output, typeof(float)));
        }

        protected override ParameterValue ComputeValue(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != ValueOut) return default;
            float v = ResolveFloat(ValueIn, context, graph, value);
            float i0 = ResolveFloat(InMinIn, context, graph, inMin);
            float i1 = ResolveFloat(InMaxIn, context, graph, inMax);
            float o0 = ResolveFloat(OutMinIn, context, graph, outMin);
            float o1 = ResolveFloat(OutMaxIn, context, graph, outMax);
            float denom = i1 - i0;
            float t = Mathf.Abs(denom) < 1e-8f ? 0f : (v - i0) / denom;
            return ParameterValue.FromFloat(Mathf.Lerp(o0, o1, t));
        }
    }
}

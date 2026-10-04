using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Values.Math
{
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

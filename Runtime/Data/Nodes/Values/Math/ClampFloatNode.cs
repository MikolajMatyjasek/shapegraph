using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Values.Math
{
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
}

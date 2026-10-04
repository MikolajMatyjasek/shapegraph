using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Values.Math
{
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
}

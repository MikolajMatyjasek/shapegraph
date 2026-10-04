using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Values.Vector2Nodes
{
    [NodeMenu("Values/Vector2/From Floats")]
    public sealed class FromFloatsVector2Node : ShapeNode
    {
        public static readonly PortId XIn = new("X");
        public static readonly PortId YIn = new("Y");
        public static readonly PortId ValueOut = new("Value Out");

        [SerializeField]
        private float x;
        [SerializeField]
        private float y;

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(XIn, "X", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(YIn, "Y", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(ValueOut, "Value Out", PortDirection.Output, typeof(Vector2)));
        }

        protected override ParameterValue ComputeValue(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != ValueOut) return default;
            float rx = ResolveFloat(XIn, context, graph, x);
            float ry = ResolveFloat(YIn, context, graph, y);
            return ParameterValue.FromVector2(new Vector2(rx, ry));
        }
    }
}

using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Values.Vector2Nodes
{
    [NodeMenu("Values/Vector2/Add")]
    public sealed class AddVector2Node : ShapeNode
    {
        public static readonly PortId AIn = new("A");
        public static readonly PortId BIn = new("B");
        public static readonly PortId ValueOut = new("Value Out");

        [SerializeField]
        private Vector2 a;
        [SerializeField]
        private Vector2 b;

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(AIn, "A", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(BIn, "B", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(ValueOut, "Value Out", PortDirection.Output, typeof(Vector2)));
        }

        protected override ParameterValue ComputeValue(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != ValueOut) return default;
            Vector2 ra = ResolveVector2(AIn, context, graph, a);
            Vector2 rb = ResolveVector2(BIn, context, graph, b);
            return ParameterValue.FromVector2(ra + rb);
        }
    }
}

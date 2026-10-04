using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Values.Vector2Nodes
{
    [NodeMenu("Values/Vector2/Split")]
    public sealed class SplitVector2Node : ShapeNode
    {
        public static readonly PortId VectorIn = new("Vector");
        public static readonly PortId XOut = new("X");
        public static readonly PortId YOut = new("Y");

        [SerializeField]
        private Vector2 vector;

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(VectorIn, "Vector", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(XOut, "X", PortDirection.Output, typeof(float)));
            ports.Add(new NodePort(YOut, "Y", PortDirection.Output, typeof(float)));
        }

        protected override ParameterValue ComputeValue(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            Vector2 v = ResolveVector2(VectorIn, context, graph, vector);
            if (portId == XOut) return ParameterValue.FromFloat(v.x);
            if (portId == YOut) return ParameterValue.FromFloat(v.y);
            return default;
        }
    }
}

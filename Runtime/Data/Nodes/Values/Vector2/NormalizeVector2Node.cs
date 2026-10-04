using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Values.Vector2Nodes
{
    [NodeMenu("Values/Vector2/Normalize")]
    public sealed class NormalizeVector2Node : ShapeNode
    {
        public static readonly PortId VectorIn = new("Vector");
        public static readonly PortId ValueOut = new("Value Out");

        [SerializeField]
        private Vector2 vector = Vector2.right;

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(VectorIn, "Vector", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(ValueOut, "Value Out", PortDirection.Output, typeof(Vector2)));
        }

        protected override ParameterValue ComputeValue(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != ValueOut) return default;
            Vector2 v = ResolveVector2(VectorIn, context, graph, vector);
            if (v.sqrMagnitude < 1e-12f)
            {
                return ParameterValue.FromVector2(Vector2.zero);
            }
            return ParameterValue.FromVector2(v.normalized);
        }
    }
}

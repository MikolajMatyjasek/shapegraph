using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Values.Vector2Nodes
{
    [NodeMenu("Values/Vector2/Scale")]
    public sealed class ScaleVector2Node : ShapeNode
    {
        public static readonly PortId VectorIn = new("Vector");
        public static readonly PortId ScaleIn = new("Scale");
        public static readonly PortId ValueOut = new("Value Out");

        [SerializeField]
        private Vector2 vector = Vector2.one;
        [SerializeField]
        private float scale = 1f;

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(VectorIn, "Vector", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(ScaleIn, "Scale", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(ValueOut, "Value Out", PortDirection.Output, typeof(Vector2)));
        }

        protected override ParameterValue ComputeValue(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != ValueOut) return default;
            Vector2 v = ResolveVector2(VectorIn, context, graph, vector);
            float s = ResolveFloat(ScaleIn, context, graph, scale);
            return ParameterValue.FromVector2(v * s);
        }
    }
}

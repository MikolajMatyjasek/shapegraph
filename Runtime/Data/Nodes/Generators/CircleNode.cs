using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Generators
{
    [NodeMenu("Generators/Circle")]
    public sealed class CircleNode : ShapeGeneratorNode
    {
        public static readonly PortId CenterIn = new("Center");
        public static readonly PortId RadiusIn = new("Radius");
        public static readonly PortId SegmentsIn = new("Segments");

        [SerializeField] 
        private Vector2 center = Vector2.zero;
        [SerializeField] 
        private float radius = 0.5f;
        [SerializeField] 
        private int segments = 48;

        public Vector2 Center { get => center; set => center = value; }
        public float Radius { get => radius; set => radius = value; }
        public int Segments { get => segments; set => segments = value; }

        public override void CollectPorts(List<NodePort> ports)
        {
            base.CollectPorts(ports);
            ports.Add(new NodePort(CenterIn, "Center", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(RadiusIn, "Radius", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(SegmentsIn, "Segments", PortDirection.Input, typeof(int)));
        }

        protected override IShape2D GenerateShape(ShapeContext context, ShapeGraphAsset graph)
        {
            Vector2 resolvedCenter = ResolveVector2(CenterIn, context, graph, center);
            float resolvedRadius = ShapeMath.ClampRadius(ResolveFloat(RadiusIn, context, graph, radius));
            int resolvedSegments = ShapeMath.ClampSegments(ResolveInt(SegmentsIn, context, graph, segments));
            return new Circle2D(resolvedCenter, resolvedRadius, resolvedSegments);
        }
    }
}

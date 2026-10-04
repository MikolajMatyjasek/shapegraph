using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Generators
{
    [NodeMenu("Generators/Ring")]
    public sealed class RingNode : ShapeGeneratorNode
    {
        public static readonly PortId CenterIn = new("Center");
        public static readonly PortId OuterRadiusIn = new("Outer Radius");
        public static readonly PortId InnerRadiusIn = new("Inner Radius");
        public static readonly PortId SegmentsIn = new("Segments");

        [SerializeField] 
        private Vector2 center;
        [SerializeField] 
        private float outerRadius = 0.5f;
        [SerializeField] 
        private float innerRadius = 0.28f;
        [SerializeField] 
        private int segments = 48;

        public override void CollectPorts(List<NodePort> ports)
        {
            base.CollectPorts(ports);
            ports.Add(new NodePort(CenterIn, "Center", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(OuterRadiusIn, "Outer Radius", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(InnerRadiusIn, "Inner Radius", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(SegmentsIn, "Segments", PortDirection.Input, typeof(int)));
        }

        protected override IShape2D GenerateShape(ShapeContext context, ShapeGraphAsset graph)
        {
            Vector2 c = ResolveVector2(CenterIn, context, graph, center);
            float outer = ShapeMath.ClampRadius(ResolveFloat(OuterRadiusIn, context, graph, outerRadius));
            float inner = ShapeMath.ClampRadius(ResolveFloat(InnerRadiusIn, context, graph, innerRadius));
            int segs = ShapeMath.ClampSegments(ResolveInt(SegmentsIn, context, graph, segments));

            if (inner >= outer)
            {
                inner = outer * 0.5f;
            }

            var outerShape = new Ellipse2D(c, outer, outer, 0f, segs);
            var innerShape = new Ellipse2D(c, inner, inner, 0f, segs);

            if (PolygonBoolean.TryDifference(outerShape, innerShape, out IShape2D result) && result != null) return result;

            return outerShape.ToPolygon();
        }
    }
}

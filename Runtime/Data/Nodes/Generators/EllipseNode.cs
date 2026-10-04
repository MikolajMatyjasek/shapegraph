using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Generators
{
    [NodeMenu("Generators/Ellipse")]
    public sealed class EllipseNode : ShapeGeneratorNode
    {
        public static readonly PortId CenterIn = new("Center");
        public static readonly PortId RadiusXIn = new("Radius X");
        public static readonly PortId RadiusYIn = new("Radius Y");
        public static readonly PortId RotationIn = new("Rotation");
        public static readonly PortId SegmentsIn = new("Segments");

        [SerializeField] 
        private Vector2 center;
        [SerializeField] 
        private float radiusX = 0.55f;
        [SerializeField] 
        private float radiusY = 0.32f;
        [SerializeField] 
        private float rotationDegrees;
        [SerializeField] 
        private int segments = 48;

        public Vector2 Center { get => center; set => center = value; }
        public float RadiusX { get => radiusX; set => radiusX = value; }
        public float RadiusY { get => radiusY; set => radiusY = value; }
        public float RotationDegrees { get => rotationDegrees; set => rotationDegrees = value; }
        public int Segments { get => segments; set => segments = value; }

        public override void CollectPorts(List<NodePort> ports)
        {
            base.CollectPorts(ports);
            ports.Add(new NodePort(CenterIn, "Center", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(RadiusXIn, "Radius X", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(RadiusYIn, "Radius Y", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(RotationIn, "Rotation", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(SegmentsIn, "Segments", PortDirection.Input, typeof(int)));
        }

        protected override IShape2D GenerateShape(ShapeContext context, ShapeGraphAsset graph)
        {
            Vector2 c = ResolveVector2(CenterIn, context, graph, center);
            float rx = ShapeMath.ClampRadius(ResolveFloat(RadiusXIn, context, graph, radiusX));
            float ry = ShapeMath.ClampRadius(ResolveFloat(RadiusYIn, context, graph, radiusY));
            float rot = ResolveFloat(RotationIn, context, graph, rotationDegrees);
            int segs = ShapeMath.ClampSegments(ResolveInt(SegmentsIn, context, graph, segments));
            return new Ellipse2D(c, rx, ry, rot, segs);
        }
    }
}

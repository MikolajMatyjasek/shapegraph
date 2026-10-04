using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Generators
{
    [NodeMenu("Generators/Capsule")]
    public sealed class CapsuleNode : ShapeGeneratorNode
    {
        public static readonly PortId CenterIn = new("Center");
        public static readonly PortId LengthIn = new("Length");
        public static readonly PortId RadiusIn = new("Radius");
        public static readonly PortId RotationIn = new("Rotation");
        public static readonly PortId SegmentsIn = new("Segments");

        [SerializeField] 
        private Vector2 center;
        [SerializeField] 
        private float length = 0.8f;
        [SerializeField] 
        private float radius = 0.22f;
        [SerializeField] 
        private float rotationDegrees;
        [SerializeField] 
        private int segments = 24;

        public override void CollectPorts(List<NodePort> ports)
        {
            base.CollectPorts(ports);
            ports.Add(new NodePort(CenterIn, "Center", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(LengthIn, "Length", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(RadiusIn, "Radius", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(RotationIn, "Rotation", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(SegmentsIn, "Segments", PortDirection.Input, typeof(int)));
        }

        protected override IShape2D GenerateShape(ShapeContext context, ShapeGraphAsset graph)
        {
            Vector2 c = ResolveVector2(CenterIn, context, graph, center);
            float len = Mathf.Max(0f, ResolveFloat(LengthIn, context, graph, length));
            float r = ShapeMath.ClampRadius(ResolveFloat(RadiusIn, context, graph, radius));
            float rot = ResolveFloat(RotationIn, context, graph, rotationDegrees);
            int halfSegs = Mathf.Max(4, ShapeMath.ClampSegments(ResolveInt(SegmentsIn, context, graph, segments)) / 2);

            float half = len * 0.5f;
            var poly = new Polygon2D();

            for (int i = 0; i <= halfSegs; i++)
            {
                float t = i / (float)halfSegs;
                float ang = -Mathf.PI * 0.5f + t * Mathf.PI;
                Vector2 local = new Vector2(half + Mathf.Cos(ang) * r, Mathf.Sin(ang) * r);
                poly.Points.Add(c + ShapeMath.Rotate(local, rot));
            }

            for (int i = 0; i <= halfSegs; i++)
            {
                float t = i / (float)halfSegs;
                float ang = Mathf.PI * 0.5f + t * Mathf.PI;
                Vector2 local = new Vector2(-half + Mathf.Cos(ang) * r, Mathf.Sin(ang) * r);
                poly.Points.Add(c + ShapeMath.Rotate(local, rot));
            }

            poly.EnsureClockwise();
            return poly;
        }
    }
}

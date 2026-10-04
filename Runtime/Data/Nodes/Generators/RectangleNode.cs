using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Generators
{
    [NodeMenu("Generators/Rectangle")]
    public sealed class RectangleNode : ShapeGeneratorNode
    {
        public static readonly PortId CenterIn = new("Center");
        public static readonly PortId WidthIn = new("Width");
        public static readonly PortId HeightIn = new("Height");
        public static readonly PortId RotationIn = new("Rotation");

        [SerializeField] 
        private Vector2 center;
        [SerializeField] 
        private float width = 1f;
        [SerializeField] 
        private float height = 0.6f;
        [SerializeField] 
        private float rotationDegrees;

        public Vector2 Center { get => center; set => center = value; }
        public float Width { get => width; set => width = value; }
        public float Height { get => height; set => height = value; }
        public float RotationDegrees { get => rotationDegrees; set => rotationDegrees = value; }

        public override void CollectPorts(List<NodePort> ports)
        {
            base.CollectPorts(ports);
            ports.Add(new NodePort(CenterIn, "Center", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(WidthIn, "Width", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(HeightIn, "Height", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(RotationIn, "Rotation", PortDirection.Input, typeof(float)));
        }

        protected override IShape2D GenerateShape(ShapeContext context, ShapeGraphAsset graph)
        {
            Vector2 c = ResolveVector2(CenterIn, context, graph, center);
            float w = Mathf.Max(0.0001f, ResolveFloat(WidthIn, context, graph, width));
            float h = Mathf.Max(0.0001f, ResolveFloat(HeightIn, context, graph, height));
            float rot = ResolveFloat(RotationIn, context, graph, rotationDegrees);

            float hx = w * 0.5f;
            float hy = h * 0.5f;
            var poly = new Polygon2D();
            poly.Points.Add(c + ShapeMath.Rotate(new Vector2(-hx, -hy), rot));
            poly.Points.Add(c + ShapeMath.Rotate(new Vector2(hx, -hy), rot));
            poly.Points.Add(c + ShapeMath.Rotate(new Vector2(hx, hy), rot));
            poly.Points.Add(c + ShapeMath.Rotate(new Vector2(-hx, hy), rot));
            poly.EnsureClockwise();
            return poly;
        }
    }
}

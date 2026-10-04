using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Generators
{
    [NodeMenu("Generators/Regular Polygon")]
    public sealed class RegularPolygonNode : ShapeGeneratorNode
    {
        public static readonly PortId CenterIn = new("Center");
        public static readonly PortId RadiusIn = new("Radius");
        public static readonly PortId SidesIn = new("Sides");
        public static readonly PortId RotationIn = new("Rotation");

        [SerializeField] 
        private Vector2 center = Vector2.zero;
        [SerializeField] 
        private float radius = 0.5f;
        [SerializeField] 
        private int sides = 6;
        [SerializeField] 
        private float rotationDegrees;

        public Vector2 Center { get => center; set => center = value; }
        public float Radius { get => radius; set => radius = value; }
        public int Sides { get => sides; set => sides = value; }
        public float RotationDegrees { get => rotationDegrees; set => rotationDegrees = value; }

        public override void CollectPorts(List<NodePort> ports)
        {
            base.CollectPorts(ports);
            ports.Add(new NodePort(CenterIn, "Center", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(RadiusIn, "Radius", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(SidesIn, "Sides", PortDirection.Input, typeof(int)));
            ports.Add(new NodePort(RotationIn, "Rotation", PortDirection.Input, typeof(float)));
        }

        protected override IShape2D GenerateShape(ShapeContext context, ShapeGraphAsset graph)
        {
            return new RegularPolygon2D(
                ResolveVector2(CenterIn, context, graph, center),
                ShapeMath.ClampRadius(ResolveFloat(RadiusIn, context, graph, radius)),
                ShapeMath.ClampSides(ResolveInt(SidesIn, context, graph, sides)),
                ResolveFloat(RotationIn, context, graph, rotationDegrees));
        }
    }
}

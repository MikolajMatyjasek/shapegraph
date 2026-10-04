using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Generators
{
    [NodeMenu("Generators/Star")]
    public sealed class StarNode : ShapeGeneratorNode
    {
        public static readonly PortId CenterIn = new("Center");
        public static readonly PortId OuterRadiusIn = new("Outer Radius");
        public static readonly PortId InnerRadiusIn = new("Inner Radius");
        public static readonly PortId PointsIn = new("Points");
        public static readonly PortId RotationIn = new("Rotation");

        [SerializeField] 
        private Vector2 center = Vector2.zero;
        [SerializeField] 
        private float outerRadius = 0.55f;
        [SerializeField] 
        private float innerRadius = 0.25f;
        [SerializeField] 
        private int points = 5;
        [SerializeField] 
        private float rotationDegrees = -90f;

        public Vector2 Center { get => center; set => center = value; }
        public float OuterRadius { get => outerRadius; set => outerRadius = value; }
        public float InnerRadius { get => innerRadius; set => innerRadius = value; }
        public int Points { get => points; set => points = value; }
        public float RotationDegrees { get => rotationDegrees; set => rotationDegrees = value; }

        public override void CollectPorts(List<NodePort> ports)
        {
            base.CollectPorts(ports);
            ports.Add(new NodePort(CenterIn, "Center", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(OuterRadiusIn, "Outer Radius", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(InnerRadiusIn, "Inner Radius", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(PointsIn, "Points", PortDirection.Input, typeof(int)));
            ports.Add(new NodePort(RotationIn, "Rotation", PortDirection.Input, typeof(float)));
        }

        protected override IShape2D GenerateShape(ShapeContext context, ShapeGraphAsset graph)
        {
            return new Star2D(
                ResolveVector2(CenterIn, context, graph, center),
                ShapeMath.ClampRadius(ResolveFloat(OuterRadiusIn, context, graph, outerRadius)),
                ShapeMath.ClampRadius(ResolveFloat(InnerRadiusIn, context, graph, innerRadius)),
                ShapeMath.ClampSides(ResolveInt(PointsIn, context, graph, points)),
                ResolveFloat(RotationIn, context, graph, rotationDegrees));
        }
    }
}

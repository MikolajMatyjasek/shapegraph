using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Modifiers
{
    [NodeMenu("Modifiers/Twist")]
    public sealed class TwistNode : ShapeModifierNode
    {
        public static readonly PortId AngleIn = new("Angle");
        public static readonly PortId FalloffIn = new("Falloff");

        [SerializeField] 
        private float angleDegrees = 45f;
        [SerializeField] 
        private float falloff = 1f;

        public override void CollectPorts(List<NodePort> ports)
        {
            base.CollectPorts(ports);
            ports.Add(new NodePort(AngleIn, "Angle", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(FalloffIn, "Falloff", PortDirection.Input, typeof(float)));
        }

        protected override IShape2D ModifyShape(IShape2D input, ShapeContext context, ShapeGraphAsset graph)
        {
            if (input == null) return null;

            Polygon2D poly = input as Polygon2D ?? input.ToPolygon();
            if (poly == null || !poly.IsValid) return input.CloneShape();

            float angle = ResolveFloat(AngleIn, context, graph, angleDegrees);
            float fall = Mathf.Max(0.0001f, ResolveFloat(FalloffIn, context, graph, falloff));
            Vector2 centroid = PolygonOps.Centroid(poly);

            float maxR = 0.0001f;
            for (int i = 0; i < poly.Count; i++)
            {
                maxR = Mathf.Max(maxR, (poly.Points[i] - centroid).magnitude);
            }

            Polygon2D result = poly.Clone();
            for (int i = 0; i < result.Count; i++)
            {
                Vector2 d = result.Points[i] - centroid;
                float r = d.magnitude;
                float t = Mathf.Clamp01(r / maxR);
                float twist = angle * Mathf.Pow(t, fall);
                result.Points[i] = centroid + ShapeMath.Rotate(d, twist);
            }

            result.Sanitize();
            result.EnsureClockwise();
            return result.IsValid ? result : input.CloneShape();
        }

        protected override void Modify(Polygon2D polygon, ShapeContext context, ShapeGraphAsset graph) { }
    }
}

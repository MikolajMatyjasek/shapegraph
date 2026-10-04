using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Modifiers
{
    [NodeMenu("Modifiers/Polar Displace")]
    public sealed class PolarDisplaceNode : ShapeModifierNode
    {
        public static readonly PortId AmplitudeIn = new("Amplitude");
        public static readonly PortId FrequencyIn = new("Frequency");

        [SerializeField] 
        private float amplitude = 0.04f;
        [SerializeField] 
        private float frequency = 3f;

        public override void CollectPorts(List<NodePort> ports)
        {
            base.CollectPorts(ports);
            ports.Add(new NodePort(AmplitudeIn, "Amplitude", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(FrequencyIn, "Frequency", PortDirection.Input, typeof(float)));
        }

        protected override IShape2D ModifyShape(IShape2D input, ShapeContext context, ShapeGraphAsset graph)
        {
            if (input == null) return null;

            Polygon2D poly = input as Polygon2D ?? input.ToPolygon();
            if (poly == null || !poly.IsValid) return input.CloneShape();

            float amp = ResolveFloat(AmplitudeIn, context, graph, amplitude);
            float freq = ResolveFloat(FrequencyIn, context, graph, frequency);
            Vector2 centroid = PolygonOps.Centroid(poly);

            Polygon2D result = poly.Clone();
            for (int i = 0; i < result.Count; i++)
            {
                Vector2 d = result.Points[i] - centroid;
                float r = d.magnitude;
                if (r < 1e-6f) continue;

                Vector2 dir = d / r;
                float angle = Mathf.Atan2(d.y, d.x);
                float n = Mathf.Sin(angle * freq + context.GlobalSeed * 0.01f);
                float wave = DeterministicRng.Hash01(context.GlobalSeed, Id.GetHashCode(), i) * 2f - 1f;
                result.Points[i] = centroid + dir * (r + amp * (0.65f * n + 0.35f * wave));
            }

            result.Sanitize();
            result.EnsureClockwise();
            return result.IsValid ? result : input.CloneShape();
        }

        protected override void Modify(Polygon2D polygon, ShapeContext context, ShapeGraphAsset graph) { }
    }
}

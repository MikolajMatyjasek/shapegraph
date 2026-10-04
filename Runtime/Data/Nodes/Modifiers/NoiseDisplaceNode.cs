using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Modifiers
{
    [NodeMenu("Modifiers/Noise Displace")]
    public sealed class NoiseDisplaceNode : ShapeModifierNode
    {
        public static readonly PortId AmplitudeIn = new("Amplitude");
        public static readonly PortId NormalBlendIn = new("Normal Blend");

        [SerializeField] 
        private float amplitude = 0.04f;
        [SerializeField] 
        [Range(0f, 1f)] private float normalBlend = 0.75f;

        public float Amplitude { get => amplitude; set => amplitude = value; }
        public float NormalBlend { get => normalBlend; set => normalBlend = Mathf.Clamp01(value); }

        public override void CollectPorts(List<NodePort> ports)
        {
            base.CollectPorts(ports);
            ports.Add(new NodePort(AmplitudeIn, "Amplitude", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(NormalBlendIn, "Normal Blend", PortDirection.Input, typeof(float)));
        }

        protected override IShape2D ModifyShape(IShape2D input, ShapeContext context, ShapeGraphAsset graph)
        {
            if (input == null) return null;

            float resolvedAmplitude = ResolveFloat(AmplitudeIn, context, graph, amplitude);
            float resolvedBlend = Mathf.Clamp01(ResolveFloat(NormalBlendIn, context, graph, normalBlend));
            if (Mathf.Abs(resolvedAmplitude) < 1e-8f) return input.CloneShape();

            Polygon2D raster = input.ToPolygon();
            if (raster == null || !raster.IsValid) return input;

            Polygon2D result = raster.Clone();
            var displacement = new SeededNoiseDisplacement(context, Id, resolvedAmplitude, resolvedBlend);
            PolygonOps.Displace(result, displacement);
            result.Sanitize();
            result.EnsureClockwise();
            return result.IsValid ? result : input;
        }

        protected override void Modify(Polygon2D polygon, ShapeContext context, ShapeGraphAsset graph) { }

        private readonly struct SeededNoiseDisplacement : IVertexDisplacement
        {
            private readonly ShapeContext context;
            private readonly NodeId nodeId;
            private readonly float amplitude;
            private readonly float normalBlend;

            public SeededNoiseDisplacement(
                ShapeContext context,
                NodeId nodeId,
                float amplitude,
                float normalBlend)
            {
                this.context = context;
                this.nodeId = nodeId;
                this.amplitude = amplitude;
                this.normalBlend = normalBlend;
            }

            public Vector2 Displace(int index, Vector2 position, Vector2 outwardNormal)
            {
                float angle = context.Next01(nodeId, index) * Mathf.PI * 2f;
                Vector2 randomDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 dir = Vector2.Lerp(randomDir, outwardNormal, normalBlend).normalized;
                if (dir.sqrMagnitude < 1e-12f)
                {
                    dir = randomDir;
                }

                float signed = context.Next01(nodeId, index + 7919) * 2f - 1f;
                return position + dir * (amplitude * signed);
            }
        }
    }
}

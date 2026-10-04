using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Modifiers
{
    [NodeMenu("Modifiers/Offset")]
    public sealed class OffsetNode : ShapeModifierNode
    {
        public static readonly PortId DistanceIn = new("Distance");

        [SerializeField] 
        private float distance = 0.05f;
        [SerializeField] 
        private float maxMiterLimit = 3f;

        public float Distance { get => distance; set => distance = value; }
        public float MaxMiterLimit { get => maxMiterLimit; set => maxMiterLimit = value; }

        public override void CollectPorts(List<NodePort> ports)
        {
            base.CollectPorts(ports);
            ports.Add(new NodePort(DistanceIn, "Distance", PortDirection.Input, typeof(float)));
        }

        protected override IShape2D ModifyShape(IShape2D input, ShapeContext context, ShapeGraphAsset graph)
        {
            if (input == null) return null;

            float resolvedDistance = ResolveFloat(DistanceIn, context, graph, distance);
            if (Mathf.Abs(resolvedDistance) < 1e-8f) return input.CloneShape();

            if (input is IAnalyticOffsettable2D analytic) return analytic.OffsetAnalytic(resolvedDistance);

            Polygon2D raster = input.ToPolygon();
            if (raster == null || !raster.IsValid) return input;

            if (!PolygonOps.TryOffset(raster, resolvedDistance, Mathf.Max(0.01f, maxMiterLimit), out Polygon2D offset))
            {
                Debug.LogWarning($"[ShapeGraph] Offset on '{name}' produced an invalid polygon. Passing input through.");
                return input;
            }

            return offset;
        }

        protected override void Modify(Polygon2D polygon, ShapeContext context, ShapeGraphAsset graph) { }
    }
}

using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Modifiers
{
    public enum MirrorAxis
    {
        X = 0,
        Y = 1,
        Both = 2
    }

    [NodeMenu("Modifiers/Mirror")]
    public sealed class MirrorNode : ShapeModifierNode
    {
        public static readonly PortId PivotIn = new("Pivot");

        [SerializeField] 
        private MirrorAxis axis = MirrorAxis.X;
        [SerializeField] 
        private bool useCentroid = true;
        [SerializeField] 
        private Vector2 pivot;

        public override void CollectPorts(List<NodePort> ports)
        {
            base.CollectPorts(ports);
            ports.Add(new NodePort(PivotIn, "Pivot", PortDirection.Input, typeof(Vector2)));
        }

        protected override IShape2D ModifyShape(IShape2D input, ShapeContext context, ShapeGraphAsset graph)
        {
            if (input == null) return null;

            Polygon2D poly = input as Polygon2D ?? input.ToPolygon();
            if (poly == null || !poly.IsValid) return input.CloneShape();

            Vector2 p = useCentroid
                ? PolygonOps.Centroid(poly)
                : ResolveVector2(PivotIn, context, graph, pivot);

            Polygon2D result = poly.Clone();
            for (int i = 0; i < result.Count; i++)
            {
                Vector2 v = result.Points[i] - p;
                if (axis == MirrorAxis.X || axis == MirrorAxis.Both)
                {
                    v.x = -v.x;
                }
                if (axis == MirrorAxis.Y || axis == MirrorAxis.Both)
                {
                    v.y = -v.y;
                }
                result.Points[i] = v + p;
            }

            result.Sanitize();
            result.EnsureClockwise();
            return result.IsValid ? result : input.CloneShape();
        }

        protected override void Modify(Polygon2D polygon, ShapeContext context, ShapeGraphAsset graph) { }
    }
}

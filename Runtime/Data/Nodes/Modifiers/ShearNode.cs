using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Modifiers
{
    [NodeMenu("Modifiers/Shear")]
    public sealed class ShearNode : ShapeModifierNode
    {
        public static readonly PortId ShearXIn = new("Shear X");
        public static readonly PortId ShearYIn = new("Shear Y");
        public static readonly PortId PivotIn = new("Pivot");

        [SerializeField] 
        private float shearX = 0.35f;
        [SerializeField] 
        private float shearY;
        [SerializeField] 
        private bool useCentroid = true;
        [SerializeField] 
        private Vector2 pivot;

        public override void CollectPorts(List<NodePort> ports)
        {
            base.CollectPorts(ports);
            ports.Add(new NodePort(ShearXIn, "Shear X", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(ShearYIn, "Shear Y", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(PivotIn, "Pivot", PortDirection.Input, typeof(Vector2)));
        }

        protected override IShape2D ModifyShape(IShape2D input, ShapeContext context, ShapeGraphAsset graph)
        {
            if (input == null) return null;

            Polygon2D poly = input as Polygon2D ?? input.ToPolygon();
            if (poly == null || !poly.IsValid) return input.CloneShape();

            float sx = ResolveFloat(ShearXIn, context, graph, shearX);
            float sy = ResolveFloat(ShearYIn, context, graph, shearY);
            Vector2 p = useCentroid
                ? PolygonOps.Centroid(poly)
                : ResolveVector2(PivotIn, context, graph, pivot);

            Polygon2D result = poly.Clone();
            for (int i = 0; i < result.Count; i++)
            {
                Vector2 v = result.Points[i] - p;
                float x = v.x + sx * v.y;
                float y = v.y + sy * v.x;
                result.Points[i] = new Vector2(x, y) + p;
            }

            result.Sanitize();
            result.EnsureClockwise();
            return result.IsValid ? result : input.CloneShape();
        }

        protected override void Modify(Polygon2D polygon, ShapeContext context, ShapeGraphAsset graph) { }
    }
}

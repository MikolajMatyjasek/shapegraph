using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Modifiers
{
    [NodeMenu("Modifiers/Inflate")]
    public sealed class InflateNode : ShapeModifierNode
    {
        public static readonly PortId FactorIn = new("Factor");

        [SerializeField] 
        private float factor = 1.15f;

        public float Factor { get => factor; set => factor = value; }

        public override void CollectPorts(List<NodePort> ports)
        {
            base.CollectPorts(ports);
            ports.Add(new NodePort(FactorIn, "Factor", PortDirection.Input, typeof(float)));
        }

        protected override IShape2D ModifyShape(IShape2D input, ShapeContext context, ShapeGraphAsset graph)
        {
            if (input == null) return null;

            float resolved = ShapeMath.ClampFactor(ResolveFloat(FactorIn, context, graph, factor));
            if (Mathf.Abs(resolved - 1f) < 1e-6f) return input.CloneShape();

            if (input is IAnalyticInflatable2D analytic) return analytic.InflateAnalytic(resolved);

            Polygon2D raster = input.ToPolygon();
            if (raster == null || !raster.IsValid) return input;

            Polygon2D result = raster.Clone();
            Vector2 centroid = PolygonOps.Centroid(result);
            PolygonOps.InflateAbout(result, centroid, resolved);
            result.Sanitize();
            result.EnsureClockwise();
            return result.IsValid ? result : input;
        }

        protected override void Modify(Polygon2D polygon, ShapeContext context, ShapeGraphAsset graph) { }
    }
}

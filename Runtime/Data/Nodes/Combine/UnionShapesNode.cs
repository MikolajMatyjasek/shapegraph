using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Combine
{
    [NodeMenu("Combine/Union")]
    public sealed class UnionShapesNode : ShapeNode
    {
        public static readonly PortId ShapeAIn = new("Shape A");
        public static readonly PortId ShapeBIn = new("Shape B");
        public static readonly PortId ShapeCIn = new("Shape C");
        public static readonly PortId ShapeOut = new("Shape Out");

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(ShapeAIn, "Shape A", PortDirection.Input, typeof(IShape2D)));
            ports.Add(new NodePort(ShapeBIn, "Shape B", PortDirection.Input, typeof(IShape2D)));
            ports.Add(new NodePort(ShapeCIn, "Shape C", PortDirection.Input, typeof(IShape2D)));
            ports.Add(new NodePort(ShapeOut, "Shape Out", PortDirection.Output, typeof(IShape2D)));
        }

        protected override IShape2D ComputeShape(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != ShapeOut) return null;

            IShape2D a = GetInputShape(ShapeAIn, context, graph);
            IShape2D b = GetInputShape(ShapeBIn, context, graph);
            IShape2D c = GetInputShape(ShapeCIn, context, graph);

            IShape2D acc = null;
            if (a != null) acc = a;
            if (b != null) acc = UnionPair(acc, b);
            if (c != null) acc = UnionPair(acc, c);
            return acc?.CloneShape();
        }

        private static IShape2D UnionPair(IShape2D primary, IShape2D other)
        {
            if (primary == null) return other;
            if (other == null) return primary;

            if (PolygonBoolean.TryUnion(primary, other, out IShape2D united) && united != null) return united;

            Debug.LogWarning("[ShapeGraph] Union failed; returning clone of primary input.");
            return primary.CloneShape();
        }
    }
}

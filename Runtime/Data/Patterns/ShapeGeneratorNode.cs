using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;

namespace Galaretka.ShapeGraph.Data.Patterns
{
    public abstract class ShapeGeneratorNode : ShapeNode
    {
        public static readonly PortId ShapeOut = new("Shape Out");

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(ShapeOut, "Shape Out", PortDirection.Output, typeof(IShape2D)));
        }

        protected sealed override IShape2D ComputeShape(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != ShapeOut) return null;

            IShape2D shape = GenerateShape(context, graph);
            if (shape is Polygon2D poly)
            {
                poly.Sanitize();
                poly.EnsureClockwise();
                return poly.IsValid ? poly : null;
            }

            return shape;
        }

        protected abstract IShape2D GenerateShape(ShapeContext context, ShapeGraphAsset graph);
    }
}

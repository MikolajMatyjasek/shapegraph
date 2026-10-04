using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Patterns
{
    public abstract class ShapeModifierNode : ShapeNode
    {
        public static readonly PortId ShapeIn = new("Shape In");
        public static readonly PortId ShapeOut = new("Shape Out");

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(ShapeIn, "Shape In", PortDirection.Input, typeof(IShape2D)));
            ports.Add(new NodePort(ShapeOut, "Shape Out", PortDirection.Output, typeof(IShape2D)));
        }

        protected sealed override IShape2D ComputeShape(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != ShapeOut) return null;

            IShape2D input = ShapeGeometry.RequireBare(GetInputShape(ShapeIn, context, graph), "Modifier");
            if (input == null) return null;

            return ModifyShape(input, context, graph);
        }

        protected virtual IShape2D ModifyShape(IShape2D input, ShapeContext context, ShapeGraphAsset graph)
        {
            Polygon2D polygon = input as Polygon2D ?? input.ToPolygon();
            if (polygon == null || !polygon.IsValid) return input;

            Polygon2D result = polygon.Clone();
            try
            {
                Modify(result, context, graph);
                result.Sanitize();
                result.EnsureClockwise();
                return result.IsValid ? result : input;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ShapeGraph] Modifier '{name}' failed: {ex.Message}. Passing input through.");
                return input;
            }
        }

        protected abstract void Modify(Polygon2D polygon, ShapeContext context, ShapeGraphAsset graph);
    }
}

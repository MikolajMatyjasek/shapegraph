using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Meshing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Patterns
{
    public abstract class TerminalOutputNode : ShapeNode
    {
        public static readonly PortId ShapeIn = new("Shape In");
        public static readonly PortId FillColorIn = new("Fill Color");

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(ShapeIn, "Shape In", PortDirection.Input, typeof(IShape2D)));
            ports.Add(new NodePort(FillColorIn, "Fill Color", PortDirection.Input, typeof(Color32)));
        }

        public GeneratedMeshData EvaluateFinalMesh(ShapeContext context, ShapeGraphAsset graph)
        {
            IShape2D shape = GetInputShape(ShapeIn, context, graph);
            if (shape == null) return GeneratedMeshData.Empty;

            Polygon2D polygon = shape.ToPolygon();
            if (polygon == null || !polygon.IsValid) return GeneratedMeshData.Empty;

            polygon.Sanitize();
            Color32 fillColor = GetInputColor32(FillColorIn, context, graph, new Color32(230, 230, 230, 255));
            return ShapeMeshBuilder.BuildFilledPolygon(polygon, fillColor);
        }
    }
}

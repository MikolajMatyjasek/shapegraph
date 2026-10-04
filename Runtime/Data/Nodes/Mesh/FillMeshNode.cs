using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Meshing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Mesh
{
    [NodeMenu("Mesh/Fill")]
    public sealed class FillMeshNode : ShapeNode
    {
        public static readonly PortId ShapeIn = new("Shape In");
        public static readonly PortId FillColorIn = new("Fill Color");
        public static readonly PortId MeshOut = new("Mesh Out");

        [SerializeField] 
        private Color32 fillColor = new Color32(160, 170, 190, 255);

        public Color32 FillColor
        {
            get => fillColor;
            set => fillColor = value;
        }

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(ShapeIn, "Shape In", PortDirection.Input, typeof(IShape2D)));
            ports.Add(new NodePort(FillColorIn, "Fill Color", PortDirection.Input, typeof(Color32)));
            ports.Add(new NodePort(MeshOut, "Mesh Out", PortDirection.Output, typeof(ShapeMesh2D)));
        }

        protected override ShapeMesh2D ComputeMesh(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != MeshOut) return null;

            IShape2D shape = GetInputShape(ShapeIn, context, graph);
            if (shape == null) return null;

            var polygons = new List<Polygon2D>(4);
            PolygonBoolean.CollectPolygons(shape, polygons);
            if (polygons.Count == 0) return null;

            Color32 color = GetInputColor32(FillColorIn, context, graph, fillColor);
            return MeshOps.BuildFill(polygons, color);
        }
    }
}

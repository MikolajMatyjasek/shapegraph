using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Style
{
    [NodeMenu("Style/Fill")]
    public sealed class FillStyleNode : ShapeNode
    {
        public static readonly PortId ShapeIn = new("Shape In");
        public static readonly PortId FillColorIn = new("Fill Color");
        public static readonly PortId RegionOut = new("Region Out");

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
            ports.Add(new NodePort(RegionOut, "Region Out", PortDirection.Output, typeof(ShapeRegion2D)));
        }

        protected override IShape2D ComputeShape(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != RegionOut) return null;

            IShape2D shape = ShapeGeometry.RequireBare(GetInputShape(ShapeIn, context, graph), "Style/Fill");
            if (shape == null) return null;

            Color32 color = GetInputColor32(FillColorIn, context, graph, fillColor);
            return new ShapeRegion2D(shape, color);
        }
    }
}

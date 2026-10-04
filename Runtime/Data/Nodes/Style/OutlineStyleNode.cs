using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Style
{
    [NodeMenu("Style/Outline")]
    public sealed class OutlineStyleNode : ShapeNode
    {
        public static readonly PortId RegionIn = new("Region In");
        public static readonly PortId OutlineColorIn = new("Outline Color");
        public static readonly PortId ThicknessIn = new("Thickness");
        public static readonly PortId RegionOut = new("Region Out");

        [SerializeField]
        private Color32 outlineColor = new Color32(20, 24, 36, 255);
        [SerializeField]
        private float thickness = 0.05f;

        public Color32 OutlineColor
        {
            get => outlineColor;
            set => outlineColor = value;
        }

        public float Thickness
        {
            get => thickness;
            set => thickness = value;
        }

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(RegionIn, "Region In", PortDirection.Input, typeof(ShapeRegion2D)));
            ports.Add(new NodePort(OutlineColorIn, "Outline Color", PortDirection.Input, typeof(Color32)));
            ports.Add(new NodePort(ThicknessIn, "Thickness", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(RegionOut, "Region Out", PortDirection.Output, typeof(ShapeRegion2D)));
        }

        protected override IShape2D ComputeShape(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != RegionOut) return null;

            IShape2D input = GetInputShape(RegionIn, context, graph);
            if (input is not ShapeRegion2D region || region.Geometry == null) return null;

            Color32 color = GetInputColor32(OutlineColorIn, context, graph, outlineColor);
            float resolvedThickness = ResolveFloat(ThicknessIn, context, graph, thickness);

            var result = (ShapeRegion2D)region.CloneShape();
            result.HasOutline = resolvedThickness > 0.0001f;
            result.OutlineColor = color;
            result.OutlineThickness = resolvedThickness;
            return result;
        }
    }
}

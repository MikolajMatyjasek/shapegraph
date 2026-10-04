using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Style
{
    [NodeMenu("Style/Tint")]
    public sealed class TintStyleNode : ShapeNode
    {
        public static readonly PortId RegionIn = new("Region In");
        public static readonly PortId TintIn = new("Tint");
        public static readonly PortId RegionOut = new("Region Out");

        [SerializeField]
        private Color32 tint = new Color32(255, 220, 180, 255);

        public Color32 Tint
        {
            get => tint;
            set => tint = value;
        }

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(RegionIn, "Region In", PortDirection.Input, typeof(ShapeRegion2D)));
            ports.Add(new NodePort(TintIn, "Tint", PortDirection.Input, typeof(Color32)));
            ports.Add(new NodePort(RegionOut, "Region Out", PortDirection.Output, typeof(ShapeRegion2D)));
        }

        protected override IShape2D ComputeShape(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != RegionOut) return null;

            if (GetInputShape(RegionIn, context, graph) is not ShapeRegion2D region || region.Geometry == null) return null;

            Color32 mul = GetInputColor32(TintIn, context, graph, tint);
            var result = (ShapeRegion2D)region.CloneShape();
            result.FillColor = Multiply(result.FillColor, mul);
            if (result.HasOutline)
            {
                result.OutlineColor = Multiply(result.OutlineColor, mul);
            }
            return result;
        }

        private static Color32 Multiply(Color32 a, Color32 b) =>
            new Color32(
                (byte)(a.r * b.r / 255),
                (byte)(a.g * b.g / 255),
                (byte)(a.b * b.b / 255),
                (byte)(a.a * b.a / 255));
    }
}

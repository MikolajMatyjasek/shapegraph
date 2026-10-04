using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Style
{
    [NodeMenu("Style/Opacity")]
    public sealed class OpacityStyleNode : ShapeNode
    {
        public static readonly PortId RegionIn = new("Region In");
        public static readonly PortId OpacityIn = new("Opacity");
        public static readonly PortId RegionOut = new("Region Out");

        [SerializeField]
        [Range(0f, 1f)]
        private float opacity = 1f;

        public float Opacity
        {
            get => opacity;
            set => opacity = Mathf.Clamp01(value);
        }

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(RegionIn, "Region In", PortDirection.Input, typeof(ShapeRegion2D)));
            ports.Add(new NodePort(OpacityIn, "Opacity", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(RegionOut, "Region Out", PortDirection.Output, typeof(ShapeRegion2D)));
        }

        protected override IShape2D ComputeShape(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != RegionOut) return null;

            if (GetInputShape(RegionIn, context, graph) is not ShapeRegion2D region || region.Geometry == null) return null;

            float o = Mathf.Clamp01(ResolveFloat(OpacityIn, context, graph, opacity));
            var result = (ShapeRegion2D)region.CloneShape();
            result.FillColor = ScaleAlpha(result.FillColor, o);
            if (result.HasOutline)
            {
                result.OutlineColor = ScaleAlpha(result.OutlineColor, o);
            }
            return result;
        }

        private static Color32 ScaleAlpha(Color32 c, float opacity) =>
            new Color32(c.r, c.g, c.b, (byte)Mathf.Clamp(Mathf.RoundToInt(c.a * opacity), 0, 255));
    }
}

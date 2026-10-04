using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;

namespace Galaretka.ShapeGraph.Data.Nodes.Combine
{
    [NodeMenu("Combine/Compose")]
    public sealed class ComposeShapesNode : ShapeNode
    {
        public static readonly PortId RegionAIn = new("Region A");
        public static readonly PortId RegionBIn = new("Region B");
        public static readonly PortId RegionCIn = new("Region C");
        public static readonly PortId PictureOut = new("Picture Out");

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(RegionAIn, "Region A", PortDirection.Input, typeof(ShapeRegion2D)));
            ports.Add(new NodePort(RegionBIn, "Region B", PortDirection.Input, typeof(ShapeRegion2D)));
            ports.Add(new NodePort(RegionCIn, "Region C", PortDirection.Input, typeof(ShapeRegion2D)));
            ports.Add(new NodePort(PictureOut, "Picture Out", PortDirection.Output, typeof(ShapePicture2D)));
        }

        protected override ShapePicture2D ComputePicture(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != PictureOut) return null;

            var picture = new ShapePicture2D();
            TryAdd(picture, GetInputShape(RegionAIn, context, graph));
            TryAdd(picture, GetInputShape(RegionBIn, context, graph));
            TryAdd(picture, GetInputShape(RegionCIn, context, graph));
            return picture.Layers.Count > 0 ? picture : null;
        }

        private static void TryAdd(ShapePicture2D picture, IShape2D shape)
        {
            if (shape is ShapeRegion2D region)
            {
                picture.Add(region);
            }
        }
    }
}

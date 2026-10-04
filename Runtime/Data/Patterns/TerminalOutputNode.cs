using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Meshing;

namespace Galaretka.ShapeGraph.Data.Patterns
{
    public abstract class TerminalOutputNode : ShapeNode
    {
        public static readonly PortId PictureIn = new("Picture In");

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(PictureIn, "Picture In", PortDirection.Input, typeof(ShapePicture2D)));
        }

        public GeneratedMeshData EvaluateFinalMesh(ShapeContext context, ShapeGraphAsset graph)
        {
            ShapePicture2D picture = GetInputPicture(PictureIn, context, graph);
            if (picture == null || picture.Layers.Count == 0) return GeneratedMeshData.Empty;

            return ShapeBake.BakePicture(picture);
        }
    }
}

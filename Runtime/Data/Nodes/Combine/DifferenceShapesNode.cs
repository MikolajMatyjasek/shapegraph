using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Combine
{
    [NodeMenu("Combine/Difference")]
    public sealed class DifferenceShapesNode : ShapeNode
    {
        public static readonly PortId SubjectIn = new("Subject");
        public static readonly PortId ClipIn = new("Clip");
        public static readonly PortId ShapeOut = new("Shape Out");

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(SubjectIn, "Subject", PortDirection.Input, typeof(IShape2D)));
            ports.Add(new NodePort(ClipIn, "Clip", PortDirection.Input, typeof(IShape2D)));
            ports.Add(new NodePort(ShapeOut, "Shape Out", PortDirection.Output, typeof(IShape2D)));
        }

        protected override IShape2D ComputeShape(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != ShapeOut) return null;

            IShape2D subject = ShapeGeometry.RequireBare(GetInputShape(SubjectIn, context, graph), "Combine/Difference");
            if (subject == null) return null;

            IShape2D clip = ShapeGeometry.RequireBare(GetInputShape(ClipIn, context, graph), "Combine/Difference");
            if (clip == null) return subject.CloneShape();

            if (PolygonBoolean.TryDifference(subject, clip, out IShape2D result) && result != null) return result;

            Debug.LogWarning("[ShapeGraph] Difference failed; returning clone of subject.");
            return subject.CloneShape();
        }
    }
}

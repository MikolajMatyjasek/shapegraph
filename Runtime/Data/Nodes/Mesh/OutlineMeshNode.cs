using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Meshing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Mesh
{
    [NodeMenu("Mesh/Outline")]
    public sealed class OutlineMeshNode : ShapeNode
    {
        public static readonly PortId MeshIn = new("Mesh In");
        public static readonly PortId OutlineColorIn = new("Outline Color");
        public static readonly PortId ThicknessIn = new("Thickness");
        public static readonly PortId MeshOut = new("Mesh Out");

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
            ports.Add(new NodePort(MeshIn, "Mesh In", PortDirection.Input, typeof(ShapeMesh2D)));
            ports.Add(new NodePort(OutlineColorIn, "Outline Color", PortDirection.Input, typeof(Color32)));
            ports.Add(new NodePort(ThicknessIn, "Thickness", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(MeshOut, "Mesh Out", PortDirection.Output, typeof(ShapeMesh2D)));
        }

        protected override ShapeMesh2D ComputeMesh(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != MeshOut) return null;

            ShapeMesh2D source = GetInputMesh(MeshIn, context, graph);
            if (source == null || !source.IsValid) return null;

            Color32 color = GetInputColor32(OutlineColorIn, context, graph, outlineColor);
            float resolvedThickness = ResolveFloat(ThicknessIn, context, graph, thickness);
            return MeshOps.AppendOutline(source, resolvedThickness, color);
        }
    }
}

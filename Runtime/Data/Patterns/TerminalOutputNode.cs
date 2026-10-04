using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Patterns
{
    public abstract class TerminalOutputNode : ShapeNode
    {
        public static readonly PortId MeshIn = new("Mesh In");

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(MeshIn, "Mesh In", PortDirection.Input, typeof(ShapeMesh2D)));
        }

        public GeneratedMeshData EvaluateFinalMesh(ShapeContext context, ShapeGraphAsset graph)
        {
            ShapeMesh2D mesh = GetInputMesh(MeshIn, context, graph);
            if (mesh == null || !mesh.IsValid) return GeneratedMeshData.Empty;

            return mesh.ToGeneratedMeshData();
        }
    }
}

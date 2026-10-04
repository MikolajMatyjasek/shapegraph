using System;
using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Data;
using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Editor.Graph.Theme;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Meshing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Editor.Graph.Preview
{
    public readonly struct PreviewEvalResult
    {
        public readonly GeneratedMeshData Mesh;
        public readonly bool Success;
        public readonly string Error;

        public PreviewEvalResult(GeneratedMeshData mesh, bool success, string error)
        {
            Mesh = mesh;
            Success = success;
            Error = error;
        }

        public static PreviewEvalResult Failed(string error) =>
            new(GeneratedMeshData.Empty, false, error);
    }

    /// <summary>
    /// Evaluates a node to <see cref="GeneratedMeshData"/> using the runtime pipeline.
    /// Geometry nodes get a neutral editor fill.
    /// </summary>
    public static class PreviewEvaluator
    {
        private static readonly List<NodePort> PortBuffer = new(16);
        private static readonly List<Polygon2D> PolygonBuffer = new(8);

        public static PreviewEvalResult EvaluateNode(ShapeGraphAsset asset, ShapeNode node, int seed)
        {
            if (asset == null || node == null)
                return PreviewEvalResult.Failed("Missing asset or node.");

            try
            {
                var context = new ShapeContext(seed);

                if (node is TerminalOutputNode terminal)
                {
                    GeneratedMeshData data = terminal.EvaluateFinalMesh(context, asset);
                    return new PreviewEvalResult(data, data.Vertices != null && data.Vertices.Length >= 3, null);
                }

                PortBuffer.Clear();
                node.CollectPorts(PortBuffer);

                PortId? meshOut = FindFirstOutput(typeof(ShapeMesh2D));
                if (meshOut.HasValue)
                {
                    ShapeMesh2D mesh = node.EvaluateMesh(meshOut.Value, context, asset);
                    if (mesh == null || !mesh.IsValid)
                        return PreviewEvalResult.Failed("Mesh output invalid.");
                    return new PreviewEvalResult(mesh.ToGeneratedMeshData(), true, null);
                }

                PortId? shapeOut = FindFirstShapeOutput();
                if (shapeOut.HasValue)
                {
                    IShape2D shape = node.EvaluateShape(shapeOut.Value, context, asset);
                    if (shape == null)
                        return PreviewEvalResult.Failed("Shape output null.");

                    PolygonBuffer.Clear();
                    PolygonBoolean.CollectPolygons(shape, PolygonBuffer);
                    ShapeMesh2D filled = MeshOps.BuildFill(PolygonBuffer, PortColorMap.PreviewNeutralFill);
                    if (filled == null || !filled.IsValid)
                        return PreviewEvalResult.Failed("Geometry preview fill failed.");
                    return new PreviewEvalResult(filled.ToGeneratedMeshData(), true, null);
                }

                return PreviewEvalResult.Failed("No previewable output port.");
            }
            catch (Exception ex)
            {
                return PreviewEvalResult.Failed(ex.Message);
            }
        }

        public static PreviewEvalResult EvaluateMaster(ShapeGraphAsset asset, int seed)
        {
            if (asset == null)
                return PreviewEvalResult.Failed("Missing asset.");

            try
            {
                var context = new ShapeContext(seed);
                GeneratedMeshData data = asset.Evaluate(context);
                bool ok = data.Vertices != null && data.Vertices.Length >= 3;
                return new PreviewEvalResult(data, ok, ok ? null : "Terminal produced empty mesh.");
            }
            catch (Exception ex)
            {
                return PreviewEvalResult.Failed(ex.Message);
            }
        }

        private static PortId? FindFirstOutput(Type exactType)
        {
            for (int i = 0; i < PortBuffer.Count; i++)
            {
                NodePort p = PortBuffer[i];
                if (p.Direction == PortDirection.Output && p.GetPortType() == exactType)
                    return p.Id;
            }
            return null;
        }

        private static PortId? FindFirstShapeOutput()
        {
            for (int i = 0; i < PortBuffer.Count; i++)
            {
                NodePort p = PortBuffer[i];
                Type t = p.GetPortType();
                if (p.Direction == PortDirection.Output && t != null && typeof(IShape2D).IsAssignableFrom(t))
                    return p.Id;
            }
            return null;
        }
    }
}

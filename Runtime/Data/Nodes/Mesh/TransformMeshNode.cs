using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Data.Nodes.Modifiers;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Meshing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Mesh
{
    [NodeMenu("Mesh/Transform")]
    public sealed class TransformMeshNode : ShapeNode
    {
        public static readonly PortId MeshIn = new("Mesh In");
        public static readonly PortId TranslationIn = new("Translation");
        public static readonly PortId RotationIn = new("Rotation");
        public static readonly PortId ScaleIn = new("Scale");
        public static readonly PortId PivotIn = new("Pivot");
        public static readonly PortId MeshOut = new("Mesh Out");

        [SerializeField] 
        private Vector2 translation = Vector2.zero;
        [SerializeField] 
        private float rotationDegrees;
        [SerializeField] 
        private Vector2 scale = Vector2.one;
        [SerializeField] 
        private Vector2 pivot = Vector2.zero;
        [SerializeField] 
        private TransformPivotMode pivotMode = TransformPivotMode.Centroid;

        public Vector2 Translation { get => translation; set => translation = value; }
        public float RotationDegrees { get => rotationDegrees; set => rotationDegrees = value; }
        public Vector2 Scale { get => scale; set => scale = value; }
        public Vector2 Pivot { get => pivot; set => pivot = value; }
        public TransformPivotMode PivotMode { get => pivotMode; set => pivotMode = value; }

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(MeshIn, "Mesh In", PortDirection.Input, typeof(ShapeMesh2D)));
            ports.Add(new NodePort(TranslationIn, "Translation", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(RotationIn, "Rotation", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(ScaleIn, "Scale", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(PivotIn, "Pivot", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(MeshOut, "Mesh Out", PortDirection.Output, typeof(ShapeMesh2D)));
        }

        protected override ShapeMesh2D ComputeMesh(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != MeshOut) return null;

            ShapeMesh2D source = GetInputMesh(MeshIn, context, graph);
            if (source == null || !source.IsValid) return null;

            Vector2 resolvedTranslation = ResolveVector2(TranslationIn, context, graph, translation);
            float resolvedRotation = ResolveFloat(RotationIn, context, graph, rotationDegrees);
            Vector2 resolvedScale = ResolveVector2(ScaleIn, context, graph, scale);

            Vector2 resolvedPivot;
            if (pivotMode == TransformPivotMode.Explicit)
            {
                resolvedPivot = ResolveVector2(PivotIn, context, graph, pivot);
            }
            else if (source.Contour != null && source.Contour.Length >= 3)
            {
                resolvedPivot = PolygonOps.Centroid(new Polygon2D(source.Contour));
            }
            else
            {
                resolvedPivot = Vector2.zero;
            }

            var transform = new AffineTransform2D(
                resolvedTranslation,
                resolvedRotation,
                resolvedScale,
                resolvedPivot);

            return MeshOps.Transform(source, transform);
        }
    }
}

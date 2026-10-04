using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Core.Geometry;
using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Evaluation;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Modifiers
{
    public enum TransformPivotMode
    {
        Centroid = 0,
        Explicit = 1
    }

    [NodeMenu("Modifiers/Transform")]
    public sealed class TransformNode : ShapeModifierNode
    {
        public static readonly PortId TranslationIn = new("Translation");
        public static readonly PortId RotationIn = new("Rotation");
        public static readonly PortId ScaleIn = new("Scale");
        public static readonly PortId PivotIn = new("Pivot");

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
            base.CollectPorts(ports);
            ports.Add(new NodePort(TranslationIn, "Translation", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(RotationIn, "Rotation", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(ScaleIn, "Scale", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(PivotIn, "Pivot", PortDirection.Input, typeof(Vector2)));
        }

        protected override IShape2D ModifyShape(IShape2D input, ShapeContext context, ShapeGraphAsset graph)
        {
            if (input == null) return null;

            Vector2 resolvedTranslation = ResolveVector2(TranslationIn, context, graph, translation);
            float resolvedRotation = ResolveFloat(RotationIn, context, graph, rotationDegrees);
            Vector2 resolvedScale = ResolveVector2(ScaleIn, context, graph, scale);

            Vector2 resolvedPivot;
            if (pivotMode == TransformPivotMode.Explicit)
            {
                resolvedPivot = ResolveVector2(PivotIn, context, graph, pivot);
            }
            else
            {
                Polygon2D poly = input as Polygon2D ?? input.ToPolygon();
                resolvedPivot = poly != null && poly.IsValid ? PolygonOps.Centroid(poly) : Vector2.zero;
            }

            var transform = new AffineTransform2D(
                resolvedTranslation,
                resolvedRotation,
                resolvedScale,
                resolvedPivot);

            if (transform.IsIdentity()) return input.CloneShape();

            if (input is IAnalyticTransformable2D analytic) return analytic.TransformAnalytic(transform);

            Polygon2D raster = input.ToPolygon();
            if (raster == null || !raster.IsValid) return input;

            Polygon2D result = raster.Clone();
            PolygonOps.ApplyTransform(result, transform);
            result.Sanitize();
            result.EnsureClockwise();
            return result.IsValid ? result : input;
        }

        protected override void Modify(Polygon2D polygon, ShapeContext context, ShapeGraphAsset graph)
        {
            // Unused: ModifyShape handles the full path.
        }
    }
}

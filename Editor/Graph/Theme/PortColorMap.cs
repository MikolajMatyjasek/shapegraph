using System;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Editor.Graph.Theme
{
    public enum NodeDomainKind
    {
        Geometry,
        Mesh,
        Output,
        Utility
    }

    public static class PortColorMap
    {
        public static readonly Color Geometry = new(0.35f, 0.62f, 0.95f, 1f);
        public static readonly Color Mesh = new(0.25f, 0.78f, 0.72f, 1f);
        public static readonly Color Value = new(0.92f, 0.72f, 0.28f, 1f);
        public static readonly Color ColorPort = new(0.85f, 0.35f, 0.75f, 1f);
        public static readonly Color Output = new(0.78f, 0.78f, 0.82f, 1f);
        public static readonly Color32 PreviewNeutralFill = new Color32(170, 176, 186, 255);

        public static Color ForType(Type type)
        {
            if (type == null)
                return Value;

            if (type == typeof(ShapeMesh2D))
                return Mesh;
            if (typeof(IShape2D).IsAssignableFrom(type))
                return Geometry;
            if (type == typeof(Color) || type == typeof(Color32))
                return ColorPort;
            if (type == typeof(float) || type == typeof(int) || type == typeof(bool) || type == typeof(Vector2))
                return Value;

            return Value;
        }

        public static Color ForPortTypeId(PortTypeId typeId)
        {
            return PortTypeRegistry.TryResolve(typeId, out Type type) ? ForType(type) : Value;
        }

        public static NodeDomainKind ClassifyNode(Type nodeType, System.Collections.Generic.List<Data.NodePort> ports)
        {
            if (nodeType != null && typeof(Data.Patterns.TerminalOutputNode).IsAssignableFrom(nodeType))
                return NodeDomainKind.Output;

            bool hasMesh = false;
            bool hasShape = false;
            if (ports != null)
            {
                for (int i = 0; i < ports.Count; i++)
                {
                    Type t = ports[i].GetPortType();
                    if (t == typeof(ShapeMesh2D))
                        hasMesh = true;
                    if (t != null && typeof(IShape2D).IsAssignableFrom(t))
                        hasShape = true;
                }
            }

            if (hasMesh)
                return NodeDomainKind.Mesh;
            if (hasShape)
                return NodeDomainKind.Geometry;
            return NodeDomainKind.Utility;
        }
    }
}

using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Values.Vector2Nodes
{
    [NodeMenu("Values/Vector2/Add")]
    public sealed class AddVector2Node : ShapeNode
    {
        public static readonly PortId AIn = new("A");
        public static readonly PortId BIn = new("B");
        public static readonly PortId ValueOut = new("Value Out");

        [SerializeField] 
        private Vector2 a;
        [SerializeField] 
        private Vector2 b;

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(AIn, "A", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(BIn, "B", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(ValueOut, "Value Out", PortDirection.Output, typeof(Vector2)));
        }

        protected override ParameterValue ComputeValue(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != ValueOut) return default;
            Vector2 ra = ResolveVector2(AIn, context, graph, a);
            Vector2 rb = ResolveVector2(BIn, context, graph, b);
            return ParameterValue.FromVector2(ra + rb);
        }
    }

    [NodeMenu("Values/Vector2/Scale")]
    public sealed class ScaleVector2Node : ShapeNode
    {
        public static readonly PortId VectorIn = new("Vector");
        public static readonly PortId ScaleIn = new("Scale");
        public static readonly PortId ValueOut = new("Value Out");

        [SerializeField] 
        private Vector2 vector = Vector2.one;
        [SerializeField] 
        private float scale = 1f;

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(VectorIn, "Vector", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(ScaleIn, "Scale", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(ValueOut, "Value Out", PortDirection.Output, typeof(Vector2)));
        }

        protected override ParameterValue ComputeValue(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != ValueOut) return default;
            Vector2 v = ResolveVector2(VectorIn, context, graph, vector);
            float s = ResolveFloat(ScaleIn, context, graph, scale);
            return ParameterValue.FromVector2(v * s);
        }
    }

    [NodeMenu("Values/Vector2/Length")]
    public sealed class LengthVector2Node : ShapeNode
    {
        public static readonly PortId VectorIn = new("Vector");
        public static readonly PortId ValueOut = new("Value Out");

        [SerializeField] 
        private Vector2 vector = Vector2.right;

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(VectorIn, "Vector", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(ValueOut, "Value Out", PortDirection.Output, typeof(float)));
        }

        protected override ParameterValue ComputeValue(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != ValueOut) return default;
            Vector2 v = ResolveVector2(VectorIn, context, graph, vector);
            return ParameterValue.FromFloat(v.magnitude);
        }
    }

    [NodeMenu("Values/Vector2/Normalize")]
    public sealed class NormalizeVector2Node : ShapeNode
    {
        public static readonly PortId VectorIn = new("Vector");
        public static readonly PortId ValueOut = new("Value Out");

        [SerializeField] 
        private Vector2 vector = Vector2.right;

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(VectorIn, "Vector", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(ValueOut, "Value Out", PortDirection.Output, typeof(Vector2)));
        }

        protected override ParameterValue ComputeValue(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != ValueOut) return default;
            Vector2 v = ResolveVector2(VectorIn, context, graph, vector);
            if (v.sqrMagnitude < 1e-12f)
            {
                return ParameterValue.FromVector2(Vector2.zero);
            }
            return ParameterValue.FromVector2(v.normalized);
        }
    }

    [NodeMenu("Values/Vector2/From Floats")]
    public sealed class FromFloatsVector2Node : ShapeNode
    {
        public static readonly PortId XIn = new("X");
        public static readonly PortId YIn = new("Y");
        public static readonly PortId ValueOut = new("Value Out");

        [SerializeField] 
        private float x;
        [SerializeField] 
        private float y;

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(XIn, "X", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(YIn, "Y", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(ValueOut, "Value Out", PortDirection.Output, typeof(Vector2)));
        }

        protected override ParameterValue ComputeValue(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != ValueOut) return default;
            float rx = ResolveFloat(XIn, context, graph, x);
            float ry = ResolveFloat(YIn, context, graph, y);
            return ParameterValue.FromVector2(new Vector2(rx, ry));
        }
    }

    [NodeMenu("Values/Vector2/Split")]
    public sealed class SplitVector2Node : ShapeNode
    {
        public static readonly PortId VectorIn = new("Vector");
        public static readonly PortId XOut = new("X");
        public static readonly PortId YOut = new("Y");

        [SerializeField] 
        private Vector2 vector;

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(VectorIn, "Vector", PortDirection.Input, typeof(Vector2)));
            ports.Add(new NodePort(XOut, "X", PortDirection.Output, typeof(float)));
            ports.Add(new NodePort(YOut, "Y", PortDirection.Output, typeof(float)));
        }

        protected override ParameterValue ComputeValue(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            Vector2 v = ResolveVector2(VectorIn, context, graph, vector);
            if (portId == XOut) return ParameterValue.FromFloat(v.x);
            if (portId == YOut) return ParameterValue.FromFloat(v.y);
            return default;
        }
    }
}

using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Patterns
{
    public abstract class BinaryFloatMathNode : ShapeNode
    {
        public static readonly PortId AIn = new("A");
        public static readonly PortId BIn = new("B");
        public static readonly PortId ValueOut = new("Value Out");

        [SerializeField]
        private float a = 0f;
        [SerializeField]
        private float b = 1f;

        public float A { get => a; set => a = value; }
        public float B { get => b; set => b = value; }

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(AIn, "A", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(BIn, "B", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(ValueOut, "Value Out", PortDirection.Output, typeof(float)));
        }

        protected sealed override ParameterValue ComputeValue(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != ValueOut) return default;

            float ra = ResolveFloat(AIn, context, graph, a);
            float rb = ResolveFloat(BIn, context, graph, b);
            return ParameterValue.FromFloat(Evaluate(ra, rb));
        }

        protected abstract float Evaluate(float aValue, float bValue);
    }
}

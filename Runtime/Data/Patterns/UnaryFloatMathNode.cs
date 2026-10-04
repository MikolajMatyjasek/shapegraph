using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Patterns
{
    public abstract class UnaryFloatMathNode : ShapeNode
    {
        public static readonly PortId ValueIn = new("Value In");
        public static readonly PortId ValueOut = new("Value Out");

        [SerializeField]
        private float value;

        public float Value { get => value; set => this.value = value; }

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(ValueIn, "Value In", PortDirection.Input, typeof(float)));
            ports.Add(new NodePort(ValueOut, "Value Out", PortDirection.Output, typeof(float)));
        }

        protected sealed override ParameterValue ComputeValue(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != ValueOut) return default;

            float input = ResolveFloat(ValueIn, context, graph, value);
            return ParameterValue.FromFloat(Evaluate(input));
        }

        protected abstract float Evaluate(float input);
    }
}

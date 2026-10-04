using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Values
{
    [NodeMenu("Values/Time")]
    public sealed class TimeNode : ShapeNode
    {
        public static readonly PortId ValueOut = new("Value Out");

        [SerializeField]
        private float scale = 1f;
        [SerializeField]
        private float offset;

        public float Scale { get => scale; set => scale = value; }
        public float Offset { get => offset; set => offset = value; }

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(ValueOut, "Value Out", PortDirection.Output, typeof(float)));
        }

        protected override ParameterValue ComputeValue(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != ValueOut) return default;

            float t = context.TimeSeconds * scale + offset;
            return ParameterValue.FromFloat(t);
        }
    }
}

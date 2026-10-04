using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data.Nodes.Values
{
    public sealed class ParameterNode : ShapeNode
    {
        public static readonly PortId ValueOut = new("Value Out");

        [SerializeField]
        private string parameterName = string.Empty;

        [SerializeField]
        private ParameterValueType outputType = ParameterValueType.Float;

        public string ParameterName => parameterName;
        public ParameterValueType OutputType => outputType;

        public void BindParameter(string name, ParameterValueType type)
        {
            parameterName = name ?? string.Empty;
            outputType = type;
            if (!string.IsNullOrEmpty(parameterName))
            {
                this.name = parameterName;
            }
            MarkDirty();
        }

        public override void CollectPorts(List<NodePort> ports)
        {
            ports.Add(new NodePort(ValueOut, "Value Out", PortDirection.Output, ToPortClrType(outputType)));
        }

        protected override ParameterValue ComputeValue(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != ValueOut) return default;

            if (outputType == ParameterValueType.Color) return default;

            return Resolve(graph, context);
        }

        protected override Color32 ComputeColor32(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            if (portId != ValueOut || outputType != ParameterValueType.Color) return new Color32(255, 255, 255, 255);

            ParameterValue value = Resolve(graph, context);
            if (value.Type != ParameterValueType.Color) return new Color32(255, 255, 255, 255);

            Color c = value.AsColor();
            return (Color32)c;
        }

        private ParameterValue Resolve(ShapeGraphAsset graph, ShapeContext context)
        {
            if (graph == null || string.IsNullOrEmpty(parameterName))
            {
                return ParameterValue.DefaultFor(outputType);
            }

            GraphParameter param = graph.FindParameterByName(parameterName);
            if (param == null)
            {
                Debug.LogWarning(
                    $"[ShapeGraph] ParameterNode '{name}' bound to missing parameter '{parameterName}'.");
                return ParameterValue.DefaultFor(outputType);
            }

            if (param.ValueType != outputType)
            {
                Debug.LogWarning(
                    $"[ShapeGraph] ParameterNode '{name}' type mismatch " +
                    $"(node {outputType} vs param {param.ValueType}). Using default.");
                return ParameterValue.DefaultFor(outputType);
            }

            return graph.ResolveParameter(param.Id, context);
        }

        public static System.Type ToPortClrType(ParameterValueType type) => type switch
        {
            ParameterValueType.Float => typeof(float),
            ParameterValueType.Int => typeof(int),
            ParameterValueType.Bool => typeof(bool),
            ParameterValueType.Vector2 => typeof(Vector2),
            ParameterValueType.Color => typeof(Color32),
            _ => typeof(float)
        };
    }
}

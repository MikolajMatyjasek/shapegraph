using System;
using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeMenuAttribute : Attribute
    {
        public string Path { get; }
        public NodeMenuAttribute(string path) => Path = path;
    }

    public abstract class ShapeNode : ScriptableObject
    {
        [SerializeField] 
        private NodeId id;
        [SerializeField] 
        private Vector2 graphPosition;

        [NonSerialized] 
        private uint localVersion = 1;

        public NodeId Id => id;
        public Vector2 GraphPosition { get => graphPosition; set => graphPosition = value; }
        public uint Version => localVersion;

        public virtual void OnEnable()
        {
            if (!id.IsValid)
            {
                id = NodeId.NewId();
            }
        }

        public void MarkDirty()
        {
            unchecked { localVersion++; }
        }

        public abstract void CollectPorts(List<NodePort> ports);

        public IShape2D EvaluateShape(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            PortKey key = new(id, portId);
            if (context.TryGetShape(key, out IShape2D cached)) return cached;

            IShape2D result = ComputeShape(portId, context, graph);
            context.SetShape(key, result);
            return result;
        }

        public ParameterValue EvaluateValue(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            PortKey key = new(id, portId);
            if (context.TryGetValue(key, out ParameterValue cached)) return cached;

            ParameterValue result = ComputeValue(portId, context, graph);
            context.SetValue(key, result);
            return result;
        }

        public Color32 EvaluateColor32(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            PortKey key = new(id, portId);
            if (context.TryGetColor32(key, out Color32 cached)) return cached;

            Color32 result = ComputeColor32(portId, context, graph);
            context.SetColor32(key, result);
            return result;
        }

        public ShapeMesh2D EvaluateMesh(PortId portId, ShapeContext context, ShapeGraphAsset graph)
        {
            PortKey key = new(id, portId);
            if (context.TryGetMesh(key, out ShapeMesh2D cached)) return cached;

            ShapeMesh2D result = ComputeMesh(portId, context, graph);
            context.SetMesh(key, result);
            return result;
        }

        protected virtual IShape2D ComputeShape(PortId portId, ShapeContext context, ShapeGraphAsset graph) => null;

        protected virtual ParameterValue ComputeValue(PortId portId, ShapeContext context, ShapeGraphAsset graph) =>
            default;

        protected virtual Color32 ComputeColor32(PortId portId, ShapeContext context, ShapeGraphAsset graph) =>
            new Color32(255, 255, 255, 255);

        protected virtual ShapeMesh2D ComputeMesh(PortId portId, ShapeContext context, ShapeGraphAsset graph) => null;

        protected IShape2D GetInputShape(PortId inputPortId, ShapeContext context, ShapeGraphAsset graph, IShape2D fallback = null)
        {
            NodeConnection? conn = graph.GetConnectionToInput(id, inputPortId);
            if (!conn.HasValue) return fallback;

            ShapeNode source = graph.GetNodeById(conn.Value.FromNodeId);
            return source != null
                ? source.EvaluateShape(conn.Value.FromPortId, context, graph)
                : fallback;
        }

        protected ParameterValue GetInputValue(PortId inputPortId, ShapeContext context, ShapeGraphAsset graph, ParameterValue fallback = default)
        {
            NodeConnection? conn = graph.GetConnectionToInput(id, inputPortId);
            if (!conn.HasValue) return fallback;

            ShapeNode source = graph.GetNodeById(conn.Value.FromNodeId);
            return source != null
                ? source.EvaluateValue(conn.Value.FromPortId, context, graph)
                : fallback;
        }

        protected float GetInputFloat(PortId inputPortId, ShapeContext context, ShapeGraphAsset graph, float fallback = 0f)
        {
            ParameterValue value = GetInputValue(inputPortId, context, graph, ParameterValue.FromFloat(fallback));
            return value.Type == ParameterValueType.Float ? value.AsFloat() : fallback;
        }

        protected int GetInputInt(PortId inputPortId, ShapeContext context, ShapeGraphAsset graph, int fallback = 0)
        {
            ParameterValue value = GetInputValue(inputPortId, context, graph, ParameterValue.FromInt(fallback));
            return value.Type == ParameterValueType.Int ? value.AsInt() : fallback;
        }

        protected bool GetInputBool(PortId inputPortId, ShapeContext context, ShapeGraphAsset graph, bool fallback = false)
        {
            ParameterValue value = GetInputValue(inputPortId, context, graph, ParameterValue.FromBool(fallback));
            return value.Type == ParameterValueType.Bool ? value.AsBool() : fallback;
        }

        protected Vector2 GetInputVector2(PortId inputPortId, ShapeContext context, ShapeGraphAsset graph, Vector2 fallback = default)
        {
            ParameterValue value = GetInputValue(inputPortId, context, graph, ParameterValue.FromVector2(fallback));
            return value.Type == ParameterValueType.Vector2 ? value.AsVector2() : fallback;
        }

        protected Color32 GetInputColor32(PortId inputPortId, ShapeContext context, ShapeGraphAsset graph, Color32 fallback = default)
        {
            NodeConnection? conn = graph.GetConnectionToInput(id, inputPortId);
            if (!conn.HasValue) return fallback;

            ShapeNode source = graph.GetNodeById(conn.Value.FromNodeId);
            return source != null
                ? source.EvaluateColor32(conn.Value.FromPortId, context, graph)
                : fallback;
        }

        protected ShapeMesh2D GetInputMesh(PortId inputPortId, ShapeContext context, ShapeGraphAsset graph, ShapeMesh2D fallback = null)
        {
            NodeConnection? conn = graph.GetConnectionToInput(id, inputPortId);
            if (!conn.HasValue) return fallback;

            ShapeNode source = graph.GetNodeById(conn.Value.FromNodeId);
            return source != null
                ? source.EvaluateMesh(conn.Value.FromPortId, context, graph)
                : fallback;
        }

        protected float ResolveFloat(PortId portId, ShapeContext context, ShapeGraphAsset graph, float embedded)
        {
            NodeConnection? conn = graph.GetConnectionToInput(id, portId);
            if (!conn.HasValue)
                return embedded;

            ShapeNode source = graph.GetNodeById(conn.Value.FromNodeId);
            if (source == null)
                return embedded;

            ParameterValue value = source.EvaluateValue(conn.Value.FromPortId, context, graph);
            if (value.Type != ParameterValueType.Float)
            {
                Debug.LogWarning(
                    $"[ShapeGraph] Node '{name}' port resolve expected Float, got {value.Type}. Using embedded value.");
                return embedded;
            }

            return value.AsFloat();
        }

        protected int ResolveInt(PortId portId, ShapeContext context, ShapeGraphAsset graph, int embedded)
        {
            NodeConnection? conn = graph.GetConnectionToInput(id, portId);
            if (!conn.HasValue) return embedded;

            ShapeNode source = graph.GetNodeById(conn.Value.FromNodeId);
            if (source == null) return embedded;

            ParameterValue value = source.EvaluateValue(conn.Value.FromPortId, context, graph);
            if (value.Type != ParameterValueType.Int)
            {
                Debug.LogWarning(
                    $"[ShapeGraph] Node '{name}' port resolve expected Int, got {value.Type}. Using embedded value.");
                return embedded;
            }

            return value.AsInt();
        }

        protected Vector2 ResolveVector2(PortId portId, ShapeContext context, ShapeGraphAsset graph, Vector2 embedded)
        {
            NodeConnection? conn = graph.GetConnectionToInput(id, portId);
            if (!conn.HasValue) return embedded;

            ShapeNode source = graph.GetNodeById(conn.Value.FromNodeId);
            if (source == null) return embedded;

            ParameterValue value = source.EvaluateValue(conn.Value.FromPortId, context, graph);
            if (value.Type != ParameterValueType.Vector2)
            {
                Debug.LogWarning(
                    $"[ShapeGraph] Node '{name}' port resolve expected Vector2, got {value.Type}. Using embedded value.");
                return embedded;
            }

            return value.AsVector2();
        }
    }
}

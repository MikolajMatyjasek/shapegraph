using System;
using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Data.Patterns;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Data
{
    [CreateAssetMenu(fileName = "NewShapeGraph", menuName = "Galaretka/Shape Graph")]
    public class ShapeGraphAsset : ScriptableObject
    {
        [Header("Rendering")]
        [SerializeField] private Material defaultMaterial;

        [Header("Parameters & Randomization")]
        [SerializeField] 
        private List<GraphParameter> parameters = new();

        [Header("Graph Topology")]
        [SerializeField] 
        private List<ShapeNode> nodes = new();
        [SerializeField] 
        private List<NodeConnection> connections = new();

        public Material DefaultMaterial => defaultMaterial;
        public IReadOnlyList<GraphParameter> Parameters => parameters;
        public IReadOnlyList<ShapeNode> Nodes => nodes;
        public IReadOnlyList<NodeConnection> Connections => connections;

        public event Action OnGraphModified;

        public GeneratedMeshData Evaluate(ShapeContext context)
        {
            TerminalOutputNode terminal = GetTerminalNode();
            if (terminal == null) return GeneratedMeshData.Empty;

            return terminal.EvaluateFinalMesh(context, this);
        }

        public TerminalOutputNode GetTerminalNode()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] is TerminalOutputNode terminal) return terminal;
            }
            return null;
        }

        public ShapeNode GetNodeById(NodeId nodeId)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] != null && nodes[i].Id == nodeId) return nodes[i];
            }
            return null;
        }

        public NodeConnection? GetConnectionToInput(NodeId nodeId, PortId inputPortId)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                NodeConnection c = connections[i];
                if (c.ToNodeId == nodeId && c.ToPortId == inputPortId) return c;
            }
            return null;
        }

        public ParameterValue ResolveParameter(ParameterId paramId, ShapeContext context)
        {
            GraphParameter param = null;
            for (int i = 0; i < parameters.Count; i++)
            {
                if (parameters[i] != null && parameters[i].Id == paramId)
                {
                    param = parameters[i];
                    break;
                }
            }

            if (param == null)
            {
                Debug.LogWarning($"[ShapeGraph] Parameter id {paramId} not found on asset '{name}'.");
                return ParameterValue.FromFloat(0f);
            }

            if (param.Mode == ParameterMode.Exposed &&
                context.TryGetOverride(paramId, out ParameterValue overridden) &&
                overridden.Type == param.ValueType)
            {
                return overridden;
            }

            return param.Evaluate(context);
        }

        public GraphParameter FindParameterByName(string paramName)
        {
            if (string.IsNullOrEmpty(paramName)) return null;

            ParameterId id = ParameterId.FromString(paramName);
            for (int i = 0; i < parameters.Count; i++)
            {
                if (parameters[i] != null && parameters[i].Id == id) return parameters[i];
            }
            return null;
        }

        #region Mutation API

        public void AddParameter(GraphParameter param)
        {
            if (param == null) return;
            param.ValidateId();
            parameters.Add(param);
            NotifyModified();
        }

        public void RemoveParameter(ParameterId paramId)
        {
            parameters.RemoveAll(p => p != null && p.Id == paramId);
            NotifyModified();
        }

        public void AddNodeDirectly(ShapeNode node)
        {
            if (node == null) return;
            if (!nodes.Contains(node))
            {
                nodes.Add(node);
                NotifyModified();
            }
        }

        public void RemoveNodeDirectly(ShapeNode node)
        {
            if (node == null) return;
            if (nodes.Contains(node))
            {
                connections.RemoveAll(c => c.FromNodeId == node.Id || c.ToNodeId == node.Id);
                nodes.Remove(node);
                NotifyModified();
            }
        }

        public bool AddConnectionDirectly(NodeConnection conn)
        {
            if (!GraphConnectionValidator.TryValidate(this, conn, out ConnectionError error))
            {
                Debug.LogWarning($"[ShapeGraph] Rejected connection ({error}) on asset '{name}'.");
                return false;
            }

            connections.RemoveAll(c => c.ToNodeId == conn.ToNodeId && c.ToPortId == conn.ToPortId);
            connections.Add(conn);
            NotifyModified();
            return true;
        }

        public void RemoveConnectionDirectly(NodeConnection conn)
        {
            if (connections.Remove(conn))
            {
                NotifyModified();
            }
        }

        public int SanitizeOrphanConnections()
        {
            var portBuffer = new List<NodePort>(16);
            int removed = 0;
            for (int i = connections.Count - 1; i >= 0; i--)
            {
                NodeConnection c = connections[i];
                ShapeNode from = GetNodeById(c.FromNodeId);
                ShapeNode to = GetNodeById(c.ToNodeId);
                if (from == null || to == null ||
                    !HasPort(from, c.FromPortId, PortDirection.Output, portBuffer) ||
                    !HasPort(to, c.ToPortId, PortDirection.Input, portBuffer))
                {
                    connections.RemoveAt(i);
                    removed++;
                }
            }

            if (removed > 0)
            {
                Debug.LogWarning(
                    $"[ShapeGraph] Removed {removed} orphan connection(s) from '{name}'.");
                NotifyModified();
            }

            return removed;
        }

        public void NotifyModified() => OnGraphModified?.Invoke();

        private static bool HasPort(ShapeNode node, PortId portId, PortDirection direction, List<NodePort> buffer)
        {
            buffer.Clear();
            node.CollectPorts(buffer);
            for (int i = 0; i < buffer.Count; i++)
            {
                if (buffer[i].Id == portId && buffer[i].Direction == direction) return true;
            }

            return false;
        }

        #endregion
    }
}

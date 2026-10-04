using System;
using System.Collections.Generic;
using Galaretka.ShapeGraph.Components;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Data;
using Galaretka.ShapeGraph.Data.Nodes.Values;
using Galaretka.ShapeGraph.Editor.Graph.Model;
using Galaretka.ShapeGraph.Typing;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Galaretka.ShapeGraph.Editor.Graph.Commands
{
    public enum GraphChangeKind
    {
        Bound,
        Structure,
        Properties,
        Layout
    }

    public sealed class GraphCommandService
    {
        private ShapeGraphAsset asset;
        private readonly GraphDirtyTracker dirtyTracker;

        public GraphCommandService(GraphDirtyTracker dirtyTracker)
        {
            this.dirtyTracker = dirtyTracker ?? throw new ArgumentNullException(nameof(dirtyTracker));
        }

        public ShapeGraphAsset Asset => asset;
        public GraphDirtyTracker Dirty => dirtyTracker;

        public event Action<GraphChangeKind> GraphChanged;

        public void Bind(ShapeGraphAsset graphAsset)
        {
            asset = graphAsset;
            if (asset != null)
            {
                asset.SanitizeOrphanConnections();
            }
            dirtyTracker.InvalidateAll(asset);
            RaiseChanged(GraphChangeKind.Bound);
        }

        public ShapeNode CreateNode(Type nodeType, Vector2 graphPosition)
        {
            if (asset == null) throw new InvalidOperationException("[ShapeGraph] No graph asset bound.");
            if (nodeType == null || !typeof(ShapeNode).IsAssignableFrom(nodeType) || nodeType.IsAbstract) throw new ArgumentException("Invalid node type.", nameof(nodeType));
            if (nodeType == typeof(ParameterNode))
            {
                throw new ArgumentException("Use CreateParameterNode for ParameterNode.", nameof(nodeType));
            }

            Undo.RegisterCompleteObjectUndo(asset, "Shape Graph Create Node");

            var node = (ShapeNode)ScriptableObject.CreateInstance(nodeType);
            node.name = nodeType.Name;
            node.hideFlags = HideFlags.HideInHierarchy;
            node.GraphPosition = graphPosition;
            node.OnEnable();

            AssetDatabase.AddObjectToAsset(node, asset);
            Undo.RegisterCreatedObjectUndo(node, "Shape Graph Create Node");

            asset.AddNodeDirectly(node);
            PersistAsset(node);
            dirtyTracker.BumpStructure();
            RaiseChanged(GraphChangeKind.Structure);
            return node;
        }

        public ParameterNode CreateParameterNode(GraphParameter parameter, Vector2 graphPosition)
        {
            if (asset == null) throw new InvalidOperationException("[ShapeGraph] No graph asset bound.");
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));

            Undo.RegisterCompleteObjectUndo(asset, "Shape Graph Create Parameter Node");

            var node = ScriptableObject.CreateInstance<ParameterNode>();
            node.hideFlags = HideFlags.HideInHierarchy;
            node.GraphPosition = graphPosition;
            node.OnEnable();
            node.BindParameter(parameter.Name, parameter.ValueType);

            AssetDatabase.AddObjectToAsset(node, asset);
            Undo.RegisterCreatedObjectUndo(node, "Shape Graph Create Parameter Node");

            asset.AddNodeDirectly(node);
            PersistAsset(node);
            dirtyTracker.BumpStructure();
            RaiseChanged(GraphChangeKind.Structure);
            return node;
        }

        public ShapeNode CreateNodeAndConnect(
            Type nodeType,
            GraphParameter bindParameter,
            Vector2 graphPosition,
            NodeId fromNode,
            PortId fromPort,
            PortId toPort)
        {
            if (asset == null) throw new InvalidOperationException("[ShapeGraph] No graph asset bound.");
            if (bindParameter == null)
            {
                if (nodeType == null || !typeof(ShapeNode).IsAssignableFrom(nodeType) || nodeType.IsAbstract)
                {
                    throw new ArgumentException("Invalid node type.", nameof(nodeType));
                }
                if (nodeType == typeof(ParameterNode))
                {
                    throw new ArgumentException("Use bindParameter for ParameterNode.", nameof(nodeType));
                }
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Shape Graph Create And Connect");
            Undo.RegisterCompleteObjectUndo(asset, "Shape Graph Create And Connect");

            ShapeNode node;
            if (bindParameter != null)
            {
                var parameterNode = ScriptableObject.CreateInstance<ParameterNode>();
                parameterNode.hideFlags = HideFlags.HideInHierarchy;
                parameterNode.GraphPosition = graphPosition;
                parameterNode.OnEnable();
                parameterNode.BindParameter(bindParameter.Name, bindParameter.ValueType);
                node = parameterNode;
            }
            else
            {
                node = (ShapeNode)ScriptableObject.CreateInstance(nodeType);
                node.name = nodeType.Name;
                node.hideFlags = HideFlags.HideInHierarchy;
                node.GraphPosition = graphPosition;
                node.OnEnable();
            }

            AssetDatabase.AddObjectToAsset(node, asset);
            Undo.RegisterCreatedObjectUndo(node, "Shape Graph Create And Connect");
            asset.AddNodeDirectly(node);

            var connection = new NodeConnection(fromNode, fromPort, node.Id, toPort);
            if (!asset.AddConnectionDirectly(connection))
            {
                asset.RemoveNodeDirectly(node);
                Undo.DestroyObjectImmediate(node);
                Undo.CollapseUndoOperations(undoGroup);
                PersistAsset();
                dirtyTracker.BumpStructure();
                RaiseChanged(GraphChangeKind.Structure);
                return null;
            }

            PersistAsset(node);
            dirtyTracker.BumpStructure();
            dirtyTracker.InvalidateNode(node.Id, asset);
            Undo.CollapseUndoOperations(undoGroup);
            RaiseChanged(GraphChangeKind.Structure);
            return node;
        }

        public void DeleteNode(ShapeNode node)
        {
            if (asset == null || node == null) return;

            Undo.RegisterCompleteObjectUndo(asset, "Shape Graph Delete Node");
            asset.RemoveNodeDirectly(node);
            Undo.DestroyObjectImmediate(node);

            PersistAsset();
            dirtyTracker.BumpStructure();
            RaiseChanged(GraphChangeKind.Structure);
        }

        public bool Connect(NodeId fromNode, PortId fromPort, NodeId toNode, PortId toPort)
        {
            if (asset == null) return false;

            var connection = new NodeConnection(fromNode, fromPort, toNode, toPort);
            Undo.RegisterCompleteObjectUndo(asset, "Shape Graph Connect");
            if (!asset.AddConnectionDirectly(connection)) return false;

            PersistAsset();
            dirtyTracker.BumpStructure();
            dirtyTracker.InvalidateNode(toNode, asset);
            RaiseChanged(GraphChangeKind.Structure);
            return true;
        }

        public void Disconnect(NodeConnection connection)
        {
            if (asset == null) return;

            Undo.RegisterCompleteObjectUndo(asset, "Shape Graph Disconnect");
            asset.RemoveConnectionDirectly(connection);
            PersistAsset();
            dirtyTracker.BumpStructure();
            dirtyTracker.InvalidateNode(connection.ToNodeId, asset);
            RaiseChanged(GraphChangeKind.Structure);
        }

        public void SetNodePosition(ShapeNode node, Vector2 position)
        {
            if (asset == null || node == null) return;

            if ((node.GraphPosition - position).sqrMagnitude < 0.0001f) return;

            Undo.RegisterCompleteObjectUndo(node, "Shape Graph Move Node");
            node.GraphPosition = position;
            EditorUtility.SetDirty(node);
            EditorUtility.SetDirty(asset);
            RaiseChanged(GraphChangeKind.Layout);
        }

        public void NotifyNodePropertiesChanged(ShapeNode node)
        {
            if (asset == null || node == null) return;

            node.MarkDirty();
            EditorUtility.SetDirty(node);
            EditorUtility.SetDirty(asset);
            dirtyTracker.InvalidateNode(node.Id, asset);
            RaiseChanged(GraphChangeKind.Properties);
        }

        public void NotifyNodePortsChanged(ShapeNode node)
        {
            if (asset == null || node == null) return;

            node.MarkDirty();
            EditorUtility.SetDirty(node);
            PersistAsset(node);
            dirtyTracker.BumpStructure();
            dirtyTracker.InvalidateNode(node.Id, asset);
            RaiseChanged(GraphChangeKind.Structure);
        }

        public GraphParameter AddParameter(string name = "NewParameter", ParameterValueType type = ParameterValueType.Float)
        {
            if (asset == null) return null;

            Undo.RegisterCompleteObjectUndo(asset, "Shape Graph Add Parameter");
            string unique = MakeUniqueParameterName(name);
            var param = new GraphParameter(unique, type, ParameterValue.DefaultFor(type));
            param.SetMode(ParameterMode.Exposed);
            asset.AddParameter(param);
            PersistAsset();
            dirtyTracker.InvalidateAll(asset);
            RaiseChanged(GraphChangeKind.Properties);
            return param;
        }

        public void RemoveParameter(ParameterId paramId)
        {
            if (asset == null) return;

            Undo.RegisterCompleteObjectUndo(asset, "Shape Graph Remove Parameter");
            asset.RemoveParameter(paramId);
            PersistAsset();
            dirtyTracker.InvalidateAll(asset);
            RaiseChanged(GraphChangeKind.Properties);
        }

        public void UpdateParameter(ParameterId paramId, Action<GraphParameter> mutator)
        {
            if (asset == null || mutator == null) return;

            GraphParameter param = FindParameter(paramId);
            if (param == null) return;

            string beforeName = param.Name;
            ParameterValueType beforeType = param.ValueType;
            Undo.RegisterCompleteObjectUndo(asset, "Shape Graph Edit Parameter");
            mutator(param);
            param.ValidateId();

            bool portsChanged = param.ValueType != beforeType || param.Name != beforeName;
            SyncParameterNodes(beforeName, param);
            if (portsChanged)
            {
                DisconnectIncompatibleParameterEdges(param);
            }

            PersistAsset();
            dirtyTracker.InvalidateAll(asset);
            RaiseChanged(portsChanged ? GraphChangeKind.Structure : GraphChangeKind.Properties);
        }

        private void SyncParameterNodes(string previousName, GraphParameter param)
        {
            IReadOnlyList<ShapeNode> nodes = asset.Nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] is not ParameterNode parameterNode) continue;
                if (parameterNode.ParameterName != previousName && parameterNode.ParameterName != param.Name) continue;

                parameterNode.BindParameter(param.Name, param.ValueType);
                parameterNode.name = param.Name;
                EditorUtility.SetDirty(parameterNode);
            }
        }

        public static void RebuildSceneInstances(ShapeGraphAsset graph)
        {
            if (graph == null) return;

            ProceduralShapeInstance[] instances = Object.FindObjectsByType<ProceduralShapeInstance>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < instances.Length; i++)
            {
                if (instances[i] != null && instances[i].Graph == graph)
                {
                    instances[i].Rebuild();
                }
            }
        }

        private GraphParameter FindParameter(ParameterId paramId)
        {
            IReadOnlyList<GraphParameter> list = asset.Parameters;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null && list[i].Id == paramId)
                {
                    return list[i];
                }
            }

            return null;
        }

        private string MakeUniqueParameterName(string baseName)
        {
            if (string.IsNullOrWhiteSpace(baseName))
            {
                baseName = "NewParameter";
            }

            baseName = baseName.Trim();
            if (asset.FindParameterByName(baseName) == null) return baseName;

            for (int i = 2; i < 1000; i++)
            {
                string candidate = $"{baseName}_{i}";
                if (asset.FindParameterByName(candidate) == null) return candidate;
            }

            return $"{baseName}_{Guid.NewGuid():N}".Substring(0, 24);
        }

        private void DisconnectIncompatibleParameterEdges(GraphParameter param)
        {
            // ParameterNode ports rebuild on type change; drop edges that no longer validate.
            var doomed = new List<NodeConnection>();
            IReadOnlyList<NodeConnection> conns = asset.Connections;
            for (int i = 0; i < conns.Count; i++)
            {
                if (!GraphConnectionValidator.TryValidate(asset, conns[i], out _))
                {
                    doomed.Add(conns[i]);
                }
            }

            for (int i = 0; i < doomed.Count; i++)
            {
                asset.RemoveConnectionDirectly(doomed[i]);
            }

            if (doomed.Count > 0)
            {
                Debug.LogWarning(
                    $"[ShapeGraph] Disconnected {doomed.Count} edge(s) after parameter '{param.Name}' type change.");
                dirtyTracker.BumpStructure();
            }
        }

        private void PersistAsset(Object extra = null)
        {
            if (asset == null) return;

            EditorUtility.SetDirty(asset);
            if (extra != null)
            {
                EditorUtility.SetDirty(extra);
            }
            AssetDatabase.SaveAssetIfDirty(asset);
        }

        private void RaiseChanged(GraphChangeKind kind) => GraphChanged?.Invoke(kind);
    }
}

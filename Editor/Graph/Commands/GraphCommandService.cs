using System;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Data;
using Galaretka.ShapeGraph.Editor.Graph.Model;
using UnityEditor;
using UnityEngine;

namespace Galaretka.ShapeGraph.Editor.Graph.Commands
{
    public enum GraphChangeKind
    {
        Bound,
        Structure,
        Properties,
        Layout
    }

    /// <summary>
    /// Sole mutation path for graph documents. Views never touch asset lists or Undo directly.
    /// </summary>
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
            dirtyTracker.InvalidateAll(asset);
            RaiseChanged(GraphChangeKind.Bound);
        }

        public ShapeNode CreateNode(Type nodeType, Vector2 graphPosition)
        {
            if (asset == null)
                throw new InvalidOperationException("[ShapeGraph] No graph asset bound.");
            if (nodeType == null || !typeof(ShapeNode).IsAssignableFrom(nodeType) || nodeType.IsAbstract)
                throw new ArgumentException("Invalid node type.", nameof(nodeType));

            Undo.RegisterCompleteObjectUndo(asset, "Shape Graph Create Node");

            var node = (ShapeNode)ScriptableObject.CreateInstance(nodeType);
            node.name = nodeType.Name;
            node.hideFlags = HideFlags.HideInHierarchy;
            node.GraphPosition = graphPosition;
            node.OnEnable();

            AssetDatabase.AddObjectToAsset(node, asset);
            Undo.RegisterCreatedObjectUndo(node, "Shape Graph Create Node");

            asset.AddNodeDirectly(node);
            EditorUtility.SetDirty(asset);
            EditorUtility.SetDirty(node);
            dirtyTracker.BumpStructure();
            RaiseChanged(GraphChangeKind.Structure);
            return node;
        }

        public void DeleteNode(ShapeNode node)
        {
            if (asset == null || node == null)
                return;

            Undo.RegisterCompleteObjectUndo(asset, "Shape Graph Delete Node");
            asset.RemoveNodeDirectly(node);
            Undo.DestroyObjectImmediate(node);

            EditorUtility.SetDirty(asset);
            dirtyTracker.BumpStructure();
            RaiseChanged(GraphChangeKind.Structure);
        }

        public bool Connect(NodeId fromNode, PortId fromPort, NodeId toNode, PortId toPort)
        {
            if (asset == null)
                return false;

            var connection = new NodeConnection(fromNode, fromPort, toNode, toPort);
            Undo.RegisterCompleteObjectUndo(asset, "Shape Graph Connect");
            if (!asset.AddConnectionDirectly(connection))
                return false;

            EditorUtility.SetDirty(asset);
            dirtyTracker.BumpStructure();
            dirtyTracker.InvalidateNode(toNode, asset);
            RaiseChanged(GraphChangeKind.Structure);
            return true;
        }

        public void Disconnect(NodeConnection connection)
        {
            if (asset == null)
                return;

            Undo.RegisterCompleteObjectUndo(asset, "Shape Graph Disconnect");
            asset.RemoveConnectionDirectly(connection);
            EditorUtility.SetDirty(asset);
            dirtyTracker.BumpStructure();
            dirtyTracker.InvalidateNode(connection.ToNodeId, asset);
            RaiseChanged(GraphChangeKind.Structure);
        }

        public void SetNodePosition(ShapeNode node, Vector2 position)
        {
            if (asset == null || node == null)
                return;

            if ((node.GraphPosition - position).sqrMagnitude < 0.0001f)
                return;

            Undo.RegisterCompleteObjectUndo(node, "Shape Graph Move Node");
            node.GraphPosition = position;
            EditorUtility.SetDirty(node);
            EditorUtility.SetDirty(asset);
            RaiseChanged(GraphChangeKind.Layout);
        }

        public void NotifyNodePropertiesChanged(ShapeNode node)
        {
            if (asset == null || node == null)
                return;

            node.MarkDirty();
            EditorUtility.SetDirty(node);
            EditorUtility.SetDirty(asset);
            dirtyTracker.InvalidateNode(node.Id, asset);
            RaiseChanged(GraphChangeKind.Properties);
        }

        private void RaiseChanged(GraphChangeKind kind) => GraphChanged?.Invoke(kind);
    }
}

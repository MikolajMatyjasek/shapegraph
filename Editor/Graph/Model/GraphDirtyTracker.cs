using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Data;

namespace Galaretka.ShapeGraph.Editor.Graph.Model
{
    /// <summary>
    /// Tracks graph structure version and computes downstream invalidation sets.
    /// </summary>
    public sealed class GraphDirtyTracker
    {
        private readonly HashSet<NodeId> pending = new();
        private bool masterDirty = true;

        public uint StructureVersion { get; private set; } = 1;
        public uint PreviewRevision { get; private set; } = 1;

        public void BumpStructure()
        {
            unchecked { StructureVersion++; }
            masterDirty = true;
            PreviewRevision++;
        }

        public void InvalidateNode(NodeId nodeId, ShapeGraphAsset asset)
        {
            if (asset == null || !nodeId.IsValid)
                return;

            pending.Add(nodeId);
            CollectDownstream(asset, nodeId, pending);
            masterDirty = true;
            unchecked { PreviewRevision++; }
        }

        public void InvalidateAll(ShapeGraphAsset asset)
        {
            pending.Clear();
            if (asset != null)
            {
                for (int i = 0; i < asset.Nodes.Count; i++)
                {
                    if (asset.Nodes[i] != null)
                        pending.Add(asset.Nodes[i].Id);
                }
            }

            masterDirty = true;
            unchecked { PreviewRevision++; }
        }

        public bool ConsumeMasterDirty()
        {
            bool value = masterDirty;
            masterDirty = false;
            return value;
        }

        public void DrainPending(List<NodeId> destination)
        {
            destination.Clear();
            foreach (NodeId id in pending)
                destination.Add(id);
            pending.Clear();
        }

        public bool HasPending => pending.Count > 0 || masterDirty;

        private static void CollectDownstream(ShapeGraphAsset asset, NodeId start, HashSet<NodeId> results)
        {
            var queue = new Queue<NodeId>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                NodeId current = queue.Dequeue();
                IReadOnlyList<NodeConnection> connections = asset.Connections;
                for (int i = 0; i < connections.Count; i++)
                {
                    NodeConnection c = connections[i];
                    if (c.FromNodeId != current)
                        continue;
                    if (results.Add(c.ToNodeId))
                        queue.Enqueue(c.ToNodeId);
                }
            }
        }
    }
}

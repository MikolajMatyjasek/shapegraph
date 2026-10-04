using System;
using Galaretka.ShapeGraph.Core;

namespace Galaretka.ShapeGraph.Data
{
    [Serializable]
    public struct NodeConnection : IEquatable<NodeConnection>
    {
        public NodeId FromNodeId;
        public PortId FromPortId;
        public NodeId ToNodeId;
        public PortId ToPortId;

        public NodeConnection(NodeId fromNodeId, PortId fromPortId, NodeId toNodeId, PortId toPortId)
        {
            FromNodeId = fromNodeId;
            FromPortId = fromPortId;
            ToNodeId = toNodeId;
            ToPortId = toPortId;
        }

        public bool Equals(NodeConnection other) =>
            FromNodeId == other.FromNodeId &&
            FromPortId == other.FromPortId &&
            ToNodeId == other.ToNodeId &&
            ToPortId == other.ToPortId;

        public override bool Equals(object obj) => obj is NodeConnection other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = FromNodeId.GetHashCode();
                hash = (hash * 397) ^ FromPortId.GetHashCode();
                hash = (hash * 397) ^ ToNodeId.GetHashCode();
                hash = (hash * 397) ^ ToPortId.GetHashCode();
                return hash;
            }
        }
    }
}

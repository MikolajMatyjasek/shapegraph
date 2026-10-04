using System;

namespace Galaretka.ShapeGraph.Core
{
    public readonly struct PortKey : IEquatable<PortKey>
    {
        public readonly NodeId NodeId;
        public readonly PortId PortId;

        public PortKey(NodeId nodeId, PortId portId)
        {
            NodeId = nodeId;
            PortId = portId;
        }

        public bool Equals(PortKey other) => NodeId == other.NodeId && PortId == other.PortId;
        public override bool Equals(object obj) => obj is PortKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (NodeId.GetHashCode() * 397) ^ PortId.GetHashCode();
            }
        }

        public static bool operator ==(PortKey left, PortKey right) => left.Equals(right);
        public static bool operator !=(PortKey left, PortKey right) => !left.Equals(right);
    }
}
using System;
using UnityEngine;

namespace Galaretka.ShapeGraph.Core
{
    [Serializable]
    public readonly struct NodeId : IEquatable<NodeId>
    {
        [SerializeField] 
        private readonly ulong partA;
        [SerializeField] 
        private readonly ulong partB;

        public bool IsValid => partA != 0 || partB != 0;

        public NodeId(ulong a, ulong b)
        {
            partA = a;
            partB = b;
        }

        public static NodeId NewId()
        {
            byte[] bytes = Guid.NewGuid().ToByteArray();
            ulong a = BitConverter.ToUInt64(bytes, 0);
            ulong b = BitConverter.ToUInt64(bytes, 8);
            return new NodeId(a, b);
        }

        public bool Equals(NodeId other) => partA == other.partA && partB == other.partB;
        public override bool Equals(object obj) => obj is NodeId other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (partA.GetHashCode() * 397) ^ partB.GetHashCode();
            }
        }

        public static bool operator ==(NodeId left, NodeId right) => left.Equals(right);
        public static bool operator !=(NodeId left, NodeId right) => !left.Equals(right);
        public override string ToString() => $"{partA:X16}{partB:X16}";
    }
}
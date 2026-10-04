using System;
using UnityEngine;

namespace Galaretka.ShapeGraph.Typing
{
    [Serializable]
    public readonly struct PortTypeId : IEquatable<PortTypeId>
    {
        [SerializeField]
        private readonly int hash;

        public int Hash => hash;

        internal PortTypeId(int rawHash) => hash = rawHash;

        public bool Equals(PortTypeId other) => hash == other.hash;
        public override bool Equals(object obj) => obj is PortTypeId other && Equals(other);
        public override int GetHashCode() => hash;
        public override string ToString() => $"PortTypeId(0x{hash:X8})";

        public static bool operator ==(PortTypeId left, PortTypeId right) => left.hash == right.hash;
        public static bool operator !=(PortTypeId left, PortTypeId right) => left.hash != right.hash;
    }
}

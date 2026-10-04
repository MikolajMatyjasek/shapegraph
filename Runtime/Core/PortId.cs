using System;
using UnityEngine;

namespace Galaretka.ShapeGraph.Core
{
    [Serializable]
    public readonly struct PortId : IEquatable<PortId>
    {
        [SerializeField] 
        private readonly int hash;

        public int Hash => hash;

        public PortId(string name) => hash = ComputeFnv1aHash(name);
        public PortId(int rawHash) => hash = rawHash;

        public static PortId FromString(string name) => new(name);

        public bool Equals(PortId other) => hash == other.hash;
        public override bool Equals(object obj) => obj is PortId other && Equals(other);
        public override int GetHashCode() => hash;
        public override string ToString() => $"PortId(0x{hash:X8})";

        public static bool operator ==(PortId left, PortId right) => left.hash == right.hash;
        public static bool operator !=(PortId left, PortId right) => left.hash != right.hash;

        private static int ComputeFnv1aHash(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            unchecked
            {
                uint h = 2166136261u;
                for (int i = 0; i < text.Length; i++)
                {
                    h ^= text[i];
                    h *= 16777619u;
                }
                return (int)h;
            }
        }
    }
}
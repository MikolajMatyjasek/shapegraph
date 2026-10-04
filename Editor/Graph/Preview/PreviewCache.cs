using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using UnityEngine;

namespace Galaretka.ShapeGraph.Editor.Graph.Preview
{
    public readonly struct PreviewCacheKey : System.IEquatable<PreviewCacheKey>
    {
        public readonly NodeId NodeId;
        public readonly uint NodeVersion;
        public readonly uint StructureVersion;
        public readonly int Seed;
        public readonly uint PreviewRevision;
        public readonly bool IsMaster;

        public PreviewCacheKey(NodeId nodeId, uint nodeVersion, uint structureVersion, int seed, uint previewRevision, bool isMaster)
        {
            NodeId = nodeId;
            NodeVersion = nodeVersion;
            StructureVersion = structureVersion;
            Seed = seed;
            PreviewRevision = previewRevision;
            IsMaster = isMaster;
        }

        public bool Equals(PreviewCacheKey other) =>
            NodeId == other.NodeId &&
            NodeVersion == other.NodeVersion &&
            StructureVersion == other.StructureVersion &&
            Seed == other.Seed &&
            PreviewRevision == other.PreviewRevision &&
            IsMaster == other.IsMaster;

        public override bool Equals(object obj) => obj is PreviewCacheKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = NodeId.GetHashCode();
                hash = (hash * 397) ^ (int)NodeVersion;
                hash = (hash * 397) ^ (int)StructureVersion;
                hash = (hash * 397) ^ Seed;
                hash = (hash * 397) ^ (int)PreviewRevision;
                hash = (hash * 397) ^ (IsMaster ? 1 : 0);
                return hash;
            }
        }
    }

    /// <summary>
    /// One live RT slot per node (plus master). Replaces in place so we never
    /// destroy a texture still bound to a node Image via LRU eviction.
    /// </summary>
    public sealed class PreviewCache
    {
        private const string MasterSlot = "__master__";

        private struct Entry
        {
            public PreviewCacheKey Fingerprint;
            public RenderTexture Rt;
            public bool HasFingerprint;
        }

        private readonly Dictionary<string, Entry> slots = new();

        public bool TryGet(PreviewCacheKey key, out RenderTexture rt)
        {
            string slot = SlotOf(key);
            if (slots.TryGetValue(slot, out Entry entry) &&
                entry.HasFingerprint &&
                entry.Fingerprint.Equals(key) &&
                entry.Rt != null)
            {
                rt = entry.Rt;
                return true;
            }

            rt = null;
            return false;
        }

        public void Set(PreviewCacheKey key, RenderTexture rt)
        {
            string slot = SlotOf(key);
            if (slots.TryGetValue(slot, out Entry existing) &&
                existing.Rt != null &&
                existing.Rt != rt)
            {
                Release(existing.Rt);
            }

            slots[slot] = new Entry
            {
                Fingerprint = key,
                Rt = rt,
                HasFingerprint = true
            };
        }

        /// <summary>
        /// Marks slots stale so the next rebuild regenerates. Keeps RTs alive until
        /// <see cref="Set"/> replaces them (avoids blank Images while queued).
        /// </summary>
        public void InvalidateNode(NodeId nodeId)
        {
            MarkStale(SlotOfNode(nodeId, isMaster: false));
            MarkStale(MasterSlot);
        }

        public void ReleaseSlot(NodeId nodeId, bool isMaster)
        {
            string slot = SlotOfNode(nodeId, isMaster);
            if (!slots.TryGetValue(slot, out Entry entry))
                return;

            Release(entry.Rt);
            slots.Remove(slot);
        }

        public void Clear()
        {
            foreach (var kv in slots)
                Release(kv.Value.Rt);
            slots.Clear();
        }

        private void MarkStale(string slot)
        {
            if (!slots.TryGetValue(slot, out Entry entry))
                return;

            entry.HasFingerprint = false;
            slots[slot] = entry;
        }

        private static string SlotOf(PreviewCacheKey key) => SlotOfNode(key.NodeId, key.IsMaster);

        private static string SlotOfNode(NodeId nodeId, bool isMaster) =>
            isMaster ? MasterSlot : nodeId.ToString();

        private static void Release(RenderTexture rt)
        {
            if (rt == null)
                return;
            rt.Release();
            Object.DestroyImmediate(rt);
        }
    }
}

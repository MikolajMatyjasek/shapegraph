using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Evaluation
{
    public sealed class ShapeContext
    {
        private readonly IReadOnlyDictionary<ParameterId, ParameterValue> parameterOverrides;
        private readonly Dictionary<PortKey, IShape2D> shapeCache = new();
        private readonly Dictionary<PortKey, ParameterValue> valueCache = new();
        private readonly Dictionary<PortKey, Color32> color32Cache = new();

        public int GlobalSeed { get; }

        public ShapeContext(int globalSeed, IReadOnlyDictionary<ParameterId, ParameterValue> overrides = null)
        {
            GlobalSeed = globalSeed;
            parameterOverrides = overrides;
        }

        public bool TryGetOverride(ParameterId id, out ParameterValue value)
        {
            if (parameterOverrides != null && parameterOverrides.TryGetValue(id, out value)) return true;

            value = default;
            return false;
        }

        public bool TryGetShape(PortKey key, out IShape2D value) => shapeCache.TryGetValue(key, out value);
        public void SetShape(PortKey key, IShape2D value) => shapeCache[key] = value;

        public bool TryGetValue(PortKey key, out ParameterValue value) => valueCache.TryGetValue(key, out value);
        public void SetValue(PortKey key, ParameterValue value) => valueCache[key] = value;

        public bool TryGetColor32(PortKey key, out Color32 value) => color32Cache.TryGetValue(key, out value);
        public void SetColor32(PortKey key, Color32 value) => color32Cache[key] = value;

        public float Next01(ParameterId paramId) =>
            DeterministicRng.Hash01(GlobalSeed, paramId.Hash);

        public float Next01(NodeId nodeId, int streamIndex) =>
            DeterministicRng.Hash01(GlobalSeed, nodeId.GetHashCode(), streamIndex);

        public float NextRange(ParameterId paramId, float min, float max) =>
            Mathf.Lerp(min, max, Next01(paramId));

        public int NextIntInclusive(ParameterId paramId, int min, int max)
        {
            if (max < min)
            {
                int tmp = min;
                min = max;
                max = tmp;
            }

            int span = max - min + 1;
            if (span <= 1)
            {
                return min;
            }

            float t = Next01(paramId);
            int offset = Mathf.FloorToInt(t * span);
            if (offset >= span)
            {
                offset = span - 1;
            }
            return min + offset;
        }
    }
}

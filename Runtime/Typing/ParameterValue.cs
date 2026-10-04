using System;
using UnityEngine;

namespace Galaretka.ShapeGraph.Typing
{
    [Serializable]
    public struct ParameterValue : IEquatable<ParameterValue>
    {
        [SerializeField] 
        private ParameterValueType type;
        [SerializeField] 
        private float f0;
        [SerializeField] 
        private float f1;
        [SerializeField] 
        private float f2;
        [SerializeField] 
        private float f3;
        [SerializeField] 
        private int i0;

        public ParameterValueType Type => type;

        public static ParameterValue FromFloat(float v) => new()
        {
            type = ParameterValueType.Float,
            f0 = v
        };

        public static ParameterValue FromInt(int v) => new()
        {
            type = ParameterValueType.Int,
            i0 = v
        };

        public static ParameterValue FromBool(bool v) => new()
        {
            type = ParameterValueType.Bool,
            i0 = v ? 1 : 0
        };

        public static ParameterValue FromVector2(Vector2 v) => new()
        {
            type = ParameterValueType.Vector2,
            f0 = v.x,
            f1 = v.y
        };

        public static ParameterValue FromColor(Color v) => new()
        {
            type = ParameterValueType.Color,
            f0 = v.r,
            f1 = v.g,
            f2 = v.b,
            f3 = v.a
        };

        public static ParameterValue DefaultFor(ParameterValueType valueType) => valueType switch
        {
            ParameterValueType.Float => FromFloat(0f),
            ParameterValueType.Int => FromInt(0),
            ParameterValueType.Bool => FromBool(false),
            ParameterValueType.Vector2 => FromVector2(Vector2.zero),
            ParameterValueType.Color => FromColor(Color.white),
            _ => FromFloat(0f)
        };

        public float AsFloat() => type == ParameterValueType.Float ? f0 : 0f;
        public int AsInt() => type == ParameterValueType.Int ? i0 : 0;
        public bool AsBool() => type == ParameterValueType.Bool && i0 != 0;
        public Vector2 AsVector2() => type == ParameterValueType.Vector2 ? new Vector2(f0, f1) : Vector2.zero;
        public Color AsColor() => type == ParameterValueType.Color ? new Color(f0, f1, f2, f3) : Color.white;

        public bool TryGet<T>(out T value)
        {
            Type t = typeof(T);
            if (t == typeof(float) && type == ParameterValueType.Float)
            {
                value = (T)(object)AsFloat();
                return true;
            }

            if (t == typeof(int) && type == ParameterValueType.Int)
            {
                value = (T)(object)AsInt();
                return true;
            }

            if (t == typeof(bool) && type == ParameterValueType.Bool)
            {
                value = (T)(object)AsBool();
                return true;
            }

            if (t == typeof(Vector2) && type == ParameterValueType.Vector2)
            {
                value = (T)(object)AsVector2();
                return true;
            }

            if (t == typeof(Color) && type == ParameterValueType.Color)
            {
                value = (T)(object)AsColor();
                return true;
            }

            value = default;
            return false;
        }

        public bool Equals(ParameterValue other) =>
            type == other.type &&
            i0 == other.i0 &&
            Mathf.Approximately(f0, other.f0) &&
            Mathf.Approximately(f1, other.f1) &&
            Mathf.Approximately(f2, other.f2) &&
            Mathf.Approximately(f3, other.f3);

        public override bool Equals(object obj) => obj is ParameterValue other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)type;
                hash = (hash * 397) ^ i0;
                hash = (hash * 397) ^ f0.GetHashCode();
                hash = (hash * 397) ^ f1.GetHashCode();
                hash = (hash * 397) ^ f2.GetHashCode();
                hash = (hash * 397) ^ f3.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(ParameterValue left, ParameterValue right) => left.Equals(right);
        public static bool operator !=(ParameterValue left, ParameterValue right) => !left.Equals(right);
    }
}

using UnityEngine;

namespace Galaretka.ShapeGraph.Core.Geometry
{
    public static class ShapeMath
    {
        public const int MinSides = 3;
        public const int MaxSides = 256;

        public static float ClampRadius(float radius) => Mathf.Max(0f, radius);

        public static float ClampFactor(float factor) => Mathf.Max(0f, factor);

        public static int ClampSides(int sides, int min = MinSides, int max = MaxSides) =>
            Mathf.Clamp(sides, min, max);

        public static int ClampSegments(int segments, int min = MinSides, int max = MaxSides) =>
            Mathf.Clamp(segments, min, max);

        public static bool IsFinite(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value);

        public static bool IsFinite(Vector2 value) =>
            IsFinite(value.x) && IsFinite(value.y);

        public static float DegToRad(float degrees) => degrees * Mathf.Deg2Rad;

        public static Vector2 Rotate(Vector2 v, float degrees)
        {
            float rad = DegToRad(degrees);
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        public static bool IsUniformScale(Vector2 scale, float epsilon = 1e-4f) =>
            Mathf.Abs(scale.x - scale.y) <= epsilon;
    }
}

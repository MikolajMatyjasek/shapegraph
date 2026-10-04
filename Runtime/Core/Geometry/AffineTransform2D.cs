using UnityEngine;

namespace Galaretka.ShapeGraph.Core.Geometry
{
    public readonly struct AffineTransform2D
    {
        public readonly Vector2 Translation;
        public readonly float RotationDegrees;
        public readonly Vector2 Scale;
        public readonly Vector2 Pivot;

        public AffineTransform2D(Vector2 translation, float rotationDegrees, Vector2 scale, Vector2 pivot)
        {
            Translation = translation;
            RotationDegrees = rotationDegrees;
            Scale = scale;
            Pivot = pivot;
        }

        public static AffineTransform2D Identity =>
            new(Vector2.zero, 0f, Vector2.one, Vector2.zero);

        public Vector2 Apply(Vector2 point)
        {
            Vector2 local = point - Pivot;
            local = new Vector2(local.x * Scale.x, local.y * Scale.y);
            local = ShapeMath.Rotate(local, RotationDegrees);
            return local + Pivot + Translation;
        }

        public bool HasUniformScale(float epsilon = 1e-4f) =>
            ShapeMath.IsUniformScale(Scale, epsilon);

        public bool IsIdentity(float epsilon = 1e-6f) =>
            Translation.sqrMagnitude <= epsilon * epsilon &&
            Mathf.Abs(RotationDegrees) <= epsilon &&
            Mathf.Abs(Scale.x - 1f) <= epsilon &&
            Mathf.Abs(Scale.y - 1f) <= epsilon;
    }
}

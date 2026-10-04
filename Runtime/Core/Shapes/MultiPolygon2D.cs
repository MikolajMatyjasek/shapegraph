using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galaretka.ShapeGraph.Core
{
    public sealed class MultiPolygon2D : IShape2D
    {
        [SerializeField]
        private List<Polygon2D> polygons = new();

        public IReadOnlyList<Polygon2D> Polygons => polygons;

        public bool IsValid
        {
            get
            {
                if (polygons == null || polygons.Count == 0) return false;
                for (int i = 0; i < polygons.Count; i++)
                {
                    if (polygons[i] != null && polygons[i].IsValid) return true;
                }
                return false;
            }
        }

        public MultiPolygon2D() { }

        public MultiPolygon2D(IEnumerable<Polygon2D> source)
        {
            if (source == null) return;

            foreach (Polygon2D poly in source)
            {
                if (poly != null && poly.IsValid)
                {
                    polygons.Add(poly.Clone());
                }
            }
        }

        public void Add(Polygon2D polygon)
        {
            if (polygon != null && polygon.IsValid)
            {
                polygons.Add(polygon.Clone());
            }
        }

        public Bounds GetBounds()
        {
            bool hasAny = false;
            Bounds bounds = default;

            for (int i = 0; i < polygons.Count; i++)
            {
                Polygon2D poly = polygons[i];
                if (poly == null || !poly.IsValid) continue;

                Bounds b = poly.GetBounds();
                if (!hasAny)
                {
                    bounds = b;
                    hasAny = true;
                }
                else
                {
                    bounds.Encapsulate(b);
                }
            }

            return hasAny ? bounds : new Bounds(Vector3.zero, Vector3.zero);
        }

        public IShape2D CloneShape() => new MultiPolygon2D(polygons);

        public Polygon2D ToPolygon()
        {
            Polygon2D best = null;
            float bestAbs = 0f;

            for (int i = 0; i < polygons.Count; i++)
            {
                Polygon2D poly = polygons[i];
                if (poly == null || !poly.IsValid) continue;

                float abs = Mathf.Abs(poly.CalculateSignedArea());
                if (best == null || abs > bestAbs)
                {
                    best = poly;
                    bestAbs = abs;
                }
            }

            return best?.Clone();
        }

        public static IShape2D FromPolygons(IReadOnlyList<Polygon2D> source)
        {
            if (source == null || source.Count == 0) return null;

            var valid = new List<Polygon2D>(source.Count);
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] != null && source[i].IsValid)
                {
                    valid.Add(source[i].Clone());
                }
            }

            if (valid.Count == 0) return null;
            if (valid.Count == 1) return valid[0];
            return new MultiPolygon2D(valid);
        }
    }
}

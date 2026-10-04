using System.Collections.Generic;
using UnityEngine;

namespace Galaretka.ShapeGraph.Core
{
    public sealed class ShapePicture2D
    {
        private readonly List<ShapeRegion2D> layers = new();

        public IReadOnlyList<ShapeRegion2D> Layers => layers;

        public ShapePicture2D() { }

        public ShapePicture2D(IEnumerable<ShapeRegion2D> source)
        {
            if (source == null) return;

            foreach (ShapeRegion2D region in source)
            {
                if (region != null)
                {
                    layers.Add((ShapeRegion2D)region.CloneShape());
                }
            }
        }

        public void Add(ShapeRegion2D region)
        {
            if (region != null)
            {
                layers.Add((ShapeRegion2D)region.CloneShape());
            }
        }

        public ShapePicture2D Clone() => new(layers);

        public Bounds GetBounds()
        {
            bool hasAny = false;
            Bounds bounds = default;
            for (int i = 0; i < layers.Count; i++)
            {
                ShapeRegion2D layer = layers[i];
                if (layer?.Geometry == null) continue;

                Bounds b = layer.GetBounds();
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

        public static ShapePicture2D FromRegion(ShapeRegion2D region)
        {
            var picture = new ShapePicture2D();
            if (region != null)
            {
                picture.Add(region);
            }
            return picture;
        }
    }
}

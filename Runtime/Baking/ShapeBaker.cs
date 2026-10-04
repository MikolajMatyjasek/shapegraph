using Galaretka.ShapeGraph.Components;
using UnityEngine;

namespace Galaretka.ShapeGraph.Baking
{
    public static class ShapeBaker
    {
        public static GameObject BakeToStaticGameObject(ProceduralShapeInstance instance, string nameSuffix = "_Baked")
        {
            if (instance == null) return null;

            instance.Rebuild();

            MeshFilter sourceFilter = instance.GetComponent<MeshFilter>();
            MeshRenderer sourceRenderer = instance.GetComponent<MeshRenderer>();
            PolygonCollider2D sourceCollider = instance.GetComponent<PolygonCollider2D>();

            if (sourceFilter == null || sourceFilter.sharedMesh == null || sourceFilter.sharedMesh.vertexCount < 3)
            {
                Debug.LogWarning("[ShapeGraph] Bake aborted: source mesh is empty.");
                return null;
            }

            GameObject bakedObj = new GameObject(instance.gameObject.name + nameSuffix);
            bakedObj.transform.SetPositionAndRotation(instance.transform.position, instance.transform.rotation);
            bakedObj.transform.localScale = instance.transform.localScale;

            Mesh meshCopy = Object.Instantiate(sourceFilter.sharedMesh);
            meshCopy.name = bakedObj.name + "_Mesh";

            var filter = bakedObj.AddComponent<MeshFilter>();
            filter.sharedMesh = meshCopy;

            var renderer = bakedObj.AddComponent<MeshRenderer>();
            if (sourceRenderer != null)
            {
                renderer.sharedMaterial = sourceRenderer.sharedMaterial;
                renderer.sortingLayerName = sourceRenderer.sortingLayerName;
                renderer.sortingOrder = sourceRenderer.sortingOrder;
            }

            if (sourceCollider != null && sourceCollider.pathCount > 0)
            {
                var col = bakedObj.AddComponent<PolygonCollider2D>();
                col.pathCount = sourceCollider.pathCount;
                for (int i = 0; i < sourceCollider.pathCount; i++)
                {
                    col.SetPath(i, sourceCollider.GetPath(i));
                }
            }

            return bakedObj;
        }
    }
}

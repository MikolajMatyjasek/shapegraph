using System;
using System.Collections.Generic;
using Galaretka.ShapeGraph.Baking;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Data;
using Galaretka.ShapeGraph.Evaluation;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;

namespace Galaretka.ShapeGraph.Components
{
    [Serializable]
    public struct ParameterOverrideEntry
    {
        public string ParameterName;
        public ParameterValue Value;
    }

    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(PolygonCollider2D))]
    public class ProceduralShapeInstance : MonoBehaviour
    {
        [SerializeField] private ShapeGraphAsset graphAsset;
        [SerializeField] private int seed = 1337;

        [Header("Materials & Rendering")]
        [SerializeField] private Material customMaterial;
        [SerializeField] private string sortingLayerName = "Default";
        [SerializeField] private int orderInLayer;

        [Header("Parameter Overrides")]
        [SerializeField] private List<ParameterOverrideEntry> overrides = new();

        private Mesh runtimeMesh;

        public ShapeGraphAsset Graph
        {
            get => graphAsset;
            set
            {
                graphAsset = value;
                Rebuild();
            }
        }

        public int Seed
        {
            get => seed;
            set
            {
                seed = value;
                Rebuild();
            }
        }

        public Material CustomMaterial
        {
            get => customMaterial;
            set
            {
                customMaterial = value;
                ApplyMaterial();
            }
        }

        public string SortingLayerName
        {
            get => sortingLayerName;
            set
            {
                sortingLayerName = value;
                ApplySorting();
            }
        }

        public int OrderInLayer
        {
            get => orderInLayer;
            set
            {
                orderInLayer = value;
                ApplySorting();
            }
        }

        public IReadOnlyList<ParameterOverrideEntry> Overrides => overrides;

        private void OnEnable() => Rebuild();

        private void OnValidate()
        {
            ApplySorting();
            ApplyMaterial();
        }

        public void SetOverride(string parameterName, ParameterValue value)
        {
            if (string.IsNullOrEmpty(parameterName)) return;

            int idx = overrides.FindIndex(o => o.ParameterName == parameterName);
            if (idx >= 0)
            {
                var entry = overrides[idx];
                entry.Value = value;
                overrides[idx] = entry;
            }
            else
            {
                overrides.Add(new ParameterOverrideEntry { ParameterName = parameterName, Value = value });
            }

            Rebuild();
        }

        public void SetOverride(string parameterName, float value) =>
            SetOverride(parameterName, ParameterValue.FromFloat(value));

        public void SetOverride(string parameterName, int value) =>
            SetOverride(parameterName, ParameterValue.FromInt(value));

        public void SetOverride(string parameterName, bool value) =>
            SetOverride(parameterName, ParameterValue.FromBool(value));

        public void SetOverride(string parameterName, Vector2 value) =>
            SetOverride(parameterName, ParameterValue.FromVector2(value));

        public void SetOverride(string parameterName, Color value) =>
            SetOverride(parameterName, ParameterValue.FromColor(value));

        public void ClearOverride(string parameterName)
        {
            overrides.RemoveAll(o => o.ParameterName == parameterName);
            Rebuild();
        }

        public void ClearAllOverrides()
        {
            overrides.Clear();
            Rebuild();
        }

        public void Rebuild()
        {
            if (graphAsset == null)
            {
                ClearRuntimeGeometry();
                return;
            }

            Dictionary<ParameterId, ParameterValue> overrideMap = BuildOverrideMap();
            var context = new ShapeContext(seed, overrideMap);
            GeneratedMeshData meshData = graphAsset.Evaluate(context);

            EnsureRuntimeMesh();

            runtimeMesh.Clear();
            if (meshData.Vertices == null || meshData.Vertices.Length < 3)
            {
                ClearCollider();
                ApplyMaterial();
                ApplySorting();
                return;
            }

            runtimeMesh.vertices = meshData.Vertices;
            runtimeMesh.triangles = meshData.Triangles;
            runtimeMesh.colors32 = meshData.Colors;
            runtimeMesh.RecalculateNormals();
            runtimeMesh.RecalculateBounds();

            var col = GetComponent<PolygonCollider2D>();
            Vector2[][] paths = meshData.ColliderPaths;
            if (paths != null && paths.Length > 0)
            {
                int pathCount = 0;
                for (int i = 0; i < paths.Length; i++)
                {
                    if (paths[i] != null && paths[i].Length >= 3)
                    {
                        pathCount++;
                    }
                }

                col.pathCount = pathCount;
                int write = 0;
                for (int i = 0; i < paths.Length; i++)
                {
                    if (paths[i] == null || paths[i].Length < 3) continue;
                    col.SetPath(write++, paths[i]);
                }
            }
            else if (meshData.ColliderBoundary != null && meshData.ColliderBoundary.Length >= 3)
            {
                col.pathCount = 1;
                col.SetPath(0, meshData.ColliderBoundary);
            }
            else
            {
                col.pathCount = 0;
            }

            ApplyMaterial();
            ApplySorting();
        }

        [ContextMenu("Bake To Static GameObject")]
        private void BakeToStaticContextMenu()
        {
            ShapeBaker.BakeToStaticGameObject(this);
        }

        private Dictionary<ParameterId, ParameterValue> BuildOverrideMap()
        {
            if (overrides.Count == 0 || graphAsset == null)
                return null;

            var map = new Dictionary<ParameterId, ParameterValue>();
            for (int i = 0; i < overrides.Count; i++)
            {
                ParameterOverrideEntry entry = overrides[i];
                if (string.IsNullOrEmpty(entry.ParameterName)) continue;

                GraphParameter param = graphAsset.FindParameterByName(entry.ParameterName);
                if (param == null) continue;

                if (param.Mode != ParameterMode.Exposed) continue;

                if (entry.Value.Type != param.ValueType)
                {
                    Debug.LogWarning(
                        $"[ShapeGraph] Override '{entry.ParameterName}' type mismatch " +
                        $"({entry.Value.Type} vs {param.ValueType}). Ignored.");
                    continue;
                }

                map[param.Id] = entry.Value;
            }

            return map.Count > 0 ? map : null;
        }

        private void EnsureRuntimeMesh()
        {
            if (runtimeMesh != null) return;

            runtimeMesh = new Mesh { name = "ProceduralShape_Runtime" };
            runtimeMesh.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            GetComponent<MeshFilter>().sharedMesh = runtimeMesh;
        }

        private void ClearRuntimeGeometry()
        {
            if (runtimeMesh != null)
            {
                runtimeMesh.Clear();
            }

            ClearCollider();
            ApplyMaterial();
            ApplySorting();
        }

        private void ClearCollider()
        {
            var col = GetComponent<PolygonCollider2D>();
            if (col != null)
            {
                col.pathCount = 0;
            }
        }

        private void ApplyMaterial()
        {
            var rend = GetComponent<MeshRenderer>();
            Material matToUse = customMaterial != null
                ? customMaterial
                : (graphAsset != null ? graphAsset.DefaultMaterial : null);

            if (matToUse != null && rend.sharedMaterial != matToUse)
            {
                rend.sharedMaterial = matToUse;
            }
        }

        private void ApplySorting()
        {
            var rend = GetComponent<MeshRenderer>();
            if (rend == null) return;

            rend.sortingLayerName = sortingLayerName;
            rend.sortingOrder = orderInLayer;
        }
    }
}

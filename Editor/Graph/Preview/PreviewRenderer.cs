using Galaretka.ShapeGraph.Core;
using UnityEditor;
using UnityEngine;

namespace Galaretka.ShapeGraph.Editor.Graph.Preview
{
    /// <summary>
    /// Orthographic blit of <see cref="GeneratedMeshData"/> into a RenderTexture.
    /// </summary>
    public sealed class PreviewRenderer : System.IDisposable
    {
        private GameObject root;
        private Camera camera;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh mesh;
        private Material material;
        private bool ownsMaterial;

        public void EnsureCreated()
        {
            if (root != null)
                return;

            root = new GameObject("ShapeGraph_PreviewRoot")
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            var camGo = new GameObject("Camera") { hideFlags = HideFlags.HideAndDontSave };
            camGo.transform.SetParent(root.transform, false);
            camera = camGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.nearClipPlane = -10f;
            camera.farClipPlane = 10f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.13f, 0.15f, 1f);
            camera.enabled = false;
            camera.cameraType = CameraType.Preview;
            camera.forceIntoRenderTexture = true;

            var meshGo = new GameObject("Mesh") { hideFlags = HideFlags.HideAndDontSave };
            meshGo.transform.SetParent(root.transform, false);
            meshFilter = meshGo.AddComponent<MeshFilter>();
            meshRenderer = meshGo.AddComponent<MeshRenderer>();

            mesh = new Mesh { name = "ShapeGraph_PreviewMesh" };
            mesh.hideFlags = HideFlags.HideAndDontSave;
            meshFilter.sharedMesh = mesh;

            material = FindVertexColorMaterial();
            meshRenderer.sharedMaterial = material;
        }

        public RenderTexture Render(GeneratedMeshData data, int size)
        {
            EnsureCreated();
            size = Mathf.Clamp(size, 32, 1024);

            if (data.Vertices == null || data.Vertices.Length < 3 ||
                data.Triangles == null || data.Triangles.Length < 3)
                return null;

            mesh.Clear();
            mesh.vertices = data.Vertices;
            mesh.triangles = data.Triangles;
            mesh.colors32 = data.Colors != null && data.Colors.Length == data.Vertices.Length
                ? data.Colors
                : CreateSolidColors(data.Vertices.Length, Color.white);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            Bounds b = mesh.bounds;
            float extent = Mathf.Max(b.extents.x, b.extents.y, 0.01f);
            camera.orthographicSize = extent * 1.25f;
            camera.transform.position = new Vector3(b.center.x, b.center.y, -5f);
            camera.transform.rotation = Quaternion.identity;

            var rt = new RenderTexture(size, size, 16, RenderTextureFormat.ARGB32)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                name = "ShapeGraph_PreviewRT"
            };
            rt.Create();

            RenderTexture previous = camera.targetTexture;
            camera.targetTexture = rt;
            camera.Render();
            camera.targetTexture = previous;
            return rt;
        }

        public void Dispose()
        {
            if (mesh != null)
            {
                Object.DestroyImmediate(mesh);
                mesh = null;
            }

            if (ownsMaterial && material != null)
            {
                Object.DestroyImmediate(material);
                material = null;
            }

            if (root != null)
            {
                Object.DestroyImmediate(root);
                root = null;
            }

            camera = null;
            meshFilter = null;
            meshRenderer = null;
        }

        private Material FindVertexColorMaterial()
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
                shader = Shader.Find("Unlit/Color");
            if (shader == null)
                shader = Shader.Find("Hidden/Internal-GUITexture");

            if (shader != null)
            {
                ownsMaterial = true;
                return new Material(shader) { hideFlags = HideFlags.HideAndDontSave, name = "ShapeGraph_PreviewMat" };
            }

            ownsMaterial = false;
            return AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
        }

        private static Color32[] CreateSolidColors(int count, Color32 color)
        {
            var colors = new Color32[count];
            for (int i = 0; i < count; i++)
                colors[i] = color;
            return colors;
        }
    }
}

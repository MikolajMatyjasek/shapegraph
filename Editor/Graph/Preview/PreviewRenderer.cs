using Galaretka.ShapeGraph.Core;
using UnityEditor;
using UnityEngine;

namespace Galaretka.ShapeGraph.Editor.Graph.Preview
{
    public sealed class PreviewRenderer : System.IDisposable
    {
        private PreviewRenderUtility previewUtility;
        private Mesh mesh;
        private Material material;
        private bool ownsMaterial;

        public void EnsureCreated()
        {
            if (previewUtility != null) return;

            previewUtility = new PreviewRenderUtility();
            previewUtility.camera.orthographic = true;
            previewUtility.camera.nearClipPlane = -10f;
            previewUtility.camera.farClipPlane = 10f;
            previewUtility.camera.clearFlags = CameraClearFlags.SolidColor;
            previewUtility.camera.backgroundColor = new Color(0.12f, 0.13f, 0.15f, 1f);
            previewUtility.camera.transform.position = new Vector3(0f, 0f, -5f);
            previewUtility.camera.transform.rotation = Quaternion.identity;

            if (previewUtility.lights != null && previewUtility.lights.Length > 0)
            {
                previewUtility.lights[0].intensity = 1.2f;
                previewUtility.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0f);
            }

            mesh = new Mesh { name = "ShapeGraph_PreviewMesh" };
            mesh.hideFlags = HideFlags.HideAndDontSave;

            material = CreateVertexColorMaterial();
        }

        public RenderTexture Render(GeneratedMeshData data, int size)
        {
            EnsureCreated();
            size = Mathf.Clamp(size, 32, 1024);

            if (data.Vertices == null || data.Vertices.Length < 3 ||
                data.Triangles == null || data.Triangles.Length < 3) return null;

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
            previewUtility.camera.orthographicSize = extent * 1.25f;
            previewUtility.camera.transform.position = new Vector3(b.center.x, b.center.y, -5f);

            var rt = new RenderTexture(size, size, 16, RenderTextureFormat.ARGB32)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                name = "ShapeGraph_PreviewRT"
            };
            rt.Create();

            previewUtility.BeginStaticPreview(new Rect(0f, 0f, size, size));
            previewUtility.DrawMesh(mesh, Matrix4x4.identity, material, 0);
            previewUtility.camera.Render();
            Texture2D previewTex = previewUtility.EndStaticPreview();

            if (previewTex != null)
            {
                Graphics.Blit(previewTex, rt);
                Object.DestroyImmediate(previewTex);
            }

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

            if (previewUtility != null)
            {
                previewUtility.Cleanup();
                previewUtility = null;
            }
        }

        private Material CreateVertexColorMaterial()
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }
            if (shader == null)
            {
                shader = Shader.Find("Hidden/Internal-GUITexture");
            }

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
            {
                colors[i] = color;
            }
            return colors;
        }
    }
}

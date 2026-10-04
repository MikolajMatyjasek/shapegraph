using System.IO;
using Galaretka.ShapeGraph.Baking;
using Galaretka.ShapeGraph.Components;
using UnityEditor;
using UnityEngine;

namespace Galaretka.ShapeGraph.Editor.Baking
{
    public static class ShapePrefabBaker
    {
        public static GameObject BakeToPrefab(ProceduralShapeInstance instance, string folderPath)
        {
            if (instance == null) return null;

            GameObject bakedGo = ShapeBaker.BakeToStaticGameObject(instance, string.Empty);
            if (bakedGo == null) return null;

            try
            {
                if (string.IsNullOrEmpty(folderPath))
                {
                    Debug.LogWarning("[ShapeGraph] Bake to prefab aborted: folder path is empty.");
                    return null;
                }

                folderPath = folderPath.Replace('\\', '/');
                if (!folderPath.StartsWith("Assets"))
                {
                    string dataPath = Application.dataPath.Replace('\\', '/');
                    if (folderPath.StartsWith(dataPath)) 
                    {
                        folderPath = "Assets" + folderPath.Substring(dataPath.Length);
                    }
                }

                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    EnsureFolderExists(folderPath);
                }

                string baseName = bakedGo.name;
                string meshPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{baseName}_Mesh.asset");
                string prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{baseName}.prefab");

                MeshFilter mf = bakedGo.GetComponent<MeshFilter>();
                Mesh mesh = mf.sharedMesh;
                AssetDatabase.CreateAsset(mesh, meshPath);
                AssetDatabase.SaveAssets();

                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(bakedGo, prefabPath);
                Debug.Log($"[ShapeGraph] Baked prefab: {prefabPath}");
                return prefab;
            }
            finally
            {
                Object.DestroyImmediate(bakedGo);
            }
        }

        private static void EnsureFolderExists(string folderPath)
        {
            folderPath = folderPath.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(folderPath)) return;

            string[] parts = folderPath.Split('/');
            if (parts.Length == 0 || parts[0] != "Assets")
            {
                Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh();
                return;
            }

            string current = "Assets";
            for (int i = 1; i < parts.Length; i++)
            {
                if (string.IsNullOrEmpty(parts[i])) continue;

                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}

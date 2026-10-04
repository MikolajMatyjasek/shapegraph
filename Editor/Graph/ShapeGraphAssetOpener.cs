using Galaretka.ShapeGraph.Data;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Galaretka.ShapeGraph.Editor.Graph
{
    [CustomEditor(typeof(ShapeGraphAsset))]
    public sealed class ShapeGraphAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Shape Graph Editor", GUILayout.Height(28)))
                ShapeGraphEditorWindow.OpenAsset((ShapeGraphAsset)target);

            GUILayout.Space(8);
            DrawDefaultInspector();
        }
    }

    public static class ShapeGraphAssetOpener
    {
        [OnOpenAsset(1)]
        public static bool OnOpenAsset(EntityId entityId, int line)
        {
            Object obj = EditorUtility.EntityIdToObject(entityId);
            if (obj is ShapeGraphAsset asset)
            {
                ShapeGraphEditorWindow.OpenAsset(asset);
                return true;
            }

            return false;
        }
    }
}

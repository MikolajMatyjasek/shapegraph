using Galaretka.ShapeGraph.Data;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Galaretka.ShapeGraph.Editor.Graph
{
    [CustomEditor(typeof(ShapeGraphAsset))]
    public sealed class ShapeGraphAssetEditor : UnityEditor.Editor
    {
        private SerializedProperty defaultMaterialProp;
        private SerializedProperty parametersProp;

        private void OnEnable()
        {
            defaultMaterialProp = serializedObject.FindProperty("defaultMaterial");
            parametersProp = serializedObject.FindProperty("parameters");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUILayout.Button("Open Shape Graph Editor", GUILayout.Height(28)))
            {
                ShapeGraphEditorWindow.OpenAsset((ShapeGraphAsset)target);
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.PropertyField(defaultMaterialProp, new GUIContent("Default Material"));

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Constant / Exposed / RandomRange. Wire via Values/Parameter in the graph editor. " +
                "Topology is edited only in the graph editor.",
                MessageType.None);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(parametersProp, includeChildren: true);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                var asset = (ShapeGraphAsset)target;
                for (int i = 0; i < asset.Parameters.Count; i++)
                {
                    asset.Parameters[i]?.ValidateId();
                }
                EditorUtility.SetDirty(asset);
            }
            else
            {
                serializedObject.ApplyModifiedProperties();
            }
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

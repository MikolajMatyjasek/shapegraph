using System.Collections.Generic;
using Galaretka.ShapeGraph.Components;
using Galaretka.ShapeGraph.Data;
using Galaretka.ShapeGraph.Editor.Baking;
using Galaretka.ShapeGraph.Typing;
using UnityEditor;
using UnityEngine;

namespace Galaretka.ShapeGraph.Editor.Inspectors
{
    [CustomEditor(typeof(ProceduralShapeInstance))]
    public sealed class ProceduralShapeInstanceEditor : UnityEditor.Editor
    {
        private SerializedProperty graphAssetProp;
        private SerializedProperty seedProp;
        private SerializedProperty customMaterialProp;
        private SerializedProperty sortingLayerNameProp;
        private SerializedProperty orderInLayerProp;

        private void OnEnable()
        {
            graphAssetProp = serializedObject.FindProperty("graphAsset");
            seedProp = serializedObject.FindProperty("seed");
            customMaterialProp = serializedObject.FindProperty("customMaterial");
            sortingLayerNameProp = serializedObject.FindProperty("sortingLayerName");
            orderInLayerProp = serializedObject.FindProperty("orderInLayer");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var instance = (ProceduralShapeInstance)target;

            EditorGUILayout.PropertyField(graphAssetProp);
            EditorGUILayout.PropertyField(customMaterialProp);
            EditorGUILayout.PropertyField(sortingLayerNameProp);
            EditorGUILayout.PropertyField(orderInLayerProp);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Seed", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            int seedField = EditorGUILayout.IntField("Seed", seedProp.intValue);
            if (EditorGUI.EndChangeCheck())
            {
                seedProp.intValue = seedField;
            }

            EditorGUI.BeginChangeCheck();
            int seedSlider = EditorGUILayout.IntSlider(
                "Seed Slider",
                Mathf.Clamp(seedProp.intValue, 0, 100000),
                0,
                100000);
            if (EditorGUI.EndChangeCheck())
            {
                seedProp.intValue = seedSlider;
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                instance.Rebuild();
            }

            EditorGUILayout.Space();
            DrawExposedParameters(instance);

            EditorGUILayout.Space();
            if (GUILayout.Button("Bake To Prefab…"))
            {
                string absolute = EditorUtility.OpenFolderPanel(
                    "Select bake folder (under Assets)",
                    Application.dataPath,
                    string.Empty);
                if (!string.IsNullOrEmpty(absolute))
                {
                    ShapePrefabBaker.BakeToPrefab(instance, absolute);
                }
            }
        }

        private void DrawExposedParameters(ProceduralShapeInstance instance)
        {
            EditorGUILayout.LabelField("Exposed Parameters", EditorStyles.boldLabel);

            ShapeGraphAsset graph = instance.Graph;
            if (graph == null)
            {
                EditorGUILayout.HelpBox("Assign a Shape Graph asset to edit Exposed parameters.", MessageType.Info);
                return;
            }

            bool any = false;
            IReadOnlyList<GraphParameter> parameters = graph.Parameters;
            for (int i = 0; i < parameters.Count; i++)
            {
                GraphParameter param = parameters[i];
                if (param == null || param.Mode != ParameterMode.Exposed) continue;

                any = true;
                ParameterValue current = GetOverrideOrDefault(instance, param);
                EditorGUI.BeginChangeCheck();
                ParameterValue edited = DrawTypedValue(param.Name, param.ValueType, current);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(instance, "Edit ShapeGraph Override");
                    instance.SetOverride(param.Name, edited);
                    EditorUtility.SetDirty(instance);
                }
            }

            if (!any)
            {
                EditorGUILayout.HelpBox("No Exposed parameters on this graph.", MessageType.None);
            }
        }

        private static ParameterValue GetOverrideOrDefault(ProceduralShapeInstance instance, GraphParameter param)
        {
            IReadOnlyList<ParameterOverrideEntry> list = instance.Overrides;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].ParameterName == param.Name && list[i].Value.Type == param.ValueType) return list[i].Value;
            }

            return param.ConstantOrDefault;
        }

        private static ParameterValue DrawTypedValue(string label, ParameterValueType type, ParameterValue value)
        {
            switch (type)
            {
                case ParameterValueType.Float:
                    return ParameterValue.FromFloat(EditorGUILayout.FloatField(label, value.AsFloat()));
                case ParameterValueType.Int:
                    return ParameterValue.FromInt(EditorGUILayout.IntField(label, value.AsInt()));
                case ParameterValueType.Bool:
                    return ParameterValue.FromBool(EditorGUILayout.Toggle(label, value.AsBool()));
                case ParameterValueType.Vector2:
                    return ParameterValue.FromVector2(EditorGUILayout.Vector2Field(label, value.AsVector2()));
                case ParameterValueType.Color:
                    return ParameterValue.FromColor(EditorGUILayout.ColorField(label, value.AsColor()));
                default:
                    EditorGUILayout.LabelField(label, "(unsupported type)");
                    return value;
            }
        }
    }
}

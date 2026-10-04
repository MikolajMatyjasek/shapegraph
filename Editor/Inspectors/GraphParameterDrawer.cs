using Galaretka.ShapeGraph.Data;
using Galaretka.ShapeGraph.Typing;
using UnityEditor;
using UnityEngine;

namespace Galaretka.ShapeGraph.Editor.Inspectors
{
    [CustomPropertyDrawer(typeof(GraphParameter))]
    public sealed class GraphParameterDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty modeProp = property.FindPropertyRelative("mode");
            var mode = (ParameterMode)modeProp.enumValueIndex;
            int rows = 4; // name, type, mode, value
            if (mode == ParameterMode.RandomRange)
            {
                rows += 2;
            }
            return rows * (EditorGUIUtility.singleLineHeight + 2f) + 4f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty nameProp = property.FindPropertyRelative("name");
            SerializedProperty typeProp = property.FindPropertyRelative("valueType");
            SerializedProperty modeProp = property.FindPropertyRelative("mode");
            SerializedProperty valueProp = property.FindPropertyRelative("constantOrDefault");
            SerializedProperty minProp = property.FindPropertyRelative("rangeMin");
            SerializedProperty maxProp = property.FindPropertyRelative("rangeMax");

            float y = position.y;
            float h = EditorGUIUtility.singleLineHeight;
            float gap = 2f;
            float width = position.width;

            EditorGUI.BeginProperty(position, label, property);

            nameProp.stringValue = EditorGUI.TextField(new Rect(position.x, y, width, h), "Name", nameProp.stringValue);
            y += h + gap;

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(new Rect(position.x, y, width, h), typeProp, new GUIContent("Type"));
            bool typeChanged = EditorGUI.EndChangeCheck();
            y += h + gap;

            if (typeChanged)
            {
                SyncPackedType(valueProp, typeProp);
                SyncPackedType(minProp, typeProp);
                SyncPackedType(maxProp, typeProp);
            }
            else
            {
                SyncPackedType(valueProp, typeProp);
                SyncPackedType(minProp, typeProp);
                SyncPackedType(maxProp, typeProp);
            }

            EditorGUI.PropertyField(new Rect(position.x, y, width, h), modeProp, new GUIContent("Mode"));
            y += h + gap;

            var mode = (ParameterMode)modeProp.enumValueIndex;
            string valueLabel = mode == ParameterMode.Exposed ? "Default" : "Value";
            float valueHeight = EditorGUI.GetPropertyHeight(valueProp, true);
            EditorGUI.PropertyField(new Rect(position.x, y, width, valueHeight), valueProp, new GUIContent(valueLabel), true);
            y += valueHeight + gap;

            if (mode == ParameterMode.RandomRange)
            {
                float minH = EditorGUI.GetPropertyHeight(minProp, true);
                EditorGUI.PropertyField(new Rect(position.x, y, width, minH), minProp, new GUIContent("Min"), true);
                y += minH + gap;
                float maxH = EditorGUI.GetPropertyHeight(maxProp, true);
                EditorGUI.PropertyField(new Rect(position.x, y, width, maxH), maxProp, new GUIContent("Max"), true);
            }

            EditorGUI.EndProperty();
        }

        private static void SyncPackedType(SerializedProperty valueProp, SerializedProperty typeProp)
        {
            SerializedProperty packedType = valueProp.FindPropertyRelative("type");
            if (packedType != null)
            {
                packedType.enumValueIndex = typeProp.enumValueIndex;
            }
        }
    }
}

using Galaretka.ShapeGraph.Typing;
using UnityEditor;
using UnityEngine;

namespace Galaretka.ShapeGraph.Editor.Inspectors
{
    [CustomPropertyDrawer(typeof(ParameterValue))]
    public sealed class ParameterValueDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty typeProp = property.FindPropertyRelative("type");
            var type = (ParameterValueType)typeProp.enumValueIndex;
            float lines = type == ParameterValueType.Vector2 ? 2.15f : 1f;
            return EditorGUIUtility.singleLineHeight * lines + 2f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty typeProp = property.FindPropertyRelative("type");
            SerializedProperty f0 = property.FindPropertyRelative("f0");
            SerializedProperty f1 = property.FindPropertyRelative("f1");
            SerializedProperty f2 = property.FindPropertyRelative("f2");
            SerializedProperty f3 = property.FindPropertyRelative("f3");
            SerializedProperty i0 = property.FindPropertyRelative("i0");

            var type = (ParameterValueType)typeProp.enumValueIndex;
            EditorGUI.BeginProperty(position, label, property);

            switch (type)
            {
                case ParameterValueType.Float:
                    f0.floatValue = EditorGUI.FloatField(position, label, f0.floatValue);
                    break;
                case ParameterValueType.Int:
                    i0.intValue = EditorGUI.IntField(position, label, i0.intValue);
                    break;
                case ParameterValueType.Bool:
                    i0.intValue = EditorGUI.Toggle(position, label, i0.intValue != 0) ? 1 : 0;
                    break;
                case ParameterValueType.Vector2:
                    Vector2 v = EditorGUI.Vector2Field(position, label, new Vector2(f0.floatValue, f1.floatValue));
                    f0.floatValue = v.x;
                    f1.floatValue = v.y;
                    break;
                case ParameterValueType.Color:
                    Color c = EditorGUI.ColorField(position, label, new Color(f0.floatValue, f1.floatValue, f2.floatValue, f3.floatValue));
                    f0.floatValue = c.r;
                    f1.floatValue = c.g;
                    f2.floatValue = c.b;
                    f3.floatValue = c.a;
                    break;
                default:
                    EditorGUI.LabelField(position, label.text, "(unsupported)");
                    break;
            }

            EditorGUI.EndProperty();
        }
    }
}

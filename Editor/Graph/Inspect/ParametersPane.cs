using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Data;
using Galaretka.ShapeGraph.Editor.Graph.Commands;
using Galaretka.ShapeGraph.Typing;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Galaretka.ShapeGraph.Editor.Graph.Inspect
{
    public sealed class ParametersPane : VisualElement
    {
        private readonly GraphCommandService commands;
        private readonly IMGUIContainer imgui;
        private int selectedIndex = -1;

        public ParametersPane(GraphCommandService commandService)
        {
            commands = commandService;
            AddToClassList("parameters-pane");

            var title = new Label("Parameters") { name = "parameters-title" };
            title.AddToClassList("inspector-title");
            Add(title);

            var buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.Add(new Button(OnAdd) { text = "Add" });
            buttons.Add(new Button(OnRemove) { text = "Remove" });
            Add(buttons);

            imgui = new IMGUIContainer(DrawIMGUI);
            imgui.style.flexGrow = 1;
            imgui.style.minHeight = 120;
            Add(imgui);
        }

        public void Refresh() => imgui?.MarkDirtyRepaint();

        private void OnAdd()
        {
            if (commands.Asset == null) return;
            commands.AddParameter();
            selectedIndex = commands.Asset.Parameters.Count - 1;
            Refresh();
        }

        private void OnRemove()
        {
            ShapeGraphAsset asset = commands.Asset;
            if (asset == null || selectedIndex < 0 || selectedIndex >= asset.Parameters.Count) return;

            GraphParameter param = asset.Parameters[selectedIndex];
            if (param == null) return;

            commands.RemoveParameter(param.Id);
            selectedIndex = Mathf.Clamp(selectedIndex, 0, asset.Parameters.Count - 1);
            if (asset.Parameters.Count == 0)
            {
                selectedIndex = -1;
            }
            Refresh();
        }

        private void DrawIMGUI()
        {
            ShapeGraphAsset asset = commands.Asset;
            if (asset == null)
            {
                EditorGUILayout.HelpBox("Bind a Shape Graph asset to edit parameters.", MessageType.Info); return;
            }

            IReadOnlyList<GraphParameter> list = asset.Parameters;
            if (list.Count == 0)
            {
                EditorGUILayout.HelpBox("No parameters. Add one to drive Values/Parameter nodes.", MessageType.None);
                return;
            }

            string[] names = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                names[i] = list[i] != null ? list[i].Name : $"[{i}]";
            }

            selectedIndex = Mathf.Clamp(selectedIndex, 0, list.Count - 1);
            selectedIndex = EditorGUILayout.Popup("Selected", selectedIndex, names);

            GraphParameter param = list[selectedIndex];
            if (param == null) return;

            EditorGUI.BeginChangeCheck();
            string newName = EditorGUILayout.TextField("Name", param.Name);
            var newType = (ParameterValueType)EditorGUILayout.EnumPopup("Type", param.ValueType);
            var newMode = (ParameterMode)EditorGUILayout.EnumPopup("Mode", param.Mode);
            ParameterValue newValue = DrawValue(
                newMode == ParameterMode.Exposed ? "Default" : "Value",
                newType,
                param.ConstantOrDefault);
            ParameterValue newMin = param.RangeMin;
            ParameterValue newMax = param.RangeMax;
            if (newMode == ParameterMode.RandomRange)
            {
                newMin = DrawValue("Min", newType, param.RangeMin);
                newMax = DrawValue("Max", newType, param.RangeMax);
            }

            if (!EditorGUI.EndChangeCheck()) return;

            ParameterId id = param.Id;
            commands.UpdateParameter(id, p =>
            {
                if (p.ValueType != newType)
                {
                    p.SetValueType(newType);
                }
                p.SetName(newName);
                p.SetMode(newMode);
                p.SetConstant(newValue);
                if (newMode == ParameterMode.RandomRange)
                {
                    p.SetRange(newMin, newMax);
                }
            });
        }

        private static ParameterValue DrawValue(string label, ParameterValueType type, ParameterValue value)
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
                    EditorGUILayout.LabelField(label, "(unsupported)");
                    return value;
            }
        }
    }
}

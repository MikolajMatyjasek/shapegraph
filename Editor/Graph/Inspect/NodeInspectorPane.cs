using System.Collections.Generic;
using Galaretka.ShapeGraph.Data;
using Galaretka.ShapeGraph.Data.Nodes.Values;
using Galaretka.ShapeGraph.Editor.Graph.Commands;
using Galaretka.ShapeGraph.Typing;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Galaretka.ShapeGraph.Editor.Graph.Inspect
{
    public sealed class NodeInspectorPane : VisualElement
    {
        private readonly GraphCommandService commands;
        private readonly Label titleLabel;
        private readonly IMGUIContainer imgui;
        private ShapeNode selected;
        private SerializedObject serialized;

        public NodeInspectorPane(GraphCommandService commandService)
        {
            commands = commandService;
            AddToClassList("inspector-pane");

            titleLabel = new Label("No selection") { name = "inspector-title" };
            titleLabel.AddToClassList("inspector-title");
            Add(titleLabel);

            imgui = new IMGUIContainer(DrawIMGUI);
            imgui.style.flexGrow = 1;
            Add(imgui);
        }

        public void SetSelection(ShapeNode node)
        {
            selected = node;
            serialized = node != null ? new SerializedObject(node) : null;
            titleLabel.text = node != null ? ObjectNames.NicifyVariableName(node.name) : "No selection";
            imgui.MarkDirtyRepaint();
        }

        private void DrawIMGUI()
        {
            if (selected == null || serialized == null)
            {
                EditorGUILayout.HelpBox("Select a node to edit embedded parameters.", MessageType.Info);
                return;
            }

            if (selected is ParameterNode parameterNode)
            {
                DrawParameterNode(parameterNode);
                return;
            }

            EditorGUI.BeginChangeCheck();
            serialized.UpdateIfRequiredOrScript();

            SerializedProperty iterator = serialized.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.name == "m_Script") continue;
                if (iterator.name == "id" || iterator.name == "graphPosition") continue;
                EditorGUILayout.PropertyField(iterator, true);
            }

            if (EditorGUI.EndChangeCheck())
            {
                serialized.ApplyModifiedProperties();
                commands.NotifyNodePropertiesChanged(selected);
            }
        }

        private void DrawParameterNode(ParameterNode node)
        {
            ShapeGraphAsset asset = commands.Asset;
            if (asset == null)
            {
                EditorGUILayout.HelpBox("No graph asset bound.", MessageType.Warning);
                return;
            }

            IReadOnlyList<GraphParameter> parameters = asset.Parameters;
            var names = new List<string> { "(none)" };
            var types = new List<ParameterValueType> { ParameterValueType.Float };
            int selectedIndex = 0;

            for (int i = 0; i < parameters.Count; i++)
            {
                GraphParameter p = parameters[i];
                if (p == null) continue;
                names.Add(p.Name);
                types.Add(p.ValueType);
                if (p.Name == node.ParameterName)
                {
                    selectedIndex = names.Count - 1;
                }
            }

            EditorGUI.BeginChangeCheck();
            int next = EditorGUILayout.Popup("Parameter", selectedIndex, names.ToArray());
            if (!EditorGUI.EndChangeCheck())
            {
                EditorGUILayout.LabelField("Output Type", node.OutputType.ToString());
                return;
            }

            ParameterValueType before = node.OutputType;
            if (next <= 0)
            {
                node.BindParameter(string.Empty, ParameterValueType.Float);
                node.name = nameof(ParameterNode);
            }
            else
            {
                node.BindParameter(names[next], types[next]);
                node.name = names[next];
            }

            EditorUtility.SetDirty(node);
            titleLabel.text = ObjectNames.NicifyVariableName(node.name);
            if (node.OutputType != before)
            {
                commands.NotifyNodePortsChanged(node);
            }
            else
            {
                commands.NotifyNodePropertiesChanged(node);
            }
        }
    }
}

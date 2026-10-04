using Galaretka.ShapeGraph.Data;
using Galaretka.ShapeGraph.Editor.Graph.Commands;
using UnityEditor;
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
            imgui.MarkDirtyLayout();
        }

        private void DrawIMGUI()
        {
            if (selected == null || serialized == null)
            {
                EditorGUILayout.HelpBox("Select a node to edit embedded parameters.", MessageType.Info);
                return;
            }

            EditorGUI.BeginChangeCheck();
            serialized.UpdateIfRequiredOrScript();

            SerializedProperty iterator = serialized.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.name == "m_Script")
                    continue;
                // Hide internal identity/position — edited via graph.
                if (iterator.name == "id" || iterator.name == "graphPosition")
                    continue;
                EditorGUILayout.PropertyField(iterator, true);
            }

            if (EditorGUI.EndChangeCheck())
            {
                serialized.ApplyModifiedProperties();
                commands.NotifyNodePropertiesChanged(selected);
            }
        }
    }
}

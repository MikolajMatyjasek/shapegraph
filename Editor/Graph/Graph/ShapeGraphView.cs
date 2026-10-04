using System;
using System.Collections.Generic;
using System.Linq;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Data;
using Galaretka.ShapeGraph.Editor.Graph.Commands;
using Galaretka.ShapeGraph.Editor.Graph.Preview;
using Galaretka.ShapeGraph.Editor.Graph.Views;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Galaretka.ShapeGraph.Editor.Graph
{
    public sealed class ShapeGraphView : GraphView, IEdgeConnectorListener
    {
        private readonly GraphCommandService commands;
        private readonly NodePreviewService previews;
        private readonly Dictionary<NodeId, ShapeGraphNodeView> nodeViews = new();
        private ShapeNodeSearchWindow searchWindow;
        private bool suppressGraphChanges;

        public event Action<ShapeNode> SelectionChanged;

        public ShapeGraphView(GraphCommandService commandService, NodePreviewService previewService)
        {
            commands = commandService;
            previews = previewService;

            Insert(0, new GridBackground());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            style.flexGrow = 1;
            graphViewChanged = OnGraphViewChanged;
            RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        public void LoadFromAsset()
        {
            suppressGraphChanges = true;
            try
            {
                foreach (var kv in nodeViews)
                    kv.Value.UnbindPreview();

                // RemoveElement (not DeleteElements): DeleteElements routes through
                // graphViewChanged and would destroy runtime nodes via DeleteNode.
                var elements = graphElements.ToList();
                for (int i = 0; i < elements.Count; i++)
                    RemoveElement(elements[i]);
                nodeViews.Clear();

                ShapeGraphAsset asset = commands.Asset;
                if (asset == null)
                    return;

                for (int i = 0; i < asset.Nodes.Count; i++)
                {
                    ShapeNode node = asset.Nodes[i];
                    if (node == null)
                        continue;
                    AddNodeView(node);
                }

                for (int i = 0; i < asset.Connections.Count; i++)
                {
                    NodeConnection c = asset.Connections[i];
                    if (!nodeViews.TryGetValue(c.FromNodeId, out ShapeGraphNodeView from) ||
                        !nodeViews.TryGetValue(c.ToNodeId, out ShapeGraphNodeView to))
                        continue;

                    ShapeGraphPortView outPort = from.GetPort(c.FromPortId);
                    ShapeGraphPortView inPort = to.GetPort(c.ToPortId);
                    if (outPort == null || inPort == null)
                        continue;

                    AddElement(outPort.ConnectTo(inPort));
                }
            }
            finally
            {
                suppressGraphChanges = false;
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var result = new List<Port>();
            var start = startPort as ShapeGraphPortView;
            if (start == null)
                return result;

            ports.ForEach(p =>
            {
                if (p == startPort || p.node == startPort.node)
                    return;
                if (p is ShapeGraphPortView other && start.CanConnectTo(other))
                    result.Add(p);
            });
            return result;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            Vector2 local = contentViewContainer.WorldToLocal(evt.mousePosition);
            evt.menu.AppendAction("Create Node", _ => OpenSearchAtLocal(local));
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position) { }

        public void OnDrop(GraphView graphView, Edge edge)
        {
            if (!TryCommitEdge(edge, reloadOnFailure: false))
                return;

            AddElement(edge);
            edge.input?.Connect(edge);
            edge.output?.Connect(edge);
        }

        public ShapeNode CreateNodeAt(Type type, Vector2 graphPosition)
        {
            ShapeNode node = commands.CreateNode(type, graphPosition);
            return node;
        }

        public void FrameAllNodes() => FrameAll();

        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);
            NotifySelection();
        }

        public override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);
            NotifySelection();
        }

        public override void ClearSelection()
        {
            base.ClearSelection();
            SelectionChanged?.Invoke(null);
        }

        private void AddNodeView(ShapeNode node)
        {
            var view = new ShapeGraphNodeView(node, this);
            view.BindPreview(previews);
            nodeViews[node.Id] = view;
            AddElement(view);
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (suppressGraphChanges)
                return change;

            if (change.elementsToRemove != null)
            {
                foreach (GraphElement element in change.elementsToRemove)
                {
                    if (element is Edge edge)
                    {
                        var output = edge.output as ShapeGraphPortView;
                        var input = edge.input as ShapeGraphPortView;
                        if (output == null || input == null)
                            continue;
                        var from = output.node as ShapeGraphNodeView;
                        var to = input.node as ShapeGraphNodeView;
                        if (from == null || to == null)
                            continue;

                        commands.Disconnect(new NodeConnection(
                            from.RuntimeNodeId, output.ShapePortId,
                            to.RuntimeNodeId, input.ShapePortId));
                    }
                    else if (element is ShapeGraphNodeView nodeView)
                    {
                        nodeView.UnbindPreview();
                        commands.DeleteNode(nodeView.RuntimeNode);
                    }
                }
            }

            if (change.movedElements != null)
            {
                foreach (GraphElement element in change.movedElements)
                {
                    if (element is ShapeGraphNodeView nodeView)
                        commands.SetNodePosition(nodeView.RuntimeNode, nodeView.GetPosition().position);
                }
            }

            return change;
        }

        private bool TryCommitEdge(Edge edge, bool reloadOnFailure)
        {
            var output = edge.output as ShapeGraphPortView;
            var input = edge.input as ShapeGraphPortView;
            if (output == null || input == null)
                return false;

            var fromNode = output.node as ShapeGraphNodeView;
            var toNode = input.node as ShapeGraphNodeView;
            if (fromNode == null || toNode == null)
                return false;

            if (commands.Connect(fromNode.RuntimeNodeId, output.ShapePortId, toNode.RuntimeNodeId, input.ShapePortId))
                return true;

            if (reloadOnFailure)
                LoadFromAsset();
            return false;
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Space)
            {
                Vector2 local = contentViewContainer.WorldToLocal(evt.originalMousePosition);
                OpenSearchAtLocal(local);
                evt.StopPropagation();
            }
        }

        private void OpenSearchAtLocal(Vector2 localGraphPosition)
        {
            if (searchWindow == null)
                searchWindow = ScriptableObject.CreateInstance<ShapeNodeSearchWindow>();

            searchWindow.Initialize(this, localGraphPosition);
            Vector2 screen = GUIUtility.GUIToScreenPoint(Event.current != null ? Event.current.mousePosition : localGraphPosition);
            SearchWindow.Open(new SearchWindowContext(screen), searchWindow);
        }

        private void NotifySelection()
        {
            foreach (ISelectable selected in selection)
            {
                if (selected is ShapeGraphNodeView nv)
                {
                    SelectionChanged?.Invoke(nv.RuntimeNode);
                    return;
                }
            }

            SelectionChanged?.Invoke(null);
        }
    }
}

using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Data;
using Galaretka.ShapeGraph.Editor.Graph.Preview;
using Galaretka.ShapeGraph.Editor.Graph.Theme;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Galaretka.ShapeGraph.Editor.Graph.Views
{
    public sealed class ShapeGraphNodeView : Node
    {
        private static readonly List<NodePort> PortBuffer = new(16);

        private readonly ShapeNode runtimeNode;
        private readonly Dictionary<PortId, ShapeGraphPortView> portsById = new();
        private readonly Image previewImage;
        private readonly Label domainLabel;
        private readonly Label statusLabel;
        private NodePreviewService previewService;

        public ShapeNode RuntimeNode => runtimeNode;
        public NodeId RuntimeNodeId => runtimeNode != null ? runtimeNode.Id : default;

        public ShapeGraphNodeView(ShapeNode node, IEdgeConnectorListener edgeListener)
        {
            runtimeNode = node;
            viewDataKey = node.Id.ToString();

            title = ObjectNamesSafe(node);
            style.minWidth = 180;

            PortBuffer.Clear();
            node.CollectPorts(PortBuffer);
            NodeDomainKind domain = PortColorMap.ClassifyNode(node.GetType(), PortBuffer);

            domainLabel = new Label(domain.ToString()) { name = "domain-pill" };
            domainLabel.AddToClassList("domain-pill");
            domainLabel.AddToClassList(domain.ToString().ToLowerInvariant());
            titleContainer.Add(domainLabel);

            previewImage = new Image
            {
                name = "node-preview",
                scaleMode = ScaleMode.ScaleToFit
            };
            previewImage.AddToClassList("node-preview");
            mainContainer.Insert(1, previewImage);

            statusLabel = new Label { name = "preview-status" };
            statusLabel.AddToClassList("preview-status");
            statusLabel.text = "Preview fill (editor)";
            if (domain == NodeDomainKind.Geometry)
                mainContainer.Insert(2, statusLabel);

            for (int i = 0; i < PortBuffer.Count; i++)
            {
                NodePort port = PortBuffer[i];
                ShapeGraphPortView portView = ShapeGraphPortView.Create(port, edgeListener);
                portsById[port.Id] = portView;
                if (port.Direction == PortDirection.Input)
                    inputContainer.Add(portView);
                else
                    outputContainer.Add(portView);
            }

            RefreshExpandedState();
            RefreshPorts();
            SetPosition(new Rect(node.GraphPosition, Vector2.zero));

            ApplyDomainClass(domain);
        }

        public ShapeGraphPortView GetPort(PortId portId)
        {
            portsById.TryGetValue(portId, out ShapeGraphPortView port);
            return port;
        }

        public void BindPreview(NodePreviewService service)
        {
            previewService = service;
            if (previewService == null || runtimeNode == null)
                return;

            previewService.Subscribe(runtimeNode.Id, OnPreviewReady);
        }

        public void UnbindPreview()
        {
            if (previewService != null && runtimeNode != null)
                previewService.Unsubscribe(runtimeNode.Id);
            previewService = null;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
        }

        private void OnPreviewReady(Texture texture)
        {
            previewImage.image = texture;
            if (statusLabel?.parent == null)
                return;

            if (texture == null)
            {
                statusLabel.text = "Preview unavailable";
                statusLabel.AddToClassList("error");
            }
            else
            {
                statusLabel.text = "Preview fill (editor)";
                statusLabel.RemoveFromClassList("error");
            }
        }

        private void ApplyDomainClass(NodeDomainKind domain)
        {
            AddToClassList("shape-graph-node");
            AddToClassList($"domain-{domain.ToString().ToLowerInvariant()}");
        }

        private static string ObjectNamesSafe(ShapeNode node)
        {
            if (node == null)
                return "Node";
            string n = node.name;
            if (string.IsNullOrEmpty(n))
                n = node.GetType().Name;
            return ObjectNames.NicifyVariableName(n.Replace("Node", string.Empty));
        }
    }
}

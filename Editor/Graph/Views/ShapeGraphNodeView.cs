using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Data;
using Galaretka.ShapeGraph.Data.Nodes.Values;
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
        private readonly bool previewable;
        private NodePreviewService previewService;

        public ShapeNode RuntimeNode => runtimeNode;
        public NodeId RuntimeNodeId => runtimeNode != null ? runtimeNode.Id : default;
        public bool IsPreviewable => previewable;

        public ShapeGraphNodeView(ShapeNode node, IEdgeConnectorListener edgeListener)
        {
            runtimeNode = node;
            viewDataKey = node.Id.ToString();

            title = ObjectNamesSafe(node);

            PortBuffer.Clear();
            node.CollectPorts(PortBuffer);
            NodeDomainKind domain = PortColorMap.ClassifyNode(node.GetType(), PortBuffer);
            previewable = IsPreviewableDomain(domain);

            bool isParameter = node is ParameterNode;
            style.minWidth = isParameter ? 120 : 180;

            domainLabel = new Label(domain.ToString()) { name = "domain-pill" };
            domainLabel.AddToClassList("domain-pill");
            domainLabel.AddToClassList(domain.ToString().ToLowerInvariant());
            titleContainer.Add(domainLabel);

            if (previewable)
            {
                previewImage = new Image
                {
                    name = "node-preview",
                    scaleMode = ScaleMode.ScaleToFit
                };
                previewImage.AddToClassList("node-preview");
                extensionContainer.Add(previewImage);

                if (domain == NodeDomainKind.Geometry)
                {
                    statusLabel = new Label { name = "preview-status" };
                    statusLabel.AddToClassList("preview-status");
                    statusLabel.text = "Preview fill (editor)";
                    extensionContainer.Add(statusLabel);
                }
                else
                {
                    statusLabel = null;
                }

                RefreshExpandedState();
            }
            else
            {
                previewImage = null;
                statusLabel = null;
            }

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

            ApplyDomainClass(domain, isParameter);
        }

        public ShapeGraphPortView GetPort(PortId portId)
        {
            portsById.TryGetValue(portId, out ShapeGraphPortView port);
            return port;
        }

        public void BindPreview(NodePreviewService service)
        {
            previewService = service;
            if (!previewable || previewService == null || runtimeNode == null) return;

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
            if (previewImage != null)
            {
                previewImage.image = texture;
            }

            if (statusLabel?.parent == null) return;

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

        private void ApplyDomainClass(NodeDomainKind domain, bool isParameter)
        {
            AddToClassList("shape-graph-node");
            AddToClassList($"domain-{domain.ToString().ToLowerInvariant()}");
            if (isParameter)
            {
                AddToClassList("parameter-node");
            }
        }

        private static bool IsPreviewableDomain(NodeDomainKind domain) =>
            domain == NodeDomainKind.Geometry
            || domain == NodeDomainKind.Style
            || domain == NodeDomainKind.Picture
            || domain == NodeDomainKind.Output;

        private static string ObjectNamesSafe(ShapeNode node)
        {
            if (node == null) return "Node";
            string n = node.name;
            if (string.IsNullOrEmpty(n))
            {
                n = node.GetType().Name;
            }
            if (node is ParameterNode) return n;
            return ObjectNames.NicifyVariableName(n.Replace("Node", string.Empty));
        }
    }
}

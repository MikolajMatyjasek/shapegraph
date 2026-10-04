using System;
using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Data;
using Galaretka.ShapeGraph.Data.Nodes.Values;
using Galaretka.ShapeGraph.Editor.Graph.Search;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Galaretka.ShapeGraph.Editor.Graph
{
    public sealed class ShapeNodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private ShapeGraphView graphView;
        private Vector2 createPosition;
        private Type filterFromOutputType;
        private NodeId connectFromNode;
        private PortId connectFromPort;
        private bool hasConnectContext;

        public void Initialize(ShapeGraphView view, Vector2 localGraphPosition)
        {
            graphView = view;
            createPosition = localGraphPosition;
            filterFromOutputType = null;
            hasConnectContext = false;
            connectFromNode = default;
            connectFromPort = default;
        }

        public void InitializeFilteredFromOutput(
            ShapeGraphView view,
            Vector2 localGraphPosition,
            Type fromOutputType,
            NodeId fromNode,
            PortId fromPort)
        {
            Initialize(view, localGraphPosition);
            filterFromOutputType = fromOutputType;
            connectFromNode = fromNode;
            connectFromPort = fromPort;
            hasConnectContext = fromOutputType != null;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent(
                    hasConnectContext ? "Create Compatible Node" : "Create Node"), 0)
            };

            string currentGroup = null;
            int added = 0;

            IReadOnlyList<NodeTypeEntry> entries = NodeTypeCatalog.GetEntries();
            for (int i = 0; i < entries.Count; i++)
            {
                NodeTypeEntry entry = entries[i];
                if (entry.Type == typeof(ParameterNode)) continue;

                if (hasConnectContext &&
                    !NodePortSignatureCache.HasCompatibleInput(entry.Type, filterFromOutputType)) continue;

                string group = GetGroup(entry.MenuPath);
                if (group != currentGroup)
                {
                    currentGroup = group;
                    tree.Add(new SearchTreeGroupEntry(new GUIContent(group), 1));
                }

                var request = new CreateNodeRequest(entry.Type, entry.MenuPath, entry.DisplayName);
                tree.Add(new SearchTreeEntry(new GUIContent(entry.DisplayName))
                {
                    level = 2,
                    userData = request
                });
                added++;
            }

            added += AppendParameterEntries(tree, ref currentGroup);

            if (hasConnectContext && added == 0)
            {
                tree.Add(new SearchTreeGroupEntry(new GUIContent("No compatible nodes"), 1));
            }

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (graphView == null || searchTreeEntry.userData is not CreateNodeRequest request)
                return false;

            if (hasConnectContext)
            {
                return graphView.CreateNodeFromOutputDrop(
                    request,
                    createPosition,
                    connectFromNode,
                    connectFromPort,
                    filterFromOutputType);
            }

            graphView.CreateNodeFromRequest(request, createPosition);
            return true;
        }

        private int AppendParameterEntries(List<SearchTreeEntry> tree, ref string currentGroup)
        {
            ShapeGraphAsset asset = graphView?.CommandsAsset;
            if (asset == null) return 0;

            IReadOnlyList<GraphParameter> parameters = asset.Parameters;
            int added = 0;
            for (int i = 0; i < parameters.Count; i++)
            {
                GraphParameter param = parameters[i];
                if (param == null) continue;

                // Parameter nodes are Value sources (output only) — never drop targets.
                if (hasConnectContext) continue;

                if (currentGroup != "Parameters")
                {
                    currentGroup = "Parameters";
                    tree.Add(new SearchTreeGroupEntry(new GUIContent("Parameters"), 1));
                }

                var request = new CreateNodeRequest(param);
                tree.Add(new SearchTreeEntry(new GUIContent(param.Name))
                {
                    level = 2,
                    userData = request
                });
                added++;
            }

            return added;
        }

        private static string GetGroup(string menuPath)
        {
            int slash = menuPath.IndexOf('/');
            return slash > 0 ? menuPath.Substring(0, slash) : "Nodes";
        }
    }
}

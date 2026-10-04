using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Galaretka.ShapeGraph.Editor.Graph
{
    public sealed class ShapeNodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private ShapeGraphView graphView;
        private Vector2 createPosition;

        public void Initialize(ShapeGraphView view, Vector2 localGraphPosition)
        {
            graphView = view;
            createPosition = localGraphPosition;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0)
            };

            string currentGroup = null;
            IReadOnlyList<Search.NodeTypeEntry> entries = Search.NodeTypeCatalog.GetEntries();
            for (int i = 0; i < entries.Count; i++)
            {
                Search.NodeTypeEntry entry = entries[i];
                string group = GetGroup(entry.MenuPath);
                if (group != currentGroup)
                {
                    currentGroup = group;
                    tree.Add(new SearchTreeGroupEntry(new GUIContent(group), 1));
                }

                tree.Add(new SearchTreeEntry(new GUIContent(entry.DisplayName))
                {
                    level = 2,
                    userData = entry
                });
            }

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (graphView == null || searchTreeEntry.userData is not Search.NodeTypeEntry entry)
                return false;

            Vector2 pos = createPosition;
            graphView.CreateNodeAt(entry.Type, pos);
            return true;
        }

        private static string GetGroup(string menuPath)
        {
            int slash = menuPath.IndexOf('/');
            return slash > 0 ? menuPath.Substring(0, slash) : "Nodes";
        }
    }
}

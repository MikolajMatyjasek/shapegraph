using System;
using System.Collections.Generic;
using System.Linq;
using Galaretka.ShapeGraph.Data;
using UnityEditor;

namespace Galaretka.ShapeGraph.Editor.Graph.Search
{
    public readonly struct NodeTypeEntry
    {
        public readonly Type Type;
        public readonly string MenuPath;
        public readonly string DisplayName;

        public NodeTypeEntry(Type type, string menuPath)
        {
            Type = type;
            MenuPath = menuPath;
            int slash = menuPath.LastIndexOf('/');
            DisplayName = slash >= 0 && slash < menuPath.Length - 1
                ? menuPath.Substring(slash + 1)
                : menuPath;
        }
    }

    /// <summary>
    /// Discovers concrete <see cref="ShapeNode"/> types annotated with <see cref="NodeMenuAttribute"/>.
    /// </summary>
    public static class NodeTypeCatalog
    {
        private static List<NodeTypeEntry> cached;

        public static IReadOnlyList<NodeTypeEntry> GetEntries()
        {
            if (cached != null)
                return cached;

            cached = new List<NodeTypeEntry>();
            foreach (Type type in TypeCache.GetTypesWithAttribute<NodeMenuAttribute>())
            {
                if (type.IsAbstract || !typeof(ShapeNode).IsAssignableFrom(type))
                    continue;

                var attr = (NodeMenuAttribute)Attribute.GetCustomAttribute(type, typeof(NodeMenuAttribute));
                if (attr == null || string.IsNullOrEmpty(attr.Path))
                    continue;

                cached.Add(new NodeTypeEntry(type, attr.Path));
            }

            cached = cached.OrderBy(e => e.MenuPath, StringComparer.OrdinalIgnoreCase).ToList();
            return cached;
        }

        public static void ClearCache() => cached = null;
    }
}

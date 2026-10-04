using System;
using Galaretka.ShapeGraph.Data;
using Galaretka.ShapeGraph.Data.Nodes.Values;

namespace Galaretka.ShapeGraph.Editor.Graph.Search
{
    public readonly struct CreateNodeRequest
    {
        public readonly Type NodeType;
        public readonly GraphParameter BindParameter;
        public readonly string MenuPath;
        public readonly string DisplayName;

        public bool IsParameter => BindParameter != null;

        public CreateNodeRequest(Type nodeType, string menuPath, string displayName)
        {
            NodeType = nodeType;
            BindParameter = null;
            MenuPath = menuPath ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
        }

        public CreateNodeRequest(GraphParameter parameter)
        {
            NodeType = typeof(ParameterNode);
            BindParameter = parameter;
            MenuPath = parameter != null ? $"Parameters/{parameter.Name}" : "Parameters";
            DisplayName = parameter != null ? parameter.Name : "Parameter";
        }
    }
}

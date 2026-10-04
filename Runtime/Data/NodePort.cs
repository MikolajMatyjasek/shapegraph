using System;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Typing;

namespace Galaretka.ShapeGraph.Data
{
    public enum PortDirection
    {
        Input,
        Output
    }

    [Serializable]
    public struct NodePort
    {
        public PortId Id;
        public string DisplayName;
        public PortDirection Direction;
        public PortTypeId TypeId;

        public NodePort(PortId id, string displayName, PortDirection direction, Type type)
        {
            Id = id;
            DisplayName = displayName;
            Direction = direction;
            TypeId = PortTypeRegistry.GetOrRegister(type);
        }

        public Type GetPortType()
        {
            PortTypeRegistry.TryResolve(TypeId, out Type type);
            return type;
        }
    }
}

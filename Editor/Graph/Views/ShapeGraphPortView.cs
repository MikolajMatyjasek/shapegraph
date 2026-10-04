using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Data;
using Galaretka.ShapeGraph.Editor.Graph.Theme;
using Galaretka.ShapeGraph.Typing;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Galaretka.ShapeGraph.Editor.Graph.Views
{
    public sealed class ShapeGraphPortView : Port
    {
        public PortId ShapePortId { get; }
        public PortTypeId ShapePortTypeId { get; }

        private ShapeGraphPortView(
            Orientation orientation,
            Direction direction,
            Capacity capacity,
            System.Type type,
            PortId portId,
            PortTypeId typeId)
            : base(orientation, direction, capacity, type)
        {
            ShapePortId = portId;
            ShapePortTypeId = typeId;
            portName = string.Empty;
        }

        public static ShapeGraphPortView Create(NodePort port, IEdgeConnectorListener listener)
        {
            Direction direction = port.Direction == PortDirection.Input ? Direction.Input : Direction.Output;
            Capacity capacity = port.Direction == PortDirection.Input ? Capacity.Single : Capacity.Multi;
            System.Type type = port.GetPortType() ?? typeof(object);

            var portView = new ShapeGraphPortView(
                Orientation.Horizontal,
                direction,
                capacity,
                type,
                port.Id,
                port.TypeId);

            portView.AddManipulator(new EdgeConnector<Edge>(listener));
            portView.portName = port.DisplayName;
            portView.portColor = PortColorMap.ForType(type);
            portView.tooltip = type.Name;
            return portView;
        }

        public bool CanConnectTo(ShapeGraphPortView other)
        {
            if (other == null || direction == other.direction)
                return false;

            ShapeGraphPortView output = direction == Direction.Output ? this : other;
            ShapeGraphPortView input = direction == Direction.Input ? this : other;
            return PortTypeRegistry.CanConnect(output.ShapePortTypeId, input.ShapePortTypeId);
        }
    }
}

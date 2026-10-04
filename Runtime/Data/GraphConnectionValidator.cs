using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Typing;

namespace Galaretka.ShapeGraph.Data
{
    public static class GraphConnectionValidator
    {
        private static readonly List<NodePort> PortBuffer = new(16);

        public static bool TryValidate(ShapeGraphAsset graph, NodeConnection connection, out ConnectionError error)
        {
            error = ConnectionError.None;

            if (graph == null)
            {
                error = ConnectionError.MissingNode;
                return false;
            }

            ShapeNode fromNode = graph.GetNodeById(connection.FromNodeId);
            ShapeNode toNode = graph.GetNodeById(connection.ToNodeId);
            if (fromNode == null || toNode == null)
            {
                error = ConnectionError.MissingNode;
                return false;
            }

            if (!TryFindPort(fromNode, connection.FromPortId, out NodePort fromPort) ||
                !TryFindPort(toNode, connection.ToPortId, out NodePort toPort))
            {
                error = ConnectionError.MissingPort;
                return false;
            }

            if (fromPort.Direction == PortDirection.Output && toPort.Direction == PortDirection.Output)
            {
                error = ConnectionError.OutputToOutput;
                return false;
            }

            if (fromPort.Direction == PortDirection.Input && toPort.Direction == PortDirection.Input)
            {
                error = ConnectionError.InputToInput;
                return false;
            }

            if (fromPort.Direction != PortDirection.Output || toPort.Direction != PortDirection.Input)
            {
                error = ConnectionError.DirectionMismatch;
                return false;
            }

            if (!PortTypeRegistry.CanConnect(fromPort.TypeId, toPort.TypeId))
            {
                error = ConnectionError.TypeIncompatible;
                return false;
            }

            return true;
        }

        private static bool TryFindPort(ShapeNode node, PortId portId, out NodePort port)
        {
            PortBuffer.Clear();
            node.CollectPorts(PortBuffer);
            for (int i = 0; i < PortBuffer.Count; i++)
            {
                if (PortBuffer[i].Id == portId)
                {
                    port = PortBuffer[i];
                    return true;
                }
            }

            port = default;
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Data;
using Galaretka.ShapeGraph.Typing;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Galaretka.ShapeGraph.Editor.Graph.Search
{
    public readonly struct PortSignature
    {
        public readonly PortId Id;
        public readonly Type ClrType;
        public readonly PortDirection Direction;

        public PortSignature(PortId id, Type clrType, PortDirection direction)
        {
            Id = id;
            ClrType = clrType;
            Direction = direction;
        }
    }

    public static class NodePortSignatureCache
    {
        private static readonly Dictionary<Type, List<PortSignature>> Cache = new();
        private static readonly List<NodePort> PortBuffer = new(16);

        public static IReadOnlyList<PortSignature> GetSignatures(Type nodeType)
        {
            if (nodeType == null || !typeof(ShapeNode).IsAssignableFrom(nodeType) || nodeType.IsAbstract) return Array.Empty<PortSignature>();

            if (Cache.TryGetValue(nodeType, out List<PortSignature> cached)) return cached;

            var list = new List<PortSignature>();
            ShapeNode temp = null;
            try
            {
                temp = (ShapeNode)ScriptableObject.CreateInstance(nodeType);
                PortBuffer.Clear();
                temp.CollectPorts(PortBuffer);
                for (int i = 0; i < PortBuffer.Count; i++)
                {
                    NodePort port = PortBuffer[i];
                    Type clr = port.GetPortType();
                    if (clr == null) continue;
                    list.Add(new PortSignature(port.Id, clr, port.Direction));
                }
            }
            finally
            {
                if (temp != null)
                {
                    Object.DestroyImmediate(temp);
                }
            }

            Cache[nodeType] = list;
            return list;
        }

        public static bool HasCompatibleInput(Type nodeType, Type fromOutputType)
        {
            if (fromOutputType == null) return false;
            IReadOnlyList<PortSignature> sigs = GetSignatures(nodeType);
            for (int i = 0; i < sigs.Count; i++)
            {
                if (sigs[i].Direction != PortDirection.Input) continue;
                if (PortTypeRegistry.CanConnect(fromOutputType, sigs[i].ClrType)) return true;
            }

            return false;
        }

        public static bool TryFindFirstCompatibleInput(Type nodeType, Type fromOutputType, out PortId portId)
        {
            portId = default;
            if (fromOutputType == null) return false;
            IReadOnlyList<PortSignature> sigs = GetSignatures(nodeType);
            for (int i = 0; i < sigs.Count; i++)
            {
                if (sigs[i].Direction != PortDirection.Input) continue;
                if (!PortTypeRegistry.CanConnect(fromOutputType, sigs[i].ClrType)) continue;
                portId = sigs[i].Id;
                return true;
            }

            return false;
        }

        public static void ClearCache() => Cache.Clear();
    }
}

using System;
using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using UnityEngine;

namespace Galaretka.ShapeGraph.Typing
{
    public static class PortTypeRegistry
    {
        private static readonly Dictionary<Type, PortTypeId> TypeToId = new();
        private static readonly Dictionary<int, Type> IdToType = new();
        private static readonly object Gate = new();

        static PortTypeRegistry()
        {
            GetOrRegister(typeof(IShape2D));
            GetOrRegister(typeof(Polygon2D));
            GetOrRegister(typeof(float));
            GetOrRegister(typeof(int));
            GetOrRegister(typeof(bool));
            GetOrRegister(typeof(Vector2));
            GetOrRegister(typeof(Color));
            GetOrRegister(typeof(Color32));
        }

        public static PortTypeId GetOrRegister(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            lock (Gate)
            {
                if (TypeToId.TryGetValue(type, out PortTypeId existing)) return existing;

                int hash = ComputeTypeHash(type);
                while (IdToType.TryGetValue(hash, out Type occupied) && occupied != type) 
                {
                    unchecked { hash++; }
                }

                var id = new PortTypeId(hash);
                TypeToId[type] = id;
                IdToType[hash] = type;
                return id;
            }
        }

        public static bool TryResolve(PortTypeId id, out Type type)
        {
            lock (Gate)
            {
                return IdToType.TryGetValue(id.Hash, out type);
            }
        }

        public static bool CanConnect(PortTypeId from, PortTypeId to)
        {
            if (!TryResolve(from, out Type fromType) || !TryResolve(to, out Type toType)) return false;
            return CanConnect(fromType, toType);
        }

        public static bool CanConnect(Type from, Type to)
        {
            if (from == null || to == null) return false;

            if (from == to) return true;

            if (IsExactOnlyType(from) || IsExactOnlyType(to)) return false;

            return to.IsAssignableFrom(from);
        }

        private static bool IsExactOnlyType(Type type)
        {
            return type.IsPrimitive
                   || type == typeof(decimal)
                   || type == typeof(Vector2)
                   || type == typeof(Vector3)
                   || type == typeof(Color)
                   || type == typeof(Color32);
        }

        private static int ComputeTypeHash(Type type)
        {
            string name = type.FullName ?? type.Name;
            unchecked
            {
                uint h = 2166136261u;
                for (int i = 0; i < name.Length; i++)
                {
                    h ^= name[i];
                    h *= 16777619u;
                }
                return (int)h;
            }
        }
    }
}

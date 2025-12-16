using System;
using System.Collections.Generic;
using UnityEngine;

namespace LegaFusionCore.Registries;

public static class LFCSpawnRegistry
{
    private static readonly Dictionary<Type, HashSet<Component>> registry = [];

    public static void Add(Component obj)
    {
        if (obj == null) return;

        Type type = obj.GetType();
        if (!registry.TryGetValue(type, out HashSet<Component> set))
        {
            set = [];
            registry[type] = set;
        }

        if (!set.Contains(obj))
            _ = set.Add(obj);
    }

    public static void Remove(Component obj)
    {
        if (obj == null) return;

        Type type = obj.GetType();
        if (registry.TryGetValue(type, out HashSet<Component> set))
            _ = set.RemoveWhere(c => c == null || c == obj);
    }

    public static List<T> GetAllAs<T>() where T : Component
    {
        List<T> result = [];
        foreach (KeyValuePair<Type, HashSet<Component>> kvp in registry)
        {
            _ = kvp.Value.RemoveWhere(c => c == null);

            foreach (Component c in kvp.Value)
            {
                if (c != null && c is T casted)
                    result.Add(casted);
            }
        }
        return result;
    }

    public static HashSet<Component> GetSetExact<T>() where T : Component
        => registry.TryGetValue(typeof(T), out HashSet<Component> set) ? set : null;

    public static void Clear() => registry.Clear();
}

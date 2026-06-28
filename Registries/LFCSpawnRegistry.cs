using System;
using System.Collections.Generic;
using UnityEngine;

namespace LegaFusionCore.Registries;

public static class LFCSpawnRegistry
{
    private static readonly Dictionary<Type, HashSet<Component>> SpawnRegistry = [];

    public static void Add(Component obj)
    {
        if (obj != null)
        {
            Type type = obj.GetType();
            if (!SpawnRegistry.TryGetValue(type, out HashSet<Component> set))
            {
                set = [];
                SpawnRegistry[type] = set;
            }

            if (!set.Contains(obj))
                _ = set.Add(obj);
        }
    }

    public static void Remove(Component obj)
    {
        if (obj != null)
        {
            Type type = obj.GetType();
            if (SpawnRegistry.TryGetValue(type, out HashSet<Component> set))
                _ = set.RemoveWhere(c => c == null || c == obj);
        }
    }

    public static List<T> GetAllAs<T>() where T : Component
    {
        List<T> result = [];
        foreach (KeyValuePair<Type, HashSet<Component>> kvp in SpawnRegistry)
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
        => SpawnRegistry.TryGetValue(typeof(T), out HashSet<Component> set) ? set : null;

    public static void Clear() => SpawnRegistry.Clear();
}

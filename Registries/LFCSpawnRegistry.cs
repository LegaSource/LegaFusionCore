using System;
using System.Collections.Generic;
using UnityEngine;

namespace LegaFusionCore.Registries;

public static class LFCSpawnRegistry
{
    private static readonly Dictionary<Type, HashSet<Component>> registry = [];

    public static void Add<T>(T obj) where T : Component
    {
        if (obj == null) return;

        Type type = typeof(T);
        if (!registry.TryGetValue(type, out HashSet<Component> set))
        {
            set = [];
            registry[type] = set;
        }
        _ = set.Add(obj);
    }

    public static void Remove<T>(T obj) where T : Component
    {
        if (obj == null) return;

        Type type = typeof(T);
        if (registry.TryGetValue(type, out HashSet<Component> set))
            _ = set.Remove(obj);
    }

    public static List<T> GetAllAs<T>() where T : Component
    {
        List<T> result = [];
        foreach (KeyValuePair<Type, HashSet<Component>> kvp in registry)
        {
            foreach (Component c in kvp.Value)
            {
                if (c is T casted) result.Add(casted);
            }
        }
        return result;
    }

    public static void Clear()
        => registry.Clear();
}

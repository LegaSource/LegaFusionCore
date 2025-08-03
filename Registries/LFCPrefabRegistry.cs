using System.Collections.Generic;
using UnityEngine;

namespace LegaFusionCore.Registries;

public static class LFCPrefabRegistry
{
    private static readonly Dictionary<string, GameObject> registry = [];

    public static void RegisterPrefab(string tag, GameObject prefab)
    {
        if (!registry.ContainsKey(tag))
            registry.Add(tag, prefab);
    }

    public static GameObject GetPrefab(string tag)
    {
        _ = registry.TryGetValue(tag, out GameObject prefab);
        return prefab;
    }
}

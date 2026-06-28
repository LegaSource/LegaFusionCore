using System.Collections.Generic;
using UnityEngine;

namespace LegaFusionCore.Registries;

public static class LFCPrefabRegistry
{
    private static readonly Dictionary<string, GameObject> PrefabRegistry = [];

    public static void RegisterPrefab(string tag, GameObject prefab)
    {
        if (!PrefabRegistry.ContainsKey(tag))
            PrefabRegistry.Add(tag, prefab);
    }

    public static GameObject GetPrefab(string tag)
    {
        _ = PrefabRegistry.TryGetValue(tag, out GameObject prefab);
        return prefab;
    }
}

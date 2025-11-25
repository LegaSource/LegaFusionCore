using System;
using System.Collections.Generic;
using UnityEngine;

namespace LegaFusionCore.Registries;

public static class LFCShaderFilterRegistry
{
    private static readonly Dictionary<string, Func<GameObject, bool>> filters = [];

    public static void AddFilter(string modName, Func<GameObject, bool> filter)
    {
        if (string.IsNullOrEmpty(modName))
        {
            LegaFusionCore.mls.LogWarning("[RendererVisibilityRegistry] Mod name is null or empty.");
            return;
        }

        if (filter == null)
        {
            LegaFusionCore.mls.LogWarning("[RendererVisibilityRegistry] Null filter provided for " + modName);
            return;
        }

        filters[modName] = filter;
    }

    public static void RemoveFilter(string modName)
    {
        if (string.IsNullOrEmpty(modName)) return;
        _ = filters.Remove(modName);
    }

    public static bool ShouldRender(GameObject sourceObject)
    {
        foreach (KeyValuePair<string, Func<GameObject, bool>> kvp in filters)
        {
            try
            {
                if (!kvp.Value(sourceObject))
                    return false;
            }
            catch (Exception ex)
            {
                LegaFusionCore.mls.LogWarning($"[RendererVisibilityRegistry] Filter '{kvp.Key}' threw an exception: {ex}");
            }
        }

        return true;
    }
}

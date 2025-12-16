using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LegaFusionCore.Registries;

public static class LFCPoweredLightsRegistry
{
    private static readonly Dictionary<Animator, HashSet<string>> lockRegistry = [];

    public static void AddLock(Animator poweredLight, string tag)
    {
        if (!lockRegistry.TryGetValue(poweredLight, out HashSet<string> tagSet))
        {
            tagSet = [];
            lockRegistry[poweredLight] = tagSet;
        }

        if (tagSet.Add(tag))
        {
            // Si c'est le premier tag -> on désactive réellement
            if (tagSet.Count == 1)
                SetPoweredLightEnabled(poweredLight, false);
        }
    }

    public static void RemoveLock(Animator poweredLight, string tag)
    {
        if (!lockRegistry.TryGetValue(poweredLight, out HashSet<string> tagSet)) return;

        if (tagSet.Remove(tag))
        {
            // Si plus aucun tag -> on réactive l’action
            if (tagSet.Count == 0)
                SetPoweredLightEnabled(poweredLight, true);
        }
    }

    public static void ClearLocks()
    {
        foreach (Animator poweredLight in lockRegistry.Keys.ToList())
        {
            if (!lockRegistry.TryGetValue(poweredLight, out HashSet<string> tagSet)) continue;

            tagSet.ToList().ForEach(t => RemoveLock(poweredLight, t));
            _ = lockRegistry.Remove(poweredLight);
        }
    }

    public static bool IsLocked(Animator poweredLight) => lockRegistry.TryGetValue(poweredLight, out HashSet<string> tagSet) && tagSet.Count > 0;
    private static void SetPoweredLightEnabled(Animator poweredLight, bool enabled) => poweredLight.SetBool("on", enabled);
}

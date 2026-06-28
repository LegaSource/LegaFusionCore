using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LegaFusionCore.Registries;

public static class LFCPoweredLightsRegistry
{
    private static readonly Dictionary<Animator, HashSet<string>> LightLockRegistry = [];

    public static void AddLock(Animator poweredLight, string tag)
    {
        if (!LightLockRegistry.TryGetValue(poweredLight, out HashSet<string> tagSet))
        {
            tagSet = [];
            LightLockRegistry[poweredLight] = tagSet;
        }
        SetPoweredLightEnabled(poweredLight, false);
        _ = tagSet.Add(tag);
    }

    public static void RemoveLock(Animator poweredLight, string tag)
    {
        if (LightLockRegistry.TryGetValue(poweredLight, out HashSet<string> tagSet))
        {
            if (tagSet.Remove(tag))
            {
                // Si plus aucun tag -> on réactive l’action
                if (tagSet.Count == 0)
                    SetPoweredLightEnabled(poweredLight, true);
            }
        }
    }

    public static void ClearLocks()
    {
        foreach (Animator poweredLight in LightLockRegistry.Keys.ToList())
        {
            if (LightLockRegistry.TryGetValue(poweredLight, out HashSet<string> tagSet))
            {
                tagSet.ToList().ForEach(t => RemoveLock(poweredLight, t));
                _ = LightLockRegistry.Remove(poweredLight);
            }
        }
    }

    public static bool IsLocked(Animator poweredLight) => LightLockRegistry.TryGetValue(poweredLight, out HashSet<string> tagSet) && tagSet.Count > 0;
    private static void SetPoweredLightEnabled(Animator poweredLight, bool enabled)
    {
        if (poweredLight != null && poweredLight.gameObject != null)
            poweredLight.SetBool("on", enabled);
    }
}

using HarmonyLib;
using UnityEngine;

namespace LegaFusionCore.Patches;

public class GrabbableObjectPatch
{
    [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.DestroyObjectInHand))]
    [HarmonyPostfix]
    private static void DestroyObjectRenderers(ref GrabbableObject __instance)
    {
        foreach (Renderer renderer in __instance.gameObject.GetComponentsInChildren<Renderer>())
            Object.Destroy(renderer);
    }
}

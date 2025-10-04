using HarmonyLib;
using LegaFusionCore.Behaviours.Addons;
using LegaFusionCore.CustomInputs;
using UnityEngine;

namespace LegaFusionCore.Patches;

public class GrabbableObjectPatch
{
    [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.SetScrapValue))]
    [HarmonyPostfix]
    private static void SetAddonScanNode(ref GrabbableObject __instance)
    {
        AddonComponent addonComponent = __instance.gameObject.GetComponent<AddonComponent>();
        if (addonComponent == null) return;

        ScanNodeProperties scanNode = __instance.gameObject.GetComponentInChildren<ScanNodeProperties>();
        if (scanNode != null) scanNode.subText += (scanNode.subText != null ? "\n" : "") + "Addon: " + addonComponent.addonName;
    }

    [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.GrabItem))]
    [HarmonyPostfix]
    private static void GrabObject(ref GrabbableObject __instance)
    {
        if (__instance.playerHeldBy == null || __instance.playerHeldBy != GameNetworkManager.Instance.localPlayerController) return;

        AddonComponent addonComponent = __instance.gameObject.GetComponent<AddonComponent>();
        if (addonComponent == null) return;

        addonComponent.StartCooldown(30);
    }

    [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.SetControlTipsForItem))]
    [HarmonyPostfix]
    private static void SetControlTipsForAddon(ref GrabbableObject __instance)
    {
        AddonComponent addonComponent = __instance.gameObject.GetComponent<AddonComponent>();
        if (addonComponent == null || addonComponent.isPassive) return;

        addonComponent.SetTipsForItem([AddonInput.Instance.GetAddonToolTip()]);
    }

    [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.DestroyObjectInHand))]
    [HarmonyPostfix]
    private static void DestroyObjectRenderers(ref GrabbableObject __instance)
    {
        foreach (Renderer renderer in __instance.gameObject.GetComponentsInChildren<Renderer>())
            Object.Destroy(renderer);
    }
}

using HarmonyLib;
using LegaFusionCore.Behaviours.Addons;
using System.Linq;

namespace LegaFusionCore.Patches;

internal class GrabbableObjectPatch
{
    [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.SetControlTipsForItem))]
    [HarmonyPostfix]
    private static void SetAddonToolTip(ref GrabbableObject __instance)
    {
        AddonComponent addonComponent = __instance.gameObject.GetComponent<AddonComponent>();
        if (addonComponent == null || !addonComponent.hasAddon || addonComponent.toolTip == null) return;

        HUDManager.Instance.ChangeControlTipMultiple(__instance.itemProperties.toolTips.Concat([addonComponent.toolTip]).ToArray(), holdingItem: true, __instance.itemProperties);
    }

    [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.SetScrapValue))]
    [HarmonyPostfix]
    private static void SetAddonScanNode(ref GrabbableObject __instance)
    {
        AddonComponent addonComponent = __instance.gameObject.GetComponent<AddonComponent>();
        if (addonComponent == null || !addonComponent.hasAddon) return;

        ScanNodeProperties scanNode = __instance.gameObject.GetComponentInChildren<ScanNodeProperties>();
        if (scanNode != null) scanNode.subText += (scanNode.subText != null ? "\n" : "") + "Addon: " + addonComponent.addonName;
    }
}

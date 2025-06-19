using LegaFusionCore.Behaviours.Addons;

namespace LegaFusionCore;

public class LFCUtilities
{
    public static void SetAddonComponent<T>(GrabbableObject grabbableObject, string addonName) where T : AddonComponent
    {
        T addonComponent = grabbableObject.gameObject.AddComponent<T>();
        addonComponent.hasAddon = true;
        addonComponent.addonName = addonName;

        ScanNodeProperties scanNode = grabbableObject.gameObject.GetComponentInChildren<ScanNodeProperties>();
        if (scanNode != null) scanNode.subText += (scanNode.subText != null ? "\n" : "") + "Addon: " + addonName;
    }
}

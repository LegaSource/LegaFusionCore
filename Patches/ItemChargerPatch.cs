using HarmonyLib;
using static LegaFusionCore.Registries.LFCShipFeatureRegistry;

namespace LegaFusionCore.Patches;

public class ItemChargerPatch
{
    [HarmonyPatch(typeof(ItemCharger), nameof(ItemCharger.Update))]
    [HarmonyPostfix]
    private static void UpdateItemCharger(ref ItemCharger __instance)
    {
        if (IsLocked(ShipFeatureType.ITEM_CHARGER))
        {
            __instance.triggerScript.disabledHoverTip = Constants.MESSAGE_NO_SHIP_ENERGY;
            __instance.triggerScript.interactable = false;
        }
    }
}

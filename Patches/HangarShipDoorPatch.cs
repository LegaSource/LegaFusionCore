using HarmonyLib;
using static LegaFusionCore.Registries.LFCShipFeatureRegistry;

namespace LegaFusionCore.Patches;

public class HangarShipDoorPatch
{
    [HarmonyPatch(typeof(HangarShipDoor), nameof(HangarShipDoor.Update))]
    [HarmonyPostfix]
    private static void UpdateShipDoor(HangarShipDoor __instance)
    {
        if (IsLocked(ShipFeatureType.SHIP_DOORS))
        {
            __instance.doorPower = 0;
            __instance.doorPowerDisplay.text = "0%";
        }
    }
}

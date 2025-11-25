using HarmonyLib;
using static LegaFusionCore.Registries.LFCShipFeatureRegistry;

namespace LegaFusionCore.Patches;

public class ShipTeleporterPatch
{
    [HarmonyPatch(typeof(ShipTeleporter), nameof(ShipTeleporter.Update))]
    [HarmonyPostfix]
    private static void UpdateShipTeleporter(ref ShipTeleporter __instance)
    {
        if (IsLocked(ShipFeatureType.SHIP_TELEPORTERS))
        {
            __instance.buttonTrigger.disabledHoverTip = Constants.MESSAGE_NO_SHIP_ENERGY;
            __instance.buttonTrigger.interactable = false;
        }
    }
}

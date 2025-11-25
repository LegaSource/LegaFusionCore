using HarmonyLib;
using static LegaFusionCore.Registries.LFCShipFeatureRegistry;

namespace LegaFusionCore.Patches;

public class ShipLightsPatch
{
    [HarmonyPatch(typeof(ShipLights), nameof(ShipLights.SetShipLightsBoolean))]
    [HarmonyPrefix]
    private static bool SetShipLightsBoolean() => !IsLocked(ShipFeatureType.SHIP_LIGHTS);

    [HarmonyPatch(typeof(ShipLights), nameof(ShipLights.SetShipLightsClientRpc))]
    [HarmonyPrefix]
    private static bool SetShipLightsClientRpc() => !IsLocked(ShipFeatureType.SHIP_LIGHTS);

    [HarmonyPatch(typeof(ShipLights), nameof(ShipLights.SetShipLightsOnLocalClientOnly))]
    [HarmonyPrefix]
    private static bool SetShipLightsOnLocalClientOnly() => !IsLocked(ShipFeatureType.SHIP_LIGHTS);

    [HarmonyPatch(typeof(ShipLights), nameof(ShipLights.ToggleShipLights))]
    [HarmonyPrefix]
    private static bool ToggleShipLights() => !IsLocked(ShipFeatureType.SHIP_LIGHTS);

    [HarmonyPatch(typeof(ShipLights), nameof(ShipLights.ToggleShipLightsOnLocalClientOnly))]
    [HarmonyPrefix]
    private static bool ToggleShipLightsOnLocalClientOnly() => !IsLocked(ShipFeatureType.SHIP_LIGHTS);
}

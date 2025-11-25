using HarmonyLib;
using static LegaFusionCore.Registries.LFCShipFeatureRegistry;

namespace LegaFusionCore.Patches;

public class ManualCameraRendererPatch
{
    [HarmonyPatch(typeof(ManualCameraRenderer), nameof(ManualCameraRenderer.SwitchScreenButton))]
    [HarmonyPrefix]
    private static bool SwitchScreenButton() => !IsLocked(ShipFeatureType.MAP_SCREEN);

    [HarmonyPatch(typeof(ManualCameraRenderer), nameof(ManualCameraRenderer.SwitchScreenOn))]
    [HarmonyPrefix]
    private static bool SwitchScreenOn() => !IsLocked(ShipFeatureType.MAP_SCREEN);

    [HarmonyPatch(typeof(ManualCameraRenderer), nameof(ManualCameraRenderer.SwitchScreenOnClientRpc))]
    [HarmonyPrefix]
    private static bool SwitchScreenOnClientRpc() => !IsLocked(ShipFeatureType.MAP_SCREEN);
}

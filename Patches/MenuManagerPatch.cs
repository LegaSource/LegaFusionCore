using HarmonyLib;
using LegaFusionCore.Registries;

namespace LegaFusionCore.Patches;

public class MenuManagerPatch
{
    [HarmonyPatch(typeof(MenuManager), nameof(MenuManager.Awake))]
    [HarmonyPostfix]
    private static void AwakeMenu()
    {
        LFCStatusEffectRegistry.ClearStatus();
        LFCShipFeatureRegistry.ResetCache();
        LFCShipFeatureRegistry.ClearLocks();
        LFCPoweredLightsRegistry.ClearLocks();
        LFCObjectStateRegistry.ClearFlickeringFlashlight();
    }
}

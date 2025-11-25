using HarmonyLib;
using static LegaFusionCore.Registries.LFCShipFeatureRegistry;

namespace LegaFusionCore.Patches;

public class TVScriptPatch
{
    [HarmonyPatch(typeof(TVScript), nameof(TVScript.TurnTVOnOff))]
    [HarmonyPrefix]
    private static bool TurnTVOnOff() => !IsLocked(ShipFeatureType.SHIP_TV);

    [HarmonyPatch(typeof(TVScript), nameof(TVScript.SwitchTVLocalClient))]
    [HarmonyPrefix]
    private static bool SwitchTVLocalClient() => !IsLocked(ShipFeatureType.SHIP_TV);

    [HarmonyPatch(typeof(TVScript), nameof(TVScript.TurnOnTVClientRpc))]
    [HarmonyPrefix]
    private static bool TurnOnTVClientRpc() => !IsLocked(ShipFeatureType.SHIP_TV);

    [HarmonyPatch(typeof(TVScript), nameof(TVScript.TurnOnTVAndSyncClientRpc))]
    [HarmonyPrefix]
    private static bool TurnOnTVAndSyncClientRpc() => !IsLocked(ShipFeatureType.SHIP_TV);

    [HarmonyPatch(typeof(TVScript), nameof(TVScript.TurnOffTVClientRpc))]
    [HarmonyPrefix]
    private static bool TurnOffTVClientRpc() => !IsLocked(ShipFeatureType.SHIP_TV);
}

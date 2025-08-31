using GameNetcodeStuff;
using HarmonyLib;
using LegaFusionCore.Registries;

namespace LegaFusionCore.Patches;

public class PlayerControllerBPatch
{
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
    [HarmonyPostfix]
    private static void ConnectPlayer(ref PlayerControllerB __instance)
    {
        if (GameNetworkManager.Instance?.localPlayerController == null || GameNetworkManager.Instance.localPlayerController != __instance) return;
        LFCStatRegistry.RegisterStat(Constants.STAT_SPEED, baseValue: __instance.movementSpeed, min: __instance.movementSpeed / 10f);
    }

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
    [HarmonyPrefix]
    private static void UpdatePlayer(ref PlayerControllerB __instance)
    {
        if (GameNetworkManager.Instance?.localPlayerController == null || GameNetworkManager.Instance.localPlayerController != __instance) return;
        __instance.movementSpeed = LFCStatRegistry.GetFinalValue(Constants.STAT_SPEED) ?? __instance.movementSpeed;
    }
}

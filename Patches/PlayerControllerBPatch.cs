using GameNetcodeStuff;
using HarmonyLib;
using LegaFusionCore.Registries;
using LegaFusionCore.Utilities;

namespace LegaFusionCore.Patches;

public class PlayerControllerBPatch
{
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
    [HarmonyPostfix]
    private static void ConnectPlayer(ref PlayerControllerB __instance)
    {
        if (LFCUtilities.ShouldBeLocalPlayer(__instance))
            LFCStatRegistry.RegisterStat(Constants.STAT_SPEED, min: 0.1f);
    }

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
    [HarmonyPrefix]
    private static void PreUpdatePlayer(ref PlayerControllerB __instance)
    {
        if (LFCUtilities.ShouldBeLocalPlayer(__instance))
            __instance.movementSpeed *= LFCStatRegistry.GetMultiplier(Constants.STAT_SPEED);
    }

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
    [HarmonyPostfix]
    private static void PostUpdatePlayer(ref PlayerControllerB __instance)
    {
        if (LFCUtilities.ShouldBeLocalPlayer(__instance))
            __instance.movementSpeed /= LFCStatRegistry.GetMultiplier(Constants.STAT_SPEED);
    }
}

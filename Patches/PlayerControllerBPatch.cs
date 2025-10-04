using GameNetcodeStuff;
using HarmonyLib;
using LegaFusionCore.Registries;
using UnityEngine;

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

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DamagePlayer))]
    [HarmonyPrefix]
    private static void DamagePlayer(ref PlayerControllerB __instance, ref int damageNumber)
    {
        if (GameNetworkManager.Instance.localPlayerController != __instance || !LFCStatusEffectRegistry.HasStatus(__instance.gameObject, LFCStatusEffectRegistry.StatusEffectType.POISON)) return;
        damageNumber = Mathf.CeilToInt(damageNumber * 1.5f);
    }
}

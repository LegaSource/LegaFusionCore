using HarmonyLib;
using LegaFusionCore.CustomInputs;
using LegaFusionCore.Managers.NetworkManagers;
using LegaFusionCore.Registries;
using Unity.Netcode;
using UnityEngine;

namespace LegaFusionCore.Patches;

internal class StartOfRoundPatch
{
    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Start))]
    [HarmonyBefore(["evaisa.lethallib"])]
    [HarmonyPostfix]
    private static void StartRound(ref StartOfRound __instance)
    {
        if (!NetworkManager.Singleton.IsHost || LFCNetworkManager.Instance != null) return;

        GameObject gameObject = Object.Instantiate(LegaFusionCore.managerPrefab, __instance.transform.parent);
        gameObject.GetComponent<NetworkObject>().Spawn();
        LegaFusionCore.mls.LogInfo("Spawning LFCNetworkManager");

        AddonInput.Instance.EnableInput();
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnDisable))]
    [HarmonyPostfix]
    public static void OnDisable()
    {
        LFCNetworkManager.Instance = null;
        LFCSpawnRegistry.Clear();
    }
}

using HarmonyLib;
using LegaFusionCore.CustomInputs;
using LegaFusionCore.Managers.NetworkManagers;
using LegaFusionCore.Registries;
using LegaFusionCore.Utilities;
using Unity.Netcode;
using UnityEngine;

namespace LegaFusionCore.Patches;

public class StartOfRoundPatch
{
    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Start))]
    [HarmonyBefore(["evaisa.lethallib"])]
    [HarmonyPostfix]
    private static void StartRound(ref StartOfRound __instance)
    {
        LoadVanillaPrefabs();
        AddonInput.Instance.EnableInput();

        if (!NetworkManager.Singleton.IsHost || LFCNetworkManager.Instance != null) return;

        GameObject gameObject = Object.Instantiate(LegaFusionCore.managerPrefab, __instance.transform.parent);
        gameObject.GetComponent<NetworkObject>().Spawn();
        LegaFusionCore.mls.LogInfo("Spawning LFCNetworkManager");
    }

    public static void LoadVanillaPrefabs()
    {
        GameObject gameObject = LFCUtilities.GetPrefabFromName(Constants.KNIFE_ITEM);
        if (gameObject != null)
        {
            GameObject tempInstance = Object.Instantiate(gameObject);
            tempInstance.SetActive(false);

            ParticleSystem ps = tempInstance.GetComponentInChildren<ParticleSystem>(true);
            if (ps != null)
            {
                LegaFusionCore.bloodParticle = Object.Instantiate(ps.gameObject);
                LegaFusionCore.bloodParticle.SetActive(false);
                Object.DontDestroyOnLoad(LegaFusionCore.bloodParticle);
            }

            Object.Destroy(tempInstance);
            LFCPrefabRegistry.RegisterPrefab($"{LegaFusionCore.modName}{LegaFusionCore.bloodParticle.name}", LegaFusionCore.bloodParticle);
        }
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnDisable))]
    [HarmonyPostfix]
    public static void OnDisable()
    {
        LFCNetworkManager.Instance = null;
        LFCSpawnRegistry.Clear();
    }
}

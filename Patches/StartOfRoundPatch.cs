using HarmonyLib;
using LegaFusionCore.Behaviours.Addons;
using LegaFusionCore.CustomInputs;
using LegaFusionCore.Managers.NetworkManagers;
using LegaFusionCore.Registries;
using LegaFusionCore.Utilities;
using System.Linq;
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
        LegaFusionCore.bloodParticle = LoadVanillaParticle(Constants.KNIFE_PREFAB, "BloodParticle");
        LegaFusionCore.groundParticle = LoadVanillaParticle(Constants.EARTH_LEVIATHAN_PREFAB, "AppearFromGround1");
    }

    public static GameObject LoadVanillaParticle(string prefabName, string particleName)
    {
        GameObject prefab = LFCUtilities.GetPrefabFromName(prefabName);
        if (prefab == null) return null;

        ParticleSystem particleSystem = prefab
            .GetComponentsInChildren<ParticleSystem>(true)
            .FirstOrDefault(p => p.gameObject.name.Equals(particleName));
        if (particleSystem == null) return null;

        GameObject particle = Object.Instantiate(particleSystem.gameObject);
        particle.name = particleSystem.gameObject.name;
        particle.SetActive(false);
        Object.DontDestroyOnLoad(particle);

        LFCPrefabRegistry.RegisterPrefab($"{LegaFusionCore.modName}{particle.name}", particle);
        return particle;
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.ShipLeave))]
    [HarmonyPostfix]
    public static void EndRound()
    {
        LFCStatusEffectRegistry.ClearStatus();
        LFCShipFeatureRegistry.ClearLocks();
        LFCPoweredLightsRegistry.ClearLocks();
        LFCObjectStateRegistry.ClearFlickeringFlashlight();

        foreach (AddonComponent addonComponent in Object.FindObjectsOfType<GrabbableObject>().Select(g => g.GetComponent<AddonComponent>()))
        {
            if (addonComponent == null) continue;
            addonComponent.StopCooldown();
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

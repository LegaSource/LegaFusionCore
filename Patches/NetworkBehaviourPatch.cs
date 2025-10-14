using HarmonyLib;
using LegaFusionCore.Registries;
using System;
using Unity.Netcode;

namespace LegaFusionCore.Patches;

public class NetworkBehaviourPatch
{
    private static readonly Type[] trackedTypes =
    {
        typeof(GrabbableObject),
        typeof(EnemyAI),
        typeof(EntranceTeleport)
    };

    [HarmonyPatch(typeof(NetworkBehaviour), nameof(NetworkBehaviour.InternalOnNetworkSpawn))]
    [HarmonyPostfix]
    private static void SpawnNetworkBehaviour(ref NetworkBehaviour __instance)
    {
        foreach (Type type in trackedTypes)
        {
            if (type.IsInstanceOfType(__instance))
            {
                LFCSpawnRegistry.Add(__instance);
                break;
            }
        }
    }

    [HarmonyPatch(typeof(NetworkBehaviour), nameof(NetworkBehaviour.InternalOnNetworkDespawn))]
    [HarmonyPostfix]
    private static void DestroyNetworkBehaviour(ref NetworkBehaviour __instance)
    {
        foreach (Type type in trackedTypes)
        {
            if (type.IsInstanceOfType(__instance))
            {
                LFCSpawnRegistry.Remove(__instance);
                break;
            }
        }
    }
}

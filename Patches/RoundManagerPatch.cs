using HarmonyLib;
using LegaFusionCore.Managers;
using static LegaFusionCore.Registries.LFCSpawnableItemRegistry;

namespace LegaFusionCore.Patches;

public class RoundManagerPatch
{
    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnScrapInLevel))]
    [HarmonyPostfix]
    private static void SpawnNewItems(ref RoundManager __instance)
    {
        foreach (SpawnableItem spawnableItem in GetAll())
        {
            for (int i = 0; i < spawnableItem.MaxSpawn; i++)
            {
                if (i < spawnableItem.MinSpawn || new System.Random().Next(1, 100) <= spawnableItem.Rarity)
                    LFCObjectsManager.SpawnNewObject(__instance, spawnableItem.Item);
            }
        }
    }
}

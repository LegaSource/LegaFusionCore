using HarmonyLib;
using LegaFusionCore.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
                    SpawnNewItem(__instance, spawnableItem.Item);
            }
        }
    }

    public static void SpawnNewItem(RoundManager roundManager, Item itemToSpawn)
    {
        try
        {
            System.Random random = new System.Random();
            List<RandomScrapSpawn> listRandomScrapSpawn = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>().Where(s => !s.spawnUsed).ToList();

            if (!listRandomScrapSpawn.Any()) return;

            int indexRandomScrapSpawn = random.Next(0, listRandomScrapSpawn.Count);
            RandomScrapSpawn randomScrapSpawn = listRandomScrapSpawn[indexRandomScrapSpawn];
            if (randomScrapSpawn.spawnedItemsCopyPosition)
            {
                randomScrapSpawn.spawnUsed = true;
                listRandomScrapSpawn.RemoveAt(indexRandomScrapSpawn);
            }
            else
            {
                randomScrapSpawn.transform.position = roundManager.GetRandomNavMeshPositionInBoxPredictable(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, roundManager.navHit, roundManager.AnomalyRandom) + (Vector3.up * itemToSpawn.verticalOffset);
            }

            Vector3 position = randomScrapSpawn.transform.position + (Vector3.up * 0.5f);
            _ = LFCObjectsManager.SpawnObjectForServer(itemToSpawn.spawnPrefab, position);
        }
        catch (Exception arg)
        {
            LegaFusionCore.mls.LogError($"Error in SpawnNewItem: {arg}");
        }
    }
}

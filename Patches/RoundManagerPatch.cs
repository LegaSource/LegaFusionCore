using HarmonyLib;
using LegaFusionCore.Managers;
using LegaFusionCore.Registries;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static LegaFusionCore.Registries.LFCSpawnableItemRegistry;

namespace LegaFusionCore.Patches;

public class RoundManagerPatch
{
    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnScrapInLevel))]
    [HarmonyPostfix]
    private static void SpawnNewItems()
    {
        System.Random random = new System.Random();
        foreach (SpawnableItem spawnableItem in GetAll())
        {
            for (int i = 0; i < spawnableItem.MaxSpawn; i++)
            {
                if (i < spawnableItem.MinSpawn || random.Next(0, 100) <= spawnableItem.Rarity)
                    _ = LFCObjectsManager.SpawnNewObject(spawnableItem.Item, spawnableItem.MinValue, spawnableItem.MaxValue);
            }
        }
    }

    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.FinishGeneratingLevel))]
    [HarmonyPostfix]
    private static void FinishGeneratingLevel()
    {
        Collider[] overlapColliders = new Collider[4096];
        int count = Physics.OverlapSphereNonAlloc(Vector3.zero, 1000f, overlapColliders, 33554432, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
            LFCTreesRegistry.AddTree(overlapColliders[i].gameObject);
    }

    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DestroyTreeAtPosition))]
    [HarmonyPostfix]
    private static void DestroyTreeAtPosition(Vector3 pos, float range = 5f)
    {
        foreach (GameObject treeObject in LFCTreesRegistry.GetTreesAtPosition(pos, range))
            LFCTreesRegistry.RemoveTree(treeObject);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.turnOnLights), MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> TurnOnLights(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        List<CodeInstruction> code = [.. instructions];

        MethodInfo miSetBool = AccessTools.Method(typeof(Animator), nameof(Animator.SetBool), [typeof(string), typeof(bool)]);
        MethodInfo miIsLocked = AccessTools.Method(typeof(LFCPoweredLightsRegistry), nameof(LFCPoweredLightsRegistry.IsLocked), [typeof(Animator)]);

        for (int i = 0; i < code.Count; i++)
        {
            if (!code[i].Calls(miSetBool)) continue;

            int ldstrOnIndex = -1;
            for (int j = i; j >= 0; j--)
            {
                if (code[j].opcode == OpCodes.Ldstr && code[j].operand is string s && s == "on")
                {
                    ldstrOnIndex = j;
                    break;
                }
            }
            if (ldstrOnIndex == -1) continue;

            int animatorLoadIndex = ldstrOnIndex - 1;
            if (animatorLoadIndex < 0) continue;

            Label notLocked = il.DefineLabel();
            Label skipAfterSetBool = il.DefineLabel();
            if (i + 1 < code.Count)
                code[i + 1].labels.Add(skipAfterSetBool);

            List<CodeInstruction> toInsert =
            [
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Call, miIsLocked),
                new CodeInstruction(OpCodes.Brfalse_S, notLocked),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Br_S, skipAfterSetBool),
            ];

            int insertionIndex = animatorLoadIndex + 1;
            code.InsertRange(insertionIndex, toInsert);
            code[insertionIndex + toInsert.Count].labels.Add(notLocked);

            i += toInsert.Count;
        }

        return code;
    }
}

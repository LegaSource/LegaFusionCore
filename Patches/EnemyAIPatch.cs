using GameNetcodeStuff;
using HarmonyLib;
using LegaFusionCore.Behaviours;
using UnityEngine;

namespace LegaFusionCore.Patches;

public class EnemyAIPatch
{
    [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.Start))]
    [HarmonyPostfix]
    public static void PostStart(EnemyAI __instance)
    {
        LFCEnemyDamageBehaviour.AddSpeedBehaviour(__instance);
        LFCEnemyDamageBehaviour.InitExtendedHP(__instance);
    }

    [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.HitEnemy))]
    [HarmonyPostfix]
    public static void HitEnemyPostfix(EnemyAI __instance, int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false, int hitID = -1)
    {
        int rateHP = LFCEnemyDamageBehaviour.rateHP;
        int extendedHP = Mathf.CeilToInt(LFCEnemyDamageBehaviour.GetExtendedHP(__instance) / rateHP);
        if (extendedHP < __instance.enemyHP) return;

        int playerId = playerWhoHit != null ? (int)playerWhoHit.playerClientId : -1;
        LFCEnemyDamageBehaviour.DamageEnemy(__instance, force * rateHP, playerId, playHitSFX, hitID, callHitEnemy: false);
    }

    [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.OnDestroy))]
    [HarmonyPostfix]
    public static void OnDestroyPostfix(EnemyAI __instance) => _ = LFCEnemyDamageBehaviour.extendedEnemyHP.Remove(__instance);
}

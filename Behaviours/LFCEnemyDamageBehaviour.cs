using GameNetcodeStuff;
using LegaFusionCore.Managers;
using LegaFusionCore.Registries;
using System.Collections.Generic;
using UnityEngine;

namespace LegaFusionCore.Behaviours;

public class LFCEnemyDamageBehaviour
{
    public static Dictionary<EnemyAI, int> extendedEnemyHP = [];
    public static int rateHP = 100;

    public static void AddSpeedBehaviour(EnemyAI __instance)
    {
        if (!__instance.TryGetComponent<LFCEnemySpeedBehaviour>(out _))
        {
            LFCEnemySpeedBehaviour speedBehaviour = __instance.gameObject.AddComponent<LFCEnemySpeedBehaviour>();
            speedBehaviour.enemy = __instance;
        }
    }

    public static void InitExtendedHP(EnemyAI __instance)
    {
        if (extendedEnemyHP.ContainsKey(__instance)) return;
        extendedEnemyHP[__instance] = __instance.enemyHP * rateHP;
    }

    public static int GetExtendedHP(EnemyAI enemy)
    {
        if (!extendedEnemyHP.ContainsKey(enemy)) InitExtendedHP(enemy);
        return extendedEnemyHP[enemy];
    }

    public static void SetExtendedHP(EnemyAI enemy, int value) => extendedEnemyHP[enemy] = Mathf.Max(0, value);

    public static void DamageEnemy(EnemyAI enemy, int force, int playerWhoHit = -1, bool playHitSFX = false, int hitID = -1, bool callHitEnemy = true)
    {
        InitExtendedHP(enemy);
        PlayerControllerB player = playerWhoHit == -1 ? null : StartOfRound.Instance.allPlayerObjects[playerWhoHit].GetComponent<PlayerControllerB>();

        if (LFCStatusEffectRegistry.HasStatus(enemy.gameObject, LFCStatusEffectRegistry.StatusEffectType.POISON))
            force = (int)(force * 1.5);

        // Inflige des dégâts à la vie étendue
        int newHP = GetExtendedHP(enemy) - force;
        SetExtendedHP(enemy, newHP);

        // Vérifie si la vie vanilla doit être mise à jour
        int expectedVanillaHP = Mathf.CeilToInt(newHP / (float)rateHP);
        if (callHitEnemy)
        {
            if (expectedVanillaHP < enemy.enemyHP)
            {
                int damageToVanilla = enemy.enemyHP - expectedVanillaHP;
                enemy.HitEnemyOnLocalClient(damageToVanilla, playerWhoHit: player, playHitSFX: playHitSFX, hitID: hitID);
            }
            else if (playHitSFX && enemy.enemyType?.hitBodySFX != null && !enemy.isEnemyDead)
            {
                enemy.creatureSFX.PlayOneShot(enemy.enemyType.hitBodySFX);
                WalkieTalkie.TransmitOneShotAudio(enemy.creatureSFX, enemy.enemyType.hitBodySFX);
            }
        }

        if (player != null && LFCStatusEffectRegistry.HasStatus(enemy.gameObject, LFCStatusEffectRegistry.StatusEffectType.BLEEDING))
        {
            int healAmount = Mathf.CeilToInt(force / 10f);
            LFCPlayerManager.HealPlayerOnLocalClient(player, healAmount);
        }
    }
}

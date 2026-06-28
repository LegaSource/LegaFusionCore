using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;

namespace LegaFusionCore.Behaviours;

public class LFCEnemyDamageBehaviour
{
    private static readonly Dictionary<EnemyAI, int> ExtendedEnemyHPRegistry = [];
    public static readonly int RateHP = 100;

    public static void InitExtendedHP(EnemyAI __instance)
    {
        if (!ExtendedEnemyHPRegistry.ContainsKey(__instance))
            ExtendedEnemyHPRegistry[__instance] = __instance.enemyHP * RateHP;
    }

    public static void RemoveExtendedEnemyHP(EnemyAI enemy) => ExtendedEnemyHPRegistry.Remove(enemy);
    public static Dictionary<EnemyAI, int> GetExtendedEnemyHP() => ExtendedEnemyHPRegistry;

    public static int GetExtendedHP(EnemyAI enemy)
    {
        if (!ExtendedEnemyHPRegistry.ContainsKey(enemy))
            InitExtendedHP(enemy);
        return ExtendedEnemyHPRegistry[enemy];
    }

    public static void SetExtendedHP(EnemyAI enemy, int value) => ExtendedEnemyHPRegistry[enemy] = Mathf.Max(0, value);

    public static void DamageEnemy(EnemyAI enemy, int force, int playerWhoHit = -1, bool playHitSFX = false, int hitID = -1, bool callHitEnemy = true)
    {
        InitExtendedHP(enemy);
        PlayerControllerB player = playerWhoHit == -1 ? null : StartOfRound.Instance.allPlayerObjects[playerWhoHit].GetComponent<PlayerControllerB>();

        // Inflige des dégâts à la vie étendue
        int newHP = GetExtendedHP(enemy) - force;
        SetExtendedHP(enemy, newHP);

        // Vérifie si la vie vanilla doit être mise à jour
        int expectedVanillaHP = Mathf.CeilToInt(newHP / (float)RateHP);
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
    }
}

using GameNetcodeStuff;
using LegaFusionCore.Managers.NetworkManagers;
using LegaFusionCore.Registries;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace LegaFusionCore.Managers;

public static class LFCEnemyManager
{
    public static bool CanDie(EnemyAI enemy)
    {
        if (enemy?.enemyType == null) return false;

        string[] enemiesNotTagged = ["Spring", "Jester", "Clay Surgeon", "Red Locust Bees", "Earth Leviathan", "Girl", "Blob", "Butler Bees", "RadMech", "Docile Locust Bees", "Puffer"];
        return enemy.enemyType.canDie && !enemiesNotTagged.Contains(enemy.enemyType.enemyName);
    }

    public static bool FoundClosestPlayerInRange(this EnemyAI enemy, int range, int senseRange, float width = 60f)
    {
        PlayerControllerB player = enemy.CheckLineOfSightForPlayer(width, range, senseRange);
        return player != null && enemy.PlayerIsTargetable(player) && (bool)(enemy.targetPlayer = player);
    }

    public static bool TargetClosestPlayerInAnyCase(this EnemyAI enemy)
    {
        enemy.mostOptimalDistance = 2000f;
        enemy.targetPlayer = null;
        for (int i = 0; i < StartOfRound.Instance.connectedPlayersAmount + 1; i++)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[i];
            if (!enemy.PlayerIsTargetable(player)) continue;

            enemy.tempDist = Vector3.Distance(enemy.transform.position, StartOfRound.Instance.allPlayerScripts[i].transform.position);
            if (enemy.tempDist < enemy.mostOptimalDistance)
            {
                enemy.mostOptimalDistance = enemy.tempDist;
                enemy.targetPlayer = StartOfRound.Instance.allPlayerScripts[i];
            }
        }
        return enemy.targetPlayer != null;
    }

    public static bool TargetOutsideChasedPlayer(this EnemyAI enemy)
    {
        if (enemy.targetPlayer != null && enemy.targetPlayer.isInsideFactory == enemy.isOutside)
        {
            enemy.GoTowardsEntrance();
            return true;
        }
        return false;
    }

    public static void GoTowardsEntrance(this EnemyAI enemy)
    {
        EntranceTeleport entranceTeleport = enemy.GetClosestEntrance();

        if (Vector3.Distance(enemy.transform.position, entranceTeleport.entrancePoint.position) < 1f)
        {
            Vector3 exitPosition = GetEntranceExitPosition(entranceTeleport);
            _ = enemy.StartCoroutine(enemy.GoTowardsCoroutine(exitPosition));
            return;
        }

        _ = enemy.SetDestinationToPosition(entranceTeleport.entrancePoint.position);
    }

    public static EntranceTeleport GetClosestEntrance(this EnemyAI enemy)
        => LFCSpawnRegistry.GetAllAs<EntranceTeleport>()
            .Where(e => e.isEntranceToBuilding == enemy.isOutside)
            .OrderBy(e => Vector3.Distance(enemy.transform.position, e.entrancePoint.position))
            .FirstOrDefault();

    public static Vector3 GetEntranceExitPosition(EntranceTeleport entranceTeleport)
        => LFCSpawnRegistry.GetAllAs<EntranceTeleport>()
            .FirstOrDefault(e => e.isEntranceToBuilding != entranceTeleport.isEntranceToBuilding && e.entranceId == entranceTeleport.entranceId)?
            .entrancePoint
            .position
                ?? Vector3.zero;

    public static IEnumerator GoTowardsCoroutine(this EnemyAI enemy, Vector3 position)
    {
        yield return new WaitForSeconds(1f);
        LFCNetworkManager.Instance.TeleportEnemyEveryoneRpc(enemy.thisNetworkObject, position, !enemy.isOutside);
    }

    public static void TeleportEnemy(this EnemyAI enemy, Vector3 teleportPosition, bool isOutside)
    {
        if (enemy != null)
        {
            enemy.SetEnemyOutside(isOutside);
            enemy.serverPosition = teleportPosition;
            enemy.transform.position = teleportPosition;
            _ = enemy.agent.Warp(teleportPosition);
            enemy.SyncPositionToClients();
        }
    }

    public static bool TryGetSafeRandomNavMeshPosition(this EnemyAI enemy, Vector3 origin, float radius, out Vector3 position, int maxAttempts = 15)
    {
        enemy.path1 = new NavMeshPath();
        for (int i = 0; i < maxAttempts; i++)
        {
            position = RoundManager.Instance.GetRandomNavMeshPositionInRadius(origin, radius);
            if (enemy.agent.CalculatePath(position, enemy.path1) && enemy.path1.status == NavMeshPathStatus.PathComplete)
                return true;
        }
        position = origin;
        return false;
    }

    public static IEnumerator WaitForFullAnimation(this EnemyAI enemy, string clipName, float maxDuration = 10, int layer = 0)
    {
        float timer = 0f;

        // Attendre que le clip démarre
        while (true)
        {
            AnimatorClipInfo[] clip = enemy.creatureAnimator.GetCurrentAnimatorClipInfo(layer);
            if (clip.Length > 0 && clip[0].clip.name.Contains(clipName)) break;

            timer += Time.deltaTime;
            if (timer > maxDuration) yield break;
            yield return null;
        }

        // Attendre fin du clip
        while (enemy.creatureAnimator.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1f)
        {
            timer += Time.deltaTime;
            if (timer > maxDuration) yield break;
            yield return null;
        }
    }
}

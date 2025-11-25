using LegaFusionCore.Managers.NetworkManagers;
using LegaFusionCore.Registries;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace LegaFusionCore.Managers;

public static class LFCEnemyManager
{
    public static bool CanDie(EnemyAI enemy)
    {
        if (enemy?.enemyType == null) return false;

        string[] enemiesNotTagged = ["Spring", "Jester", "Clay Surgeon", "Red Locust Bees", "Earth Leviathan", "Girl", "Blob", "Butler Bees", "RadMech", "Docile Locust Bees", "Puffer"];
        return enemy.enemyType.canDie && !enemiesNotTagged.Contains(enemy.enemyType.enemyName);
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
            _ = enemy.StartCoroutine(enemy.TeleportEnemyCoroutine(exitPosition));
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

    public static IEnumerator TeleportEnemyCoroutine(this EnemyAI enemy, Vector3 position)
    {
        yield return new WaitForSeconds(1f);
        LFCNetworkManager.Instance.TeleportEnemyEveryoneRpc(enemy.thisNetworkObject, position, !enemy.isOutside);
    }
}

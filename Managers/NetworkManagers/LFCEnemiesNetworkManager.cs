using Unity.Netcode;
using UnityEngine;

namespace LegaFusionCore.Managers.NetworkManagers;

public partial class LFCNetworkManager
{
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void TeleportEnemyEveryoneRpc(NetworkObjectReference enemyObj, Vector3 teleportPosition, bool isOutside)
    {
        if (!enemyObj.TryGet(out NetworkObject networkObject)) return;

        EnemyAI enemy = networkObject.gameObject.GetComponentInChildren<EnemyAI>();
        if (enemy != null)
        {
            enemy.SetEnemyOutside(isOutside);
            enemy.serverPosition = teleportPosition;
            transform.position = teleportPosition;
            _ = enemy.agent.Warp(teleportPosition);
            enemy.SyncPositionToClients();
        }
    }
}

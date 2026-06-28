using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace LegaFusionCore.Managers.NetworkManagers;

public partial class LFCNetworkManager
{
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void TeleportEnemyEveryoneRpc(NetworkObjectReference enemyObj, Vector3 teleportPosition, bool isOutside)
    {
        if (enemyObj.TryGet(out NetworkObject networkObject))
            networkObject.gameObject.GetComponentInChildren<EnemyAI>()?.TeleportEnemy(teleportPosition, isOutside);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void StunEnemyEveryoneRpc(NetworkObjectReference enemyObject, bool setToStunned, float stunTime = 1f, int playerId = -1)
    {
        if (enemyObject.TryGet(out NetworkObject networkObject))
        {
            PlayerControllerB stunnedByPlayer = playerId != -1 ? StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>() : null;
            EnemyAI enemy = networkObject.gameObject.GetComponentInChildren<EnemyAI>();
            enemy?.SetEnemyStunned(setToStunned, stunTime, stunnedByPlayer);
        }
    }
}

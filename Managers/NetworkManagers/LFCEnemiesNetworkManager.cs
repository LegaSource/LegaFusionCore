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
}

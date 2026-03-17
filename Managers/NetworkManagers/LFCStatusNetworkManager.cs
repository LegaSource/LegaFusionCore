using GameNetcodeStuff;
using LegaFusionCore.Registries;
using Unity.Netcode;

namespace LegaFusionCore.Managers.NetworkManagers;

public partial class LFCNetworkManager
{
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ApplyStatusEveryoneRpc(int playerId, NetworkObjectReference enemyObj, int type, int duration, int totalDamage = 0)
        => ApplyStatusEveryoneRpc(playerId, enemyObj, type, duration, totalDamage, default);

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ApplyStatusEveryoneRpc(int playerId, NetworkObjectReference enemyObj, int type, int duration, int totalDamage = 0, NetworkObjectReference enemyWhoHitObj = default)
    {
        if (enemyObj.TryGet(out NetworkObject networkObjectEnemy))
        {
            EnemyAI enemy = networkObjectEnemy.gameObject.GetComponentInChildren<EnemyAI>();
            EnemyAI enemyWhoHit = enemyWhoHitObj.TryGet(out NetworkObject networkObjectEnemyWhoHit) ? networkObjectEnemyWhoHit.gameObject.GetComponentInChildren<EnemyAI>() : null;
            LFCStatusEffectRegistry.ApplyStatus(enemy.gameObject, (LFCStatusEffectRegistry.StatusEffectType)type, playerId, duration, totalDamage, enemyWhoHit);
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ApplyStatusEveryoneRpc(int playerId, int targetId, int type, int duration, int totalDamage = 0)
        => ApplyStatusEveryoneRpc(playerId, targetId, type, duration, totalDamage, default);

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ApplyStatusEveryoneRpc(int playerId, int targetId, int type, int duration, int totalDamage = 0, NetworkObjectReference enemyWhoHitObj = default)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[targetId].GetComponent<PlayerControllerB>();
        EnemyAI enemyWhoHit = enemyWhoHitObj.TryGet(out NetworkObject networkObjectEnemyWhoHit) ? networkObjectEnemyWhoHit.gameObject.GetComponentInChildren<EnemyAI>() : null;
        LFCStatusEffectRegistry.ApplyStatus(player.gameObject, (LFCStatusEffectRegistry.StatusEffectType)type, playerId, duration, totalDamage, enemyWhoHit);
    }
}

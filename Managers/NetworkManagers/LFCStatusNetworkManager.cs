using GameNetcodeStuff;
using LegaFusionCore.Registries;
using Unity.Netcode;

namespace LegaFusionCore.Managers.NetworkManagers;

public partial class LFCNetworkManager
{
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ApplyStatusEveryoneRpc(int playerId, NetworkObjectReference enemyObj, int type, int duration, int totalDamage = 0)
    {
        if (!enemyObj.TryGet(out NetworkObject networkObjectEnemy)) return;

        EnemyAI enemy = networkObjectEnemy.gameObject.GetComponentInChildren<EnemyAI>();
        LFCStatusEffectRegistry.ApplyStatus(enemy.gameObject, (LFCStatusEffectRegistry.StatusEffectType)type, playerId, duration, totalDamage);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ApplyStatusEveryoneRpc(int playerId, int targetId, int type, int duration, int totalDamage = 0)
    {
        PlayerControllerB targetedPlayer = StartOfRound.Instance.allPlayerObjects[targetId].GetComponent<PlayerControllerB>();
        LFCStatusEffectRegistry.ApplyStatus(targetedPlayer.gameObject, (LFCStatusEffectRegistry.StatusEffectType)type, playerId, duration, totalDamage);
    }
}

using GameNetcodeStuff;
using LegaFusionCore.Utilities;
using Unity.Netcode;
using UnityEngine;

namespace LegaFusionCore.Managers.NetworkManagers;

public partial class LFCNetworkManager
{
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void TeleportPlayerEveryoneRpc(int playerId, Vector3 position, bool isInElevator, bool isInHangarShipRoom, bool isInsideFactory, bool withRotation = false, float rotation = 0, bool withSpawnAnimation = false)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        player.averageVelocity = 0f;
        player.velocityLastFrame = Vector3.zero;
        player.isInElevator = isInElevator;
        player.isInHangarShipRoom = isInHangarShipRoom;
        player.isInsideFactory = isInsideFactory;
        player.TeleportPlayer(position, withRotation, rotation);
        if (withSpawnAnimation) player.SpawnPlayerAnimation();

        for (int i = 0; i < player.ItemSlots.Length; i++)
        {
            if (player.ItemSlots[i] == null) continue;
            player.ItemSlots[i].isInFactory = isInsideFactory;
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void DamagePlayerEveryoneRpc(int playerId, int damageNumber)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        if (LFCUtilities.ShouldBeLocalPlayer(player)) player.DamagePlayer(damageNumber, hasDamageSFX: true, callRPC: true, CauseOfDeath.Unknown);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void KillPlayerEveryoneRpc(int playerId, Vector3 velocity, bool spawnBody, int causeOfDeath)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        if (LFCUtilities.ShouldBeLocalPlayer(player)) player.KillPlayer(velocity, spawnBody, (CauseOfDeath)causeOfDeath);
    }
}

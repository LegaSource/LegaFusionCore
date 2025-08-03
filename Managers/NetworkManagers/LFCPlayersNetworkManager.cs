using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace LegaFusionCore.Managers.NetworkManagers;

public partial class LFCNetworkManager
{
    [ServerRpc(RequireOwnership = false)]
    public void TeleportPlayerServerRpc(int playerId, Vector3 position, bool isInElevator, bool isInHangarShipRoom, bool isInsideFactory, bool withRotation = false, float rotation = 0, bool withSpawnAnimation = false)
        => TeleportPlayerClientRpc(playerId, position, isInElevator, isInHangarShipRoom, isInsideFactory, withRotation, rotation, withSpawnAnimation);

    [ClientRpc]
    public void TeleportPlayerClientRpc(int playerId, Vector3 position, bool isInElevator, bool isInHangarShipRoom, bool isInsideFactory, bool withRotation = false, float rotation = 0, bool withSpawnAnimation = false)
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

    [ServerRpc(RequireOwnership = false)]
    public void KillPlayerServerRpc(int playerId, Vector3 velocity, bool spawnBody, int causeOfDeath)
        => KillPlayerClientRpc(playerId, velocity, spawnBody, causeOfDeath);

    [ClientRpc]
    public void KillPlayerClientRpc(int playerId, Vector3 velocity, bool spawnBody, int causeOfDeath)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        if (player != GameNetworkManager.Instance.localPlayerController) return;

        player.KillPlayer(velocity, spawnBody, (CauseOfDeath)causeOfDeath);
    }
}

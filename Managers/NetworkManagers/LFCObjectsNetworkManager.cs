using GameNetcodeStuff;
using Unity.Netcode;

namespace LegaFusionCore.Managers.NetworkManagers;

public partial class LFCNetworkManager
{
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void SetScrapValueEveryoneRpc(NetworkObjectReference obj, int value)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;
        networkObject.gameObject.GetComponentInChildren<GrabbableObject>()?.SetScrapValue(value);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ForceGrabObjectEveryoneRpc(NetworkObjectReference obj, int playerId)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        if (player != GameNetworkManager.Instance.localPlayerController) return;

        _ = StartCoroutine(LFCObjectsManager.ForceGrabObjectCoroutine(networkObject.gameObject.GetComponentInChildren<GrabbableObject>(), player));
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void DestroyObjectEveryoneRpc(NetworkObjectReference obj)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
        grabbableObject?.DestroyObjectInHand(grabbableObject.playerHeldBy);
    }
}

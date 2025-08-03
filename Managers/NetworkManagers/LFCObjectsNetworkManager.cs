using GameNetcodeStuff;
using Unity.Netcode;

namespace LegaFusionCore.Managers.NetworkManagers;

public partial class LFCNetworkManager
{
    [ServerRpc(RequireOwnership = false)]
    public void SetScrapValueServerRpc(NetworkObjectReference obj, int value)
        => SetScrapValueClientRpc(obj, value);

    [ClientRpc]
    public void SetScrapValueClientRpc(NetworkObjectReference obj, int value)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;
        networkObject.gameObject.GetComponentInChildren<GrabbableObject>()?.SetScrapValue(value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ForceGrabObjectServerRpc(NetworkObjectReference obj, int playerId)
        => ForceGrabObjectClientRpc(obj, playerId);

    [ClientRpc]
    public void ForceGrabObjectClientRpc(NetworkObjectReference obj, int playerId)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        if (player != GameNetworkManager.Instance.localPlayerController) return;

        _ = StartCoroutine(LFCObjectsManager.ForceGrabObjectCoroutine(networkObject.gameObject.GetComponentInChildren<GrabbableObject>(), player));
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyObjectServerRpc(NetworkObjectReference obj)
        => DestroyObjectClientRpc(obj);

    [ClientRpc]
    public void DestroyObjectClientRpc(NetworkObjectReference obj)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
        grabbableObject?.DestroyObjectInHand(grabbableObject.playerHeldBy);
    }
}

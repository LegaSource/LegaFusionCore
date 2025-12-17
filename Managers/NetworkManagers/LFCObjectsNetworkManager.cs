using GameNetcodeStuff;
using LegaFusionCore.Utilities;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

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
        if (LFCUtilities.ShouldBeLocalPlayer(player))
            _ = StartCoroutine(LFCObjectsManager.ForceGrabObjectCoroutine(networkObject.gameObject.GetComponentInChildren<GrabbableObject>(), player));
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ForceDiscardObjectEveryoneRpc(NetworkObjectReference obj, int playerId)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        if (player.currentlyHeldObjectServer != null)
        {
            GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
            if (grabbableObject != null && grabbableObject == player.currentlyHeldObjectServer)
                grabbableObject.DropHeldItem(player);
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void DestroyObjectEveryoneRpc(NetworkObjectReference obj)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
        if (grabbableObject == null) return;

        if (grabbableObject is FlashlightItem flashlight && flashlight.isBeingUsed)
        {
            flashlight.isBeingUsed = false;
            flashlight.usingPlayerHelmetLight = false;
            flashlight.flashlightBulbGlow.enabled = false;
            flashlight.SwitchFlashlight(on: false);
        }
        else if (grabbableObject is BeltBagItem beltBagItem)
        {
            SkinnedMeshRenderer[] skinnedMeshRenderers = beltBagItem.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers.ToList())
                Destroy(skinnedMeshRenderer);
        }
        grabbableObject.DestroyObjectInHand(grabbableObject.playerHeldBy);
    }
}

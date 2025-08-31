using LegaFusionCore.Utilities;
using Unity.Netcode;
using UnityEngine;
using static LegaFusionCore.Behaviours.Addons.AddonTargetDatabase;

namespace LegaFusionCore.Behaviours.Addons;

public class AddonProp<T> : PhysicsProp where T : AddonComponent
{
    public virtual AddonTargetType TargetType { get; } = AddonTargetType.ALL;

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        if (!buttonDown || playerHeldBy == null) return;
        if (Physics.Raycast(new Ray(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward), out RaycastHit hit, 3f, 832))
        {
            GrabbableObject grabbableObject = hit.transform.GetComponent<GrabbableObject>();
            if (grabbableObject == null) return;
            if (!IsValidForItem(grabbableObject.itemProperties?.itemName))
            {
                HUDManager.Instance.DisplayTip("Impossible action", "This addon cannot be assigned to this item");
                return;
            }
            AddonComponent addonComponent = grabbableObject.GetComponent<AddonComponent>();
            if (addonComponent != null)
            {
                HUDManager.Instance.DisplayTip("Impossible action", "This item already has an addon");
                return;
            }

            SetAddonServerRpc(grabbableObject.GetComponent<NetworkObject>());
        }
    }

    public bool IsValidForItem(string itemName)
        => !string.IsNullOrEmpty(itemName) && TargetType switch
        {
            AddonTargetType.ALL => true,
            AddonTargetType.FLASHLIGHT => flashlightNames.Contains(itemName),
            AddonTargetType.KNIFE => knifeNames.Contains(itemName),
            AddonTargetType.SHOVEL => shovelNames.Contains(itemName),
            AddonTargetType.SPRAY_PAINT => sprayPaintNames.Contains(itemName),
            AddonTargetType.WALKIE_TALKIE => walkieTalkieNames.Contains(itemName),
            AddonTargetType.BOOMBOX => boomboxNames.Contains(itemName),
            AddonTargetType.SHOTGUN => shotgunNames.Contains(itemName),
            _ => false
        };

    [ServerRpc(RequireOwnership = false)]
    private void SetAddonServerRpc(NetworkObjectReference obj)
        => SetAddonClientRpc(obj);

    [ClientRpc]
    private void SetAddonClientRpc(NetworkObjectReference obj)
    {
        if (!obj.TryGet(out NetworkObject networkObject)) return;

        GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
        if (grabbableObject == null) return;

        LFCUtilities.SetAddonComponent<T>(grabbableObject, itemProperties.itemName);
        DestroyObjectInHand(playerHeldBy);
    }
}

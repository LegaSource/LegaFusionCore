using GameNetcodeStuff;
using LegaFusionCore.Managers.NetworkManagers;
using LegaFusionCore.Registries;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LegaFusionCore.Managers;

public static class LFCObjectsManager
{
    public static GrabbableObject SpawnObjectForServer(GameObject spawnPrefab, Vector3 position)
        => SpawnObjectForServer(spawnPrefab, position, Quaternion.identity);

    public static GrabbableObject SpawnObjectForServer(GameObject spawnPrefab, Vector3 position, Quaternion rotation)
    {
        if (!GameNetworkManager.Instance.localPlayerController.IsServer && !GameNetworkManager.Instance.localPlayerController.IsHost) return null;

        GameObject gameObject = Object.Instantiate(spawnPrefab, position, rotation, StartOfRound.Instance.propsContainer);
        GrabbableObject grabbableObject = gameObject.GetComponent<GrabbableObject>();
        grabbableObject.fallTime = 0f;
        gameObject.GetComponent<NetworkObject>().Spawn();
        return grabbableObject;
    }

    public static IEnumerator ForceGrabObjectCoroutine(GrabbableObject grabbableObject, PlayerControllerB player)
    {
        if (player.FirstEmptyItemSlot() == -1) player.DropAllHeldItemsAndSync();

        yield return new WaitForSeconds(0.2f);

        // Si déjà en cours de grab, attendre que l'autre objet soit bien pris en compte (2s max)
        float timePassed = 0f;
        while (player.isGrabbingObjectAnimation && timePassed < 2f)
        {
            yield return new WaitForSeconds(0.1f);
            timePassed += 0.1f;
        }

        ForceGrabObject(grabbableObject, player);
    }

    public static void ForceGrabObject(GrabbableObject grabbableObject, PlayerControllerB player)
    {
        player.currentlyGrabbingObject = grabbableObject;
        player.grabInvalidated = false;
        player.currentlyGrabbingObject.InteractItem();

        if (player.currentlyGrabbingObject.grabbable && player.FirstEmptyItemSlot() != -1)
        {
            player.playerBodyAnimator.SetBool("GrabInvalidated", value: false);
            player.playerBodyAnimator.SetBool("GrabValidated", value: false);
            player.playerBodyAnimator.SetBool("cancelHolding", value: false);
            player.playerBodyAnimator.ResetTrigger("Throw");
            player.SetSpecialGrabAnimationBool(setTrue: true);
            player.isGrabbingObjectAnimation = true;
            player.cursorIcon.enabled = false;
            player.cursorTip.text = "";
            player.twoHanded = player.currentlyGrabbingObject.itemProperties.twoHanded;
            player.carryWeight = Mathf.Clamp(player.carryWeight + (player.currentlyGrabbingObject.itemProperties.weight - 1f), 1f, 10f);
            player.grabObjectAnimationTime = player.currentlyGrabbingObject.itemProperties.grabAnimationTime > 0f
                ? player.currentlyGrabbingObject.itemProperties.grabAnimationTime
                : 0.4f;

            if (!player.isTestingPlayer) player.GrabObjectServerRpc(player.currentlyGrabbingObject.NetworkObject);
            if (player.grabObjectCoroutine != null) player.StopCoroutine(player.grabObjectCoroutine);
            player.grabObjectCoroutine = player.StartCoroutine(player.GrabObject());
        }
    }

    public static void DestroyObjectsOfTypeAllForServer<T>() where T : GrabbableObject
        => LFCSpawnRegistry.GetAllAs<GrabbableObject>().ForEach(g => { if (g is T o) DestroyObjectOfTypeForServer(o); });

    public static void DestroyObjectOfTypeForServer<T>(T grabbableObject) where T : GrabbableObject
    {
        NetworkObject networkObject = grabbableObject?.GetComponent<NetworkObject>();
        if (networkObject == null || !networkObject.IsSpawned) return;

        LFCNetworkManager.Instance.DestroyObjectClientRpc(networkObject);
    }
}

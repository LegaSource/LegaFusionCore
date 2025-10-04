using GameNetcodeStuff;
using LegaFusionCore.Behaviours.Addons;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LegaFusionCore.Utilities;

public class LFCUtilities
{
    public static bool IsServer => GameNetworkManager.Instance.localPlayerController.IsServer || GameNetworkManager.Instance.localPlayerController.IsHost;

    public static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (list[randomIndex], list[i]) = (list[i], list[randomIndex]);
        }
    }

    public static GameObject GetPrefabFromName(string name)
    {
        GameObject item = null;
        foreach (NetworkPrefabsList networkPrefabList in NetworkManager.Singleton.NetworkConfig.Prefabs.NetworkPrefabsLists ?? Enumerable.Empty<NetworkPrefabsList>())
        {
            foreach (NetworkPrefab networkPrefab in networkPrefabList.PrefabList ?? Enumerable.Empty<NetworkPrefab>())
            {
                GrabbableObject grabbableObject = networkPrefab.Prefab.GetComponent<GrabbableObject>();
                if (grabbableObject == null || grabbableObject.itemProperties == null) continue;
                if (!grabbableObject.itemProperties.itemName.Equals(name)) continue;

                item = networkPrefab.Prefab;
                if (item != null) break;
            }
        }
        return item;
    }

    public static T GetSafeComponent<T>(GameObject gameObject) where T : Component
        => gameObject == null || gameObject is not Object obj || !obj ? null : gameObject.GetComponent<T>();

    public static void SetAddonComponent<T>(GrabbableObject grabbableObject, string addonName, bool isPassive = false) where T : AddonComponent
    {
        T addonComponent = grabbableObject.gameObject.AddComponent<T>();
        addonComponent.grabbableObject = grabbableObject;
        addonComponent.addonName = addonName;
        addonComponent.isPassive = isPassive;

        ScanNodeProperties scanNode = grabbableObject.gameObject.GetComponentInChildren<ScanNodeProperties>();
        if (scanNode != null) scanNode.subText += (scanNode.subText != null ? "\n" : "") + "Addon: " + addonName;
    }

    public static T GetAddonComponent<T>(PlayerControllerB player) where T : AddonComponent
    {
        if (player == null) return null;

        T addonComponent = null;
        for (int i = 0; i < player.ItemSlots.Length; i++)
        {
            addonComponent = GetAddonComponent<T>(player.ItemSlots[i]);
            if (addonComponent != null) break;
        }
        return addonComponent;
    }

    public static T GetAddonComponent<T>(GrabbableObject grabbableObject) where T : AddonComponent => grabbableObject?.GetComponent<T>();
}

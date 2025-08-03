using GameNetcodeStuff;
using LegaFusionCore.Behaviours.Addons;
using System.Collections.Generic;
using UnityEngine;

namespace LegaFusionCore.Utilities;

public class LFCUtilities
{
    public static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (list[randomIndex], list[i]) = (list[i], list[randomIndex]);
        }
    }

    public static void SetAddonComponent<T>(GrabbableObject grabbableObject, string addonName, bool isPassive = false) where T : AddonComponent
    {
        T addonComponent = grabbableObject.gameObject.AddComponent<T>();
        addonComponent.hasAddon = true;
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

    public static T GetAddonComponent<T>(GrabbableObject grabbableObject) where T : AddonComponent
        => grabbableObject?.GetComponent<T>();
}

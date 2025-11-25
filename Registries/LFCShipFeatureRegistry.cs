using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LegaFusionCore.Registries;

public static class LFCShipFeatureRegistry
{
    private static readonly Dictionary<ShipFeatureType, HashSet<string>> lockRegistry = [];

    private static HangarShipDoor shipDoor;
    private static StartMatchLever shipLever;
    private static Terminal shipTerminal;
    private static ItemCharger itemCharger;
    private static TVScript shipTV;
    private static List<ShipTeleporter> shipTeleporters = [];

    public enum ShipFeatureType
    {
        SHIP_LIGHTS,
        MAP_SCREEN,
        SHIP_DOORS,
        SHIP_LEVER,
        SHIP_TERMINAL,
        ITEM_CHARGER,
        SHIP_TV,
        SHIP_TELEPORTERS
    }

    public static void AddLock(ShipFeatureType featureType, string tag)
    {
        if (!lockRegistry.TryGetValue(featureType, out HashSet<string> tagSet))
        {
            tagSet = [];
            lockRegistry[featureType] = tagSet;
        }

        if (tagSet.Add(tag))
        {
            // Si c'est le premier tag -> on désactive réellement
            if (tagSet.Count == 1)
                SetFeatureEnabled(featureType, false);
        }
    }

    public static void RemoveLock(ShipFeatureType featureType, string tag)
    {
        if (!lockRegistry.TryGetValue(featureType, out HashSet<string> tagSet)) return;

        if (tagSet.Remove(tag))
        {
            // Si plus aucun tag -> on réactive l’action
            if (tagSet.Count == 0)
                SetFeatureEnabled(featureType, true);
        }
    }

    public static bool IsLocked(ShipFeatureType featureType) => lockRegistry.TryGetValue(featureType, out HashSet<string> tagSet) && tagSet.Count > 0;

    private static void SetFeatureEnabled(ShipFeatureType featureType, bool enabled)
    {
        switch (featureType)
        {
            case ShipFeatureType.SHIP_LIGHTS: SetShipLights(enabled); break;
            case ShipFeatureType.MAP_SCREEN: SetMapScreen(enabled); break;
            case ShipFeatureType.SHIP_DOORS: SetShipDoors(enabled); break;
            case ShipFeatureType.SHIP_LEVER: SetShipLever(enabled); break;
            case ShipFeatureType.SHIP_TERMINAL: SetShipTerminal(enabled); break;
            case ShipFeatureType.ITEM_CHARGER: SetItemCharger(enabled); break;
            case ShipFeatureType.SHIP_TV: SetShipTV(enabled); break;
            case ShipFeatureType.SHIP_TELEPORTERS: SetShipTeleporters(enabled); break;

            default:
                LegaFusionCore.mls.LogWarning($"[ShipFeatureRegistry] Unknown feature '{featureType}'");
                break;
        }
    }

    private static void SetShipLights(bool enabled)
    {
        ShipLights shipLights = StartOfRound.Instance.shipRoomLights;
        shipLights.areLightsOn = enabled;
        shipLights.shipLightsAnimator.SetBool("lightsOn", shipLights.areLightsOn);
    }

    private static void SetMapScreen(bool enabled) => StartOfRound.Instance.mapScreen.SwitchScreenOn(enabled);

    private static void SetShipDoors(bool enabled)
    {
        shipDoor ??= Object.FindObjectOfType<HangarShipDoor>();
        if (shipDoor == null) return;

        shipDoor.hydraulicsScreenDisplayed = enabled;
        shipDoor.hydraulicsDisplay.SetActive(enabled);
        shipDoor.SetDoorButtonsEnabled(enabled);
        if (!enabled)
        {
            StartOfRound.Instance.shipDoorsAnimator.SetBool("Closed", value: enabled);
            shipDoor.SetDoorOpen();
        }
    }

    private static void SetShipLever(bool enabled)
    {
        shipLever ??= Object.FindObjectOfType<StartMatchLever>();
        if (shipLever == null) return;

        shipLever.triggerScript.disabledHoverTip = enabled ? Constants.MESSAGE_DEFAULT_SHIP_LEVER : Constants.MESSAGE_NO_SHIP_ENERGY;
        shipLever.triggerScript.interactable = enabled;
    }

    private static void SetShipTerminal(bool enabled)
    {
        shipTerminal ??= Object.FindObjectOfType<Terminal>();
        if (shipTerminal == null) return;

        if (!enabled) shipTerminal.terminalTrigger.disabledHoverTip = Constants.MESSAGE_NO_SHIP_ENERGY;
        shipTerminal.terminalTrigger.interactable = enabled;
    }

    private static void SetItemCharger(bool enabled)
    {
        itemCharger ??= Object.FindObjectOfType<ItemCharger>();
        if (itemCharger == null) return;

        itemCharger.triggerScript.disabledHoverTip = enabled ? Constants.MESSAGE_DEFAULT_ITEM_CHARGER : Constants.MESSAGE_NO_SHIP_ENERGY;
        itemCharger.triggerScript.interactable = enabled;
    }

    private static void SetShipTV(bool enabled)
    {
        shipTV ??= Object.FindObjectOfType<TVScript>();
        if (shipTV == null) return;

        shipTV.TurnTVOnOff(enabled);
    }

    private static void SetShipTeleporters(bool enabled)
    {
        _ = shipTeleporters.RemoveAll(t => t == null);

        if (shipTeleporters.Count == 0)
            shipTeleporters = Object.FindObjectsOfType<ShipTeleporter>().ToList();

        foreach (ShipTeleporter shipTeleporter in shipTeleporters)
            shipTeleporter.buttonTrigger.interactable = enabled;
    }
}

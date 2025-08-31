using LegaFusionCore.CustomInputs;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace LegaFusionCore.Behaviours.Addons;

public abstract class AddonComponent : MonoBehaviour
{
    public GrabbableObject grabbableObject;
    public string addonName;
    public bool isPassive;

    public bool onCooldown = false;

    public virtual void ActivateAddonAbility() { }

    public void StartCooldown(int cooldown)
    {
        if (onCooldown) return;

        onCooldown = true;
        _ = StartCoroutine(StartCooldownCoroutine(cooldown));
    }

    public IEnumerator StartCooldownCoroutine(int cooldown)
    {
        while (cooldown > 0)
        {
            yield return new WaitForSecondsRealtime(1f);
            if (grabbableObject == null || !grabbableObject.isHeld) continue;

            cooldown--;
            SetCooldownTipsForItem(cooldown);
        }
        onCooldown = false;
    }

    public void SetCooldownTipsForItem(int timeLeft)
    {
        string toolTip = timeLeft > 0 ? $"[On cooldown: {timeLeft}]" : "";
        SetTipsForItem(isPassive ? [toolTip] : [AddonInput.Instance.GetAddonToolTip(), toolTip]);
    }

    public void SetTipsForItem(string[] toolTips)
    {
        if (grabbableObject?.playerHeldBy == null || grabbableObject.isPocketed || grabbableObject.playerHeldBy != GameNetworkManager.Instance.localPlayerController) return;
        HUDManager.Instance.ChangeControlTipMultiple(grabbableObject.itemProperties.toolTips.Concat(toolTips).ToArray(), holdingItem: true, grabbableObject.itemProperties);
    }

    public void RemoveAddon()
    {
        ScanNodeProperties scanNode = grabbableObject?.gameObject.GetComponentInChildren<ScanNodeProperties>();
        if (scanNode == null) return;

        string[] textsToRemove = ["\nAddon: " + addonName, "Addon: " + addonName];
        foreach (string textToRemove in textsToRemove)
        {
            int index = scanNode.subText.IndexOf(textToRemove);
            if (index >= 0)
            {
                scanNode.subText = scanNode.subText.Remove(index, textToRemove.Length);
                break;
            }
        }

        Destroy(gameObject);
    }
}

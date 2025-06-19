using System.Collections;
using System.Linq;
using UnityEngine;

namespace LegaFusionCore.Behaviours.Addons;

public abstract class AddonComponent : MonoBehaviour
{
    public bool hasAddon = false;
    public string addonName;
    public string toolTip;

    public bool onCooldown = false;

    public virtual void ActivateSpecialAbility() { }

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

            cooldown--;
            SetCooldownTipsForItem(cooldown);
        }
        onCooldown = false;
    }

    public void SetCooldownTipsForItem(int timeLeft)
    {
        GrabbableObject grabbableObject = GetComponentInParent<GrabbableObject>();
        if (grabbableObject == null || grabbableObject.playerHeldBy == null || grabbableObject.isPocketed || grabbableObject.playerHeldBy != GameNetworkManager.Instance.localPlayerController) return;

        string toolTip = timeLeft > 0 ? $"[On cooldown: {timeLeft}]" : "";
        HUDManager.Instance.ChangeControlTipMultiple(grabbableObject.itemProperties.toolTips.Concat([toolTip]).ToArray(), holdingItem: true, grabbableObject.itemProperties);
    }

    public void RemoveAddon()
    {
        GrabbableObject grabbableObject = GetComponentInParent<GrabbableObject>();
        if (grabbableObject == null) return;

        ScanNodeProperties scanNode = grabbableObject.gameObject.GetComponentInChildren<ScanNodeProperties>();
        if (scanNode == null) return;

        string[] textsToRemove = { "\nAddon: " + addonName, "Addon: " + addonName };

        foreach (string textToRemove in textsToRemove)
        {
            int index = scanNode.subText.IndexOf(textToRemove);
            if (index >= 0)
            {
                scanNode.subText = scanNode.subText.Remove(index, textToRemove.Length);
                break;
            }
        }

        hasAddon = false;
        addonName = null;
    }
}

using GameNetcodeStuff;
using LegaFusionCore.Behaviours.Addons;
using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using System.Linq;
using UnityEngine.InputSystem;

namespace LegaFusionCore.CustomInputs;

public class AddonInput : LcInputActions
{
    private static AddonInput instance;

    public static AddonInput Instance
    {
        get
        {
            instance ??= new AddonInput();
            return instance;
        }
        private set
            => instance = value;
    }

    [InputAction(KeyboardControl.Y, GamepadControl = GamepadControl.ButtonNorth, Name = "Addon Ability")]
    public InputAction AddonKey { get; set; }

    public void EnableInput()
        => AddonKey.performed += ActivateAddonAbility;

    public void DisableInput()
        => AddonKey.performed -= ActivateAddonAbility;

    public void ActivateAddonAbility(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
        if (player == null || player.currentlyHeldObjectServer == null) return;

        AddonComponent addon = player.currentlyHeldObjectServer.GetComponent<AddonComponent>();
        if (addon == null || addon.isPassive) return;

        addon.ActivateAddonAbility();
    }

    public string GetAddonToolTip()
        => $"Addon Ability : [{AddonKey.GetBindingDisplayString().First()}]";
}

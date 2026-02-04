using HarmonyLib;
using LegaFusionCore.Registries;

namespace LegaFusionCore.Patches;

public class VehicleControllerPatch
{
    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.Update))]
    [HarmonyPostfix]
    private static void UpdateVehicle(ref VehicleController __instance)
    {
        if (LFCVisibilityRegistry.IsHidden(__instance.gameObject))
        {
            __instance.driverSideDoorTrigger.interactable = false;
            __instance.passengerSideDoorTrigger.interactable = false;
        }
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.EnableVehicleCollisionForAllPlayers))]
    [HarmonyPrefix]
    private static bool EnableVehicleCollisionForAllPlayers(ref VehicleController __instance)
        => !LFCVisibilityRegistry.IsHidden(__instance.gameObject);

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.SetVehicleCollisionForPlayer))]
    [HarmonyPrefix]
    private static bool SetVehicleCollisionForPlayer(ref VehicleController __instance, bool setEnabled)
        => !setEnabled || !LFCVisibilityRegistry.IsHidden(__instance.gameObject);
}

using HarmonyLib;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using static LegaFusionCore.Registries.LFCShipFeatureRegistry;

namespace LegaFusionCore.ModsCompat;

public static class SelfSortingStorageSoftCompat
{
    private static Type LightButtonType;
    private static MethodInfo SwitchLightsMethod;

    public static void Patch(Harmony harmony)
    {
        Type effectsType = Type.GetType("SelfSortingStorage.Utils.Effects, SelfSortingStorage");
        if (effectsType != null)
        {
            MethodInfo isTriggerValid = AccessTools.Method(effectsType, "IsTriggerValid");
            if (isTriggerValid != null)
            {
                HarmonyMethod prefix = new HarmonyMethod(AccessTools.Method(typeof(SelfSortingStorageSoftCompat), nameof(IsTriggerValid)));
                _ = harmony.Patch(isTriggerValid, prefix: prefix);
            }
        }

        LightButtonType = Type.GetType("SelfSortingStorage.Buttons.LightButton, SelfSortingStorage");
        if (LightButtonType != null)
        {
            SwitchLightsMethod = AccessTools.Method(LightButtonType, "SwitchLights");
            if (SwitchLightsMethod != null)
            {
                HarmonyMethod prefix = new HarmonyMethod(AccessTools.Method(typeof(SelfSortingStorageSoftCompat), nameof(PreSwitchLights)));
                _ = harmony.Patch(SwitchLightsMethod, prefix: prefix);
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static bool IsTriggerValid(out string notValidText)
    {
        if (IsLocked(ShipFeatureType.SMART_CUPBOARD))
        {
            notValidText = Constants.MESSAGE_NO_SHIP_ENERGY;
            return false;
        }
        notValidText = "[Nothing to store]";
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool PreSwitchLights() => !IsLocked(ShipFeatureType.SMART_CUPBOARD);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void SwitchLights(bool on)
    {
        if (LightButtonType != null)
        {
            UnityEngine.Object lightButton = UnityEngine.Object.FindObjectOfType(LightButtonType);
            if (SwitchLightsMethod != null && lightButton != null)
                _ = SwitchLightsMethod.Invoke(lightButton, [on]);
        }
    }
}


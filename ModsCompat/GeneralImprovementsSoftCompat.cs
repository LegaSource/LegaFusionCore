using HarmonyLib;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using static LegaFusionCore.Registries.LFCShipFeatureRegistry;

namespace LegaFusionCore.ModsCompat;

public static class GeneralImprovementsSoftCompat
{
    public static void Patch(Harmony harmony)
    {
        Type manualCameraRendererType = Type.GetType("GeneralImprovements.Patches.ManualCameraRendererPatch, GeneralImprovements");
        if (manualCameraRendererType != null)
        {
            MethodInfo switchScreenOn = AccessTools.Method(manualCameraRendererType, "SwitchScreenOn");
            if (switchScreenOn != null)
            {
                HarmonyMethod prefix = new HarmonyMethod(AccessTools.Method(typeof(GeneralImprovementsSoftCompat), nameof(SwitchScreenOn)));
                _ = harmony.Patch(switchScreenOn, prefix: prefix);
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static bool SwitchScreenOn() => !IsLocked(ShipFeatureType.MAP_SCREEN);
}

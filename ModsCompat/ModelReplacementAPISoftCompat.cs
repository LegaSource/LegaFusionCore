using GameNetcodeStuff;
using HarmonyLib;
using LegaFusionCore.Registries;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LegaFusionCore.ModsCompat;

public static class ModelReplacementAPISoftCompat
{
    private static FieldInfo fiController;
    private static Type viewStateType;

    public static void Patch(Harmony harmony)
    {
        Type viewStateManagerType = Type.GetType("ModelReplacement.ViewStateManager, ModelReplacementAPI");
        if (viewStateManagerType != null)
        {
            MethodInfo setPlayerRenderers = AccessTools.Method(viewStateManagerType, "SetPlayerRenderers");
            if (setPlayerRenderers != null)
            {
                HarmonyMethod prefix = new HarmonyMethod(AccessTools.Method(typeof(ModelReplacementAPISoftCompat), nameof(SetPlayerRenderers)));
                _ = harmony.Patch(setPlayerRenderers, prefix: prefix);
            }

            MethodInfo getViewState = AccessTools.Method(viewStateManagerType, "GetViewState");
            if (getViewState != null)
            {
                HarmonyMethod postfix = new HarmonyMethod(AccessTools.Method(typeof(ModelReplacementAPISoftCompat), nameof(GetViewState)));
                _ = harmony.Patch(getViewState, postfix: postfix);
                viewStateType = Type.GetType("ModelReplacement.ViewState, ModelReplacementAPI");
            }

            Type managerBaseType = Type.GetType("ModelReplacement.Monobehaviors.ManagerBase, ModelReplacementAPI");
            if (managerBaseType != null)
                fiController = AccessTools.Field(managerBaseType, "controller");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void SetPlayerRenderers(object __instance, ref bool enabled, ref bool helmetShadow)
    {
        PlayerControllerB player = (PlayerControllerB)(fiController?.GetValue(__instance));
        if (player != null && LFCVisibilityRegistry.IsHidden(player.gameObject))
        {
            if (enabled) enabled = false;
            if (helmetShadow) helmetShadow = false;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void GetViewState(object __instance, ref object __result)
    {
        if (viewStateType != null)
        {
            PlayerControllerB player = (PlayerControllerB)(fiController?.GetValue(__instance));
            if (player != null && LFCVisibilityRegistry.IsHidden(player.gameObject))
                __result = Enum.Parse(viewStateType, "None");
        }
    }
}

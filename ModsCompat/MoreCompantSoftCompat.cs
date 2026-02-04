using GameNetcodeStuff;
using HarmonyLib;
using LegaFusionCore.Registries;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LegaFusionCore.ModsCompat;

public static class MoreCompantSoftCompat
{
    public static void Patch(Harmony harmony)
    {
        Type cosmeticApplicationType = Type.GetType("MoreCompany.Cosmetics.CosmeticApplication, MoreCompany");
        if (cosmeticApplicationType != null)
        {
            MethodInfo updateAllCosmeticVisibilities = AccessTools.Method(cosmeticApplicationType, "UpdateAllCosmeticVisibilities");
            if (updateAllCosmeticVisibilities != null)
            {
                HarmonyMethod prefix = new HarmonyMethod(AccessTools.Method(typeof(MoreCompantSoftCompat), nameof(UpdateAllCosmeticVisibilities)));
                _ = harmony.Patch(updateAllCosmeticVisibilities, prefix: prefix);
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool UpdateAllCosmeticVisibilities(object __instance)
    {
        if (__instance is MonoBehaviour behaviour)
        {
            GameObject owner =
                behaviour.GetComponentInParent<PlayerControllerB>()?.gameObject
                ?? behaviour.GetComponentInParent<EnemyAI>()?.gameObject
                ?? behaviour.transform.root.gameObject;

            return !LFCVisibilityRegistry.IsHidden(owner.gameObject);
        }
        return true;
    }
}

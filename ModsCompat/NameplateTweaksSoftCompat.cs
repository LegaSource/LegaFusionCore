using GameNetcodeStuff;
using HarmonyLib;
using LegaFusionCore.Registries;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LegaFusionCore.ModsCompat;

public static class NameplateTweaksSoftCompat
{
    public static void Patch(Harmony harmony)
    {
        Type patchesType = Type.GetType("NameplateTweaks.Patches, NameplateTweaks");
        if (patchesType != null)
        {
            MethodInfo playerUpdate = AccessTools.Method(patchesType, "PlayerUpdate");
            if (playerUpdate != null)
            {
                HarmonyMethod postfix = new HarmonyMethod(AccessTools.Method(typeof(NameplateTweaksSoftCompat), nameof(PlayerUpdate)));
                _ = harmony.Patch(playerUpdate, postfix: postfix);
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void PlayerUpdate(object __0)
    {
        PlayerControllerB player = __0 as PlayerControllerB;
        if (LFCVisibilityRegistry.IsHidden(player.gameObject))
        {
            player.usernameAlpha.alpha = 0f;
            player.usernameBillboard.localScale = Vector3.zero;
            player.usernameCanvas?.gameObject.SetActive(false);
        }
    }
}

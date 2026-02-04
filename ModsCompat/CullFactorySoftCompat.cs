using BepInEx.Bootstrap;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LegaFusionCore.ModsCompat;

public static class CullFactorySoftCompat
{
    private static readonly bool CullFactoryAvailable;
    private static readonly MethodInfo RefreshGrabbableMethod;
    private static readonly MethodInfo RefreshLightMethod;

    static CullFactorySoftCompat()
    {
        if (Chainloader.PluginInfos.TryGetValue("com.fumiko.CullFactory", out BepInEx.PluginInfo info) && info.Metadata.Version >= new Version(1, 5, 0))
        {
            Type apiType = Type.GetType("CullFactory.Behaviours.API.DynamicObjectsAPI, CullFactory");
            if (apiType != null)
            {
                RefreshGrabbableMethod = apiType.GetMethod("RefreshGrabbableObjectPosition", BindingFlags.Public | BindingFlags.Static);
                RefreshLightMethod = apiType.GetMethod("RefreshLightPosition", BindingFlags.Public | BindingFlags.Static);
                CullFactoryAvailable = RefreshGrabbableMethod != null && RefreshLightMethod != null;
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void RefreshGrabbableObjectPosition(GrabbableObject item)
    {
        item.EnableItemMeshes(true);
        if (CullFactoryAvailable)
            _ = (RefreshGrabbableMethod?.Invoke(null, [item]));
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void RefreshLightPosition(Light light)
    {
        if (CullFactoryAvailable)
            _ = (RefreshLightMethod?.Invoke(null, [light]));
    }
}

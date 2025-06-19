using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LegaFusionCore.Patches;
using System;
using System.Reflection;
using UnityEngine;

namespace LegaFusionCore;

[BepInPlugin(modGUID, modName, modVersion)]
public class LegaFusionCore : BaseUnityPlugin
{
    private const string modGUID = "Lega.LegaFusionCore";
    private const string modName = "Lega Fusion Core";
    private const string modVersion = "1.0.0";

    private readonly Harmony harmony = new Harmony(modGUID);
    internal static ManualLogSource mls;

    public void Awake()
    {
        mls = BepInEx.Logging.Logger.CreateLogSource("LegaFusionCore");
        NetcodePatcher();

        harmony.PatchAll(typeof(GrabbableObjectPatch));
    }

    private static void NetcodePatcher()
    {
        Type[] types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (Type type in types)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (MethodInfo method in methods)
            {
                object[] attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                if (attributes.Length == 0 || method.ContainsGenericParameters) continue;

                _ = method.Invoke(null, null);
            }
        }
    }
}

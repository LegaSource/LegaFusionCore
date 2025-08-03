using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LegaFusionCore.Managers.NetworkManagers;
using LegaFusionCore.Patches;
using LegaFusionCore.Registries;
using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace LegaFusionCore;

[BepInPlugin(modGUID, modName, modVersion)]
public class LegaFusionCore : BaseUnityPlugin
{
    public const string modGUID = "Lega.LegaFusionCore";
    public const string modName = "Lega Fusion Core";
    public const string modVersion = "1.0.1";

    private readonly Harmony harmony = new Harmony(modGUID);
    private static readonly AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "legafusioncore"));
    internal static ManualLogSource mls;
    public static GameObject managerPrefab = NetworkPrefabs.CreateNetworkPrefab("LFCNetworkManager");

    // Shaders
    public static Material wallhackShader;
    public static Material transparentShader;

    // Particles
    public static GameObject smokeParticle;
    public static GameObject bluePortalParticle;
    public static GameObject redPortalParticle;

    public void Awake()
    {
        mls = BepInEx.Logging.Logger.CreateLogSource("LegaFusionCore");
        LoadManager();
        NetcodePatcher();

        LoadShaders();
        LoadParticles();

        harmony.PatchAll(typeof(StartOfRoundPatch));
        harmony.PatchAll(typeof(NetworkBehaviourPatch));
        harmony.PatchAll(typeof(PlayerControllerBPatch));
        harmony.PatchAll(typeof(GrabbableObjectPatch));
    }

    public static void LoadManager()
    {
        LethalLib.Modules.Utilities.FixMixerGroups(managerPrefab);
        _ = managerPrefab.AddComponent<LFCNetworkManager>();
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

    public static void LoadShaders()
    {
        wallhackShader = bundle.LoadAsset<Material>("Assets/Shaders/wallhackMaterial.mat");
        transparentShader = bundle.LoadAsset<Material>("Assets/Shaders/transparentMaterial.mat");
    }

    public void LoadParticles()
    {
        HashSet<GameObject> gameObjects =
        [
            (smokeParticle = bundle.LoadAsset<GameObject>("Assets/Particles/SmokeParticle.prefab")),
            (bluePortalParticle = bundle.LoadAsset<GameObject>("Assets/Particles/BluePortalParticle.prefab")),
            (redPortalParticle = bundle.LoadAsset<GameObject>("Assets/Particles/RedPortalParticle.prefab"))
        ];

        foreach (GameObject gameObject in gameObjects)
        {
            NetworkPrefabs.RegisterNetworkPrefab(gameObject);
            LethalLib.Modules.Utilities.FixMixerGroups(gameObject);
            LFCPrefabRegistry.RegisterPrefab($"{modName}{gameObject.name}", gameObject);
        }
    }
}

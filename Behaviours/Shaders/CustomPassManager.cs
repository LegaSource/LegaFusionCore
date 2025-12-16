using GameNetcodeStuff;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace LegaFusionCore.Behaviours.Shaders;

public class CustomPassManager : MonoBehaviour
{
    public static ShaderCustomPass shaderCustomPass;
    public static CustomPassVolume customPassVolume;

    public static CustomPassVolume CustomPassVolume
    {
        get
        {
            if (customPassVolume == null)
            {
                customPassVolume = GameNetworkManager.Instance.localPlayerController.gameplayCamera.gameObject.AddComponent<CustomPassVolume>();
                customPassVolume.targetCamera = GameNetworkManager.Instance.localPlayerController.gameplayCamera;
                customPassVolume.injectionPoint = (CustomPassInjectionPoint)1;
                customPassVolume.isGlobal = true;

                shaderCustomPass = new ShaderCustomPass();
                customPassVolume.customPasses.Add(shaderCustomPass);
            }
            return customPassVolume;
        }
    }

    private static Renderer[] GetFilteredRenderersFromObject(GameObject gObject)
    {
        Renderer[] renderers = gObject.GetComponentsInChildren<Renderer>().Where(r => r.shadowCastingMode != ShadowCastingMode.ShadowsOnly).ToArray();
        if (renderers.Length == 0) return [];

        if (gObject.TryGetComponent<EnemyAI>(out _) || gObject.TryGetComponent<PlayerControllerB>(out _))
        {
            renderers = renderers.Where(r => r is SkinnedMeshRenderer).ToArray();
            renderers = GetFilteredSpecificRenderers(gObject, renderers);
        }

        if (renderers.Length == 0)
        {
            LegaFusionCore.mls.LogError($"No renderer could be found on {gObject.name}.");
            return [];
        }

        return renderers;
    }

    private static Renderer[] GetFilteredSpecificRenderers(GameObject gObject, Renderer[] renderers)
    {
        Renderer[] filteredRenderers = renderers;

        if (gObject.name.Contains(Constants.MOUTH_DOG_PREFAB, StringComparison.OrdinalIgnoreCase))
        {
            filteredRenderers = renderers
                .Where(r => !r.name.Contains("LOD1", StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        return filteredRenderers;
    }

    public static void SetupAuraForObject(GameObject gObject, Material material, string tag, Color color = default)
    {
        Renderer[] renderers = GetFilteredRenderersFromObject(gObject);
        if (renderers.Length > 0) SetupCustomPass(renderers, material, tag, color, gObject);
    }

    public static void SetupAuraForObjects(GameObject[] gObjects, Material material, string tag, Color color = default)
    {
        foreach (GameObject gObject in gObjects)
            SetupAuraForObject(gObject, material, tag, color);
    }

    public static void SetupCustomPass(Renderer[] renderers, Material material, string tag, Color color, GameObject gObject)
    {
        if (CustomPassVolume == null)
        {
            LegaFusionCore.mls.LogError("CustomPassVolume is not assigned.");
            return;
        }

        shaderCustomPass ??= CustomPassVolume.customPasses.Find(pass => pass is ShaderCustomPass) as ShaderCustomPass;
        if (shaderCustomPass == null)
        {
            LegaFusionCore.mls.LogError("ShaderCustomPass could not be found in CustomPassVolume.");
            return;
        }

        shaderCustomPass.AddRenderers(renderers, material, tag, color, gObject);
    }

    public static void RemoveAuraFromObject(GameObject gObject, string tag = "default")
    {
        Renderer[] renderers = GetFilteredRenderersFromObject(gObject);
        if (renderers.Length > 0) shaderCustomPass?.RemoveRenderers(renderers, tag);
    }

    public static void RemoveAuraFromObjects(GameObject[] gObjects, string tag = "default")
    {
        foreach (GameObject gObject in gObjects)
            RemoveAuraFromObject(gObject, tag);
    }

    public static void RemoveAuraByTag(string tag) => shaderCustomPass?.RemoveRenderersByTag(tag);

    public static void ClearAllAuras() => shaderCustomPass?.ClearAll();
}
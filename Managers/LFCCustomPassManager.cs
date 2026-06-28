using GameNetcodeStuff;
using LegaFusionCore.Behaviours;
using LegaFusionCore.Utilities;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace LegaFusionCore.Managers;

public class LFCCustomPassManager : MonoBehaviour
{
    private static CustomPassVolume customPassVolume;
    private static LFCCustomPassShader shaderCustomPass;

    public static CustomPassVolume CustomPassVolume
    {
        get
        {
            if (customPassVolume == null && LFCUtilities.LocalPlayer != null)
            {
                customPassVolume = LFCUtilities.LocalPlayer.gameplayCamera.gameObject.AddComponent<CustomPassVolume>();
                customPassVolume.targetCamera = LFCUtilities.LocalPlayer.gameplayCamera;
                customPassVolume.injectionPoint = (CustomPassInjectionPoint)1;
                customPassVolume.isGlobal = true;

                shaderCustomPass = new LFCCustomPassShader();
                customPassVolume.customPasses.Add(shaderCustomPass);
            }
            return customPassVolume;
        }
    }

    public static LFCCustomPassShader LFCCustomPassShader
    {
        get
        {
            if (CustomPassVolume == null)
            {
                LegaFusionCore.mls.LogError("CustomPassVolume is not assigned.");
                return null;
            }

            shaderCustomPass ??= CustomPassVolume.customPasses.Find(pass => pass is LFCCustomPassShader) as LFCCustomPassShader;
            if (shaderCustomPass == null)
                LegaFusionCore.mls.LogError("ShaderCustomPass could not be found in CustomPassVolume.");

            return shaderCustomPass;
        }
    }

    // ------------------------------------------------------------------------------------
    //                                      AURAS
    // ------------------------------------------------------------------------------------

    public static void SetupAuraForObject(GameObject gObject, Material material, string tag, Color color = default)
    {
        Renderer[] renderers = GetFilteredRenderers(gObject);
        if (renderers.Length > 0)
            LFCCustomPassShader?.AddRenderers(renderers, material, tag, color, gObject);
    }

    public static void SetupAuraForObjects(GameObject[] gObjects, Material material, string tag, Color color = default)
    {
        foreach (GameObject obj in gObjects)
            SetupAuraForObject(obj, material, tag, color);
    }

    public static void RemoveAuraFromObjects(GameObject[] gObjects, string tag = "default")
    {
        foreach (GameObject gObject in gObjects)
            RemoveAuraFromObject(gObject, tag);
    }

    public static void RemoveAuraFromObject(GameObject gObject, string tag = "default")
    {
        Renderer[] renderers = GetFilteredRenderers(gObject);
        if (renderers.Length > 0)
            LFCCustomPassShader?.RemoveRenderers(renderers, tag);
    }

    public static void RemoveAuraByTag(string tag) => LFCCustomPassShader?.RemoveRenderersByTag(tag);

    public static void ClearAllAuras() => LFCCustomPassShader?.ClearAllRenderers();

    // ------------------------------------------------------------------------------------
    //                                  SCREEN FILTERS
    // ------------------------------------------------------------------------------------

    public static void SetupScreenFilter(Material material, string tag)
    {
        if (material != null)
            LFCCustomPassShader?.AddFilter(material, tag);
    }

    public static void RemoveFiltersByTag(string tag) => LFCCustomPassShader?.RemoveFiltersByTag(tag);

    public static void RemoveFiltersByMaterial(Material material) => LFCCustomPassShader?.RemoveFiltersByMaterial(material);

    public static void ClearAllFilters() => LFCCustomPassShader?.ClearAllFilters();

    // ------------------------------------------------------------------------------------
    //                                  INTERNAL
    // ------------------------------------------------------------------------------------

    private static Renderer[] GetFilteredRenderers(GameObject gObject)
    {
        Renderer[] renderers = gObject.GetComponentsInChildren<Renderer>().Where(r => r != null && r.shadowCastingMode != ShadowCastingMode.ShadowsOnly).ToArray();
        if (renderers.Length == 0) return [];

        if (gObject.TryGetComponent<EnemyAI>(out _) || gObject.TryGetComponent<PlayerControllerB>(out _))
        {
            renderers = renderers.OfType<SkinnedMeshRenderer>().ToArray();
            renderers = FilterSpecialCases(gObject, renderers);
        }

        if (renderers.Length == 0)
        {
            LegaFusionCore.mls.LogError($"No renderer found on {gObject.name}");
            return [];
        }

        return renderers;
    }

    private static Renderer[] FilterSpecialCases(GameObject gObject, Renderer[] renderers)
        => gObject.name.Contains(Constants.MOUTH_DOG_PREFAB, StringComparison.OrdinalIgnoreCase)
            ? renderers.Where(r => !r.name.Contains("LOD1", StringComparison.OrdinalIgnoreCase)).ToArray()
            : renderers;
}
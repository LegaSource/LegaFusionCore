using GameNetcodeStuff;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

    public static void SetupAuraForObjects(GameObject[] objects, Material material, string tag, Color color = default)
    {
        Renderer[] renderers = GetFilteredRenderersFromObjects(objects);
        if (renderers.Length > 0) SetupCustomPass(renderers, material, tag, color);
    }

    public static void SetupCustomPass(Renderer[] renderers, Material material, string tag, Color color)
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

        shaderCustomPass.AddRenderers(renderers, material, tag, color);
    }

    public static void RemoveAuraFromObjects(GameObject[] objects, string tag = "default")
    {
        Renderer[] renderers = GetFilteredRenderersFromObjects(objects);
        if (renderers.Length > 0) shaderCustomPass?.RemoveRenderers(renderers, tag);
    }

    public static void RemoveAuraByTag(string tag) => shaderCustomPass?.RemoveRenderersByTag(tag);

    public static void ClearAllAuras() => shaderCustomPass?.ClearAll();

    private static Renderer[] GetFilteredRenderersFromObjects(GameObject[] objects)
    {
        List<Renderer> collectedRenderers = [];

        foreach (GameObject obj in objects)
        {
            if (obj == null) continue;

            List<Renderer> renderers = obj.GetComponentsInChildren<Renderer>().ToList();
            if (renderers.Count == 0) continue;

            if (obj.TryGetComponent<EnemyAI>(out _) || obj.TryGetComponent<PlayerControllerB>(out _))
            {
                renderers = renderers.Where(r => r is SkinnedMeshRenderer).ToList();
            }

            if (renderers.Count == 0)
            {
                LegaFusionCore.mls.LogError($"No renderer could be found on {obj.name}.");
                continue;
            }

            collectedRenderers.AddRange(renderers);
        }

        return collectedRenderers.ToArray();
    }
}
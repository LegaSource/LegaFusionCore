using LegaFusionCore.Registries;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace LegaFusionCore.Behaviours;

public class LFCCustomPassShader : CustomPass
{
    private readonly Dictionary<string, Dictionary<Renderer, (Material, Color, GameObject)>> TargetRenderers = [];
    private readonly Dictionary<string, Material> ScreenFilters = [];

    // ------------------------------------------------------------------------------------
    //                                  RENDERERS
    // ------------------------------------------------------------------------------------

    public void AddRenderers(Renderer[] renderers, Material material, string tag, Color color, GameObject source)
    {
        tag ??= "default";
        if (!TargetRenderers.TryGetValue(tag, out Dictionary<Renderer, (Material, Color, GameObject)> dict))
        {
            dict = [];
            TargetRenderers[tag] = dict;
        }
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
                dict[renderer] = (material, color, source);
        }
    }

    public void RemoveRenderers(Renderer[] renderers, string tag)
    {
        if (!string.IsNullOrEmpty(tag) && TargetRenderers.TryGetValue(tag, out Dictionary<Renderer, (Material, Color, GameObject)> dict))
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null && dict.Remove(renderer))
                    renderer.SetPropertyBlock(null);
            }
            if (dict.Count == 0)
                _ = TargetRenderers.Remove(tag);
        }
    }

    public void RemoveRenderersByTag(string tag)
    {
        if (TargetRenderers.TryGetValue(tag, out Dictionary<Renderer, (Material, Color, GameObject)> dict))
        {
            foreach (Renderer renderer in dict.Keys)
                renderer?.SetPropertyBlock(null);

            _ = TargetRenderers.Remove(tag);
        }
    }

    public void ClearAllRenderers() => TargetRenderers.Clear();

    // ------------------------------------------------------------------------------------
    //                                  FILTERS
    // ------------------------------------------------------------------------------------

    public void AddFilter(Material material, string tag)
    {
        tag ??= "default";
        ScreenFilters[tag] = material;
    }

    public void RemoveFiltersByTag(string tag)
    {
        if (!string.IsNullOrEmpty(tag))
            _ = ScreenFilters.Remove(tag);
    }

    public void RemoveFiltersByMaterial(Material material)
    {
        List<string> keysToRemove = [];
        foreach (KeyValuePair<string, Material> kvp in ScreenFilters)
        {
            if (kvp.Value == material)
                keysToRemove.Add(kvp.Key);
        }
        foreach (string key in keysToRemove)
            _ = ScreenFilters.Remove(key);
    }

    public void ClearAllFilters() => ScreenFilters.Clear();

    // ------------------------------------------------------------------------------------
    //                                  EXECUTE
    // ------------------------------------------------------------------------------------

    public override void Execute(CustomPassContext ctx)
    {
        DrawRenderers(ctx);
        DrawFilters(ctx);
    }

    private void DrawRenderers(CustomPassContext ctx)
    {
        if (TargetRenderers.Count == 0) return;

        HashSet<Renderer> drawn = [];
        foreach (KeyValuePair<string, Dictionary<Renderer, (Material, Color, GameObject)>> kvpTag in TargetRenderers)
        {
            Dictionary<Renderer, (Material, Color, GameObject)> dict = kvpTag.Value;
            List<Renderer> toRemove = [];

            foreach (KeyValuePair<Renderer, (Material, Color, GameObject)> kvp in dict)
            {
                Renderer renderer = kvp.Key;
                if (renderer == null)
                {
                    toRemove.Add(renderer);
                    continue;
                }

                (Material material, Color color, GameObject source) = kvp.Value;
                if (renderer.enabled && material != null && LFCShaderFilterRegistry.ShouldRender(source) && drawn.Add(renderer))
                {
                    if (material.HasProperty("_BaseColor"))
                    {
                        MaterialPropertyBlock block = new MaterialPropertyBlock();
                        block.SetColor("_BaseColor", new Color(color.r, color.g, color.b, material.GetColor("_BaseColor").a));
                        renderer.SetPropertyBlock(block);
                    }

                    for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                        ctx.cmd.DrawRenderer(renderer, material, i, 0);
                }
            }

            foreach (Renderer renderer in toRemove)
                _ = dict.Remove(renderer);
        }
    }

    private void DrawFilters(CustomPassContext ctx)
    {
        foreach (Material material in ScreenFilters.Values)
            CoreUtils.DrawFullScreen(ctx.cmd, material);
    }
}
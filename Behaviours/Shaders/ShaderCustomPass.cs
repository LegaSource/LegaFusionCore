using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace LegaFusionCore.Behaviours.Shaders;

public class ShaderCustomPass : CustomPass
{
    private readonly Dictionary<string, Dictionary<Renderer, (Material, Color)>> targetRenderers = [];

    public void AddRenderers(Renderer[] renderers, Material material, string tag, Color color)
    {
        if (string.IsNullOrEmpty(tag)) tag = "default";
        if (!targetRenderers.ContainsKey(tag)) targetRenderers[tag] = [];

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || targetRenderers[tag].ContainsKey(renderer)) continue;
            targetRenderers[tag].Add(renderer, (material, color));
        }
    }

    public void RemoveRenderers(Renderer[] renderers, string tag)
    {
        if (string.IsNullOrEmpty(tag) || !targetRenderers.ContainsKey(tag)) return;

        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && targetRenderers[tag].ContainsKey(renderer))
                _ = targetRenderers[tag].Remove(renderer);
        }

        if (targetRenderers[tag].Count == 0) _ = targetRenderers.Remove(tag);
    }

    public void RemoveRenderersByTag(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return;
        _ = targetRenderers.Remove(tag);
    }

    public void ClearAll()
        => targetRenderers.Clear();

    public override void Execute(CustomPassContext ctx)
    {
        if (targetRenderers.Count == 0) return;

        foreach (KeyValuePair<string, Dictionary<Renderer, (Material, Color)>> kvpTag in targetRenderers)
        {
            foreach (KeyValuePair<Renderer, (Material, Color)> kvpRenderer in kvpTag.Value)
            {
                Renderer renderer = kvpRenderer.Key;
                (Material material, Color color) = kvpRenderer.Value;

                if (renderer == null || material == null) continue;

                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetColor("_BaseColor", color);

                ctx.cmd.DrawRenderer(renderer, material, 0, 0);
            }
        }
    }
}
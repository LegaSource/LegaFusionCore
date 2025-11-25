using LegaFusionCore.Registries;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace LegaFusionCore.Behaviours.Shaders;

public class ShaderCustomPass : CustomPass
{
    private readonly Dictionary<string, Dictionary<Renderer, (Material, Color, GameObject sourceObject)>> targetRenderers = [];

    public void AddRenderers(Renderer[] renderers, Material material, string tag, Color color, GameObject sourceObject)
    {
        if (string.IsNullOrEmpty(tag)) tag = "default";
        if (!targetRenderers.ContainsKey(tag)) targetRenderers[tag] = [];

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || targetRenderers[tag].ContainsKey(renderer)) continue;
            targetRenderers[tag].Add(renderer, (material, color, sourceObject));
        }
    }

    public void RemoveRenderers(Renderer[] renderers, string tag)
    {
        if (string.IsNullOrEmpty(tag) || !targetRenderers.ContainsKey(tag)) return;

        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && targetRenderers[tag].ContainsKey(renderer))
            {
                renderer.SetPropertyBlock(null);
                _ = targetRenderers[tag].Remove(renderer);
            }
        }

        if (targetRenderers[tag].Count == 0) _ = targetRenderers.Remove(tag);
    }

    public void RemoveRenderersByTag(string tag)
    {
        if (string.IsNullOrEmpty(tag) || !targetRenderers.ContainsKey(tag)) return;

        foreach (KeyValuePair<Renderer, (Material, Color, GameObject sourceObject)> kvp in targetRenderers[tag].ToList())
        {
            if (kvp.Key == null) continue;
            kvp.Key.SetPropertyBlock(new MaterialPropertyBlock());
        }

        _ = targetRenderers.Remove(tag);
    }

    public void ClearAll() => targetRenderers.Clear();

    public override void Execute(CustomPassContext ctx)
    {
        if (targetRenderers.Count == 0) return;

        foreach (KeyValuePair<string, Dictionary<Renderer, (Material, Color, GameObject sourceObject)>> kvpTag in targetRenderers)
        {
            foreach (KeyValuePair<Renderer, (Material, Color, GameObject sourceObject)> kvpRenderer in kvpTag.Value)
            {
                Renderer renderer = kvpRenderer.Key;
                (Material material, Color color, GameObject sourceObject) = kvpRenderer.Value;
                if (renderer == null || !renderer.enabled || material == null || !LFCShaderFilterRegistry.ShouldRender(sourceObject)) continue;

                if (material.HasProperty("_BaseColor"))
                {
                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    // Utiliser la couleur envoyée en paramètre mais garder l'alpha original
                    block.SetColor("_BaseColor", new Color(color.r, color.g, color.b, material.GetColor("_BaseColor").a));
                    renderer.SetPropertyBlock(block);
                }

                int subMeshCount = renderer.sharedMaterials.Length;
                for (int i = 0; i < subMeshCount; i++)
                    ctx.cmd.DrawRenderer(renderer, material, i, 0);
            }
        }
    }
}
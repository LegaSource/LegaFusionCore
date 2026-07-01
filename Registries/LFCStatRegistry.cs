using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LegaFusionCore.Registries;

public static class LFCStatRegistry
{
    private class StatEntry
    {
        public readonly Dictionary<string, float> Modifiers = [];
        public float? minMultiplier;
        public float? maxMultiplier;

        public float Multiplier
        {
            get
            {
                float multiplier = 1f + Modifiers.Values.Sum();
                if (minMultiplier.HasValue) multiplier = Mathf.Max(minMultiplier.Value, multiplier);
                if (maxMultiplier.HasValue) multiplier = Mathf.Min(maxMultiplier.Value, multiplier);
                return multiplier;
            }
        }
    }

    private static readonly Dictionary<string, StatEntry> stats = [];

    public static void RegisterStat(string id, float? min = null, float? max = null)
    {
        if (!stats.ContainsKey(id))
            stats[id] = new StatEntry() { minMultiplier = min, maxMultiplier = max };
    }

    public static void AddModifier(string id, string tag, float value)
    {
        if (stats.TryGetValue(id, out StatEntry entry))
            entry.Modifiers[tag] = value;
    }

    public static IEnumerator AddModifierCoroutine(string id, string tag, float value, float duration)
    {
        AddModifier(id, tag, value);
        yield return new WaitForSeconds(duration);
        RemoveModifier(id, tag);
    }

    public static void RemoveModifier(string id, string tag)
    {
        if (stats.TryGetValue(id, out StatEntry entry))
            _ = entry.Modifiers.Remove(tag);
    }

    public static bool HasModifier(string id, string tag) => stats.TryGetValue(id, out StatEntry entry) && entry.Modifiers.ContainsKey(tag);
    public static bool HasModifierWithTagPrefix(string id, string tagPrefix)
    {
        if (stats.TryGetValue(id, out StatEntry entry))
        {
            foreach (string tag in entry.Modifiers.Keys)
            {
                if (tag != null && tag.StartsWith(tagPrefix, StringComparison.Ordinal))
                    return true;
            }
        }

        return false;
    }

    public static float GetMultiplier(string id) => stats.TryGetValue(id, out StatEntry entry) ? entry.Multiplier : 1f;

    public static void ClearModifiers(string id)
    {
        if (stats.TryGetValue(id, out StatEntry entry))
            entry.Modifiers.Clear();
    }

    public static void ClearModifiersWithTagPrefix(string id, string tagPrefix)
    {
        if (stats.TryGetValue(id, out StatEntry entry))
        {
            foreach (string tag in entry.Modifiers.Keys.ToList())
            {
                if (tag != null && tag.StartsWith(tagPrefix, StringComparison.Ordinal))
                    _ = entry.Modifiers.Remove(tag);
            }
        }
    }
}
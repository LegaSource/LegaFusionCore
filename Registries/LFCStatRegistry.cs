using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LegaFusionCore.Registries;

public static class LFCStatRegistry
{
    private class StatEntry
    {
        public float baseValue;
        public float? minValue;
        public float? maxValue;

        public readonly Dictionary<string, float> modifiers = [];

        public float FinalValue
        {
            get
            {
                float multiplier = 1f + modifiers.Values.Sum();
                float value = baseValue * multiplier;

                if (minValue.HasValue) value = Mathf.Max(minValue.Value, value);
                if (maxValue.HasValue) value = Mathf.Min(maxValue.Value, value);
                return value;
            }
        }
    }

    private static readonly Dictionary<string, StatEntry> stats = [];

    public static void RegisterStat(string id, float baseValue, float? min = null, float? max = null)
        => stats[id] = new StatEntry
        {
            baseValue = baseValue,
            minValue = min,
            maxValue = max
        };

    public static void SetBaseValue(string id, float newBase)
    {
        if (stats.TryGetValue(id, out StatEntry entry))
            entry.baseValue = newBase;
    }

    public static void AddModifier(string id, string tag, float value)
    {
        if (!stats.TryGetValue(id, out StatEntry entry)) return;
        entry.modifiers[tag] = value;
    }

    public static void RemoveModifier(string id, string tag)
    {
        if (!stats.TryGetValue(id, out StatEntry entry)) return;
        _ = entry.modifiers.Remove(tag);
    }

    public static float? GetFinalValue(string id)
        => stats.TryGetValue(id, out StatEntry entry) ? entry.FinalValue : null;

    public static bool HasModifier(string id, string sourceTag)
        => stats.TryGetValue(id, out StatEntry entry) && entry.modifiers.ContainsKey(sourceTag);

    public static float GetSumModifier(string id)
        => stats.TryGetValue(id, out StatEntry entry) ? entry.modifiers.Values.Sum() : 0f;

    public static void ClearModifiers(string id)
    {
        if (stats.TryGetValue(id, out StatEntry entry))
            entry.modifiers.Clear();
    }
}

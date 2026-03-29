using LegaFusionCore.Managers;
using System;
using System.Collections.Generic;

namespace LegaFusionCore.Registries;

public static class LFCSpawnableItemRegistry
{
    private static readonly HashSet<SpawnableItem> spawnableItems = [];

    public static void Add(Type type, Item item, int minSpawn, int maxSpawn, int rarity, int minValue = 0, int maxValue = 0)
    {
        _ = LFCObjectsManager.RegisterObject(type, item);
        _ = spawnableItems.Add(new SpawnableItem(type, item, minSpawn, maxSpawn, rarity, minValue, maxValue));
    }
    public static IReadOnlyCollection<SpawnableItem> GetAll() => spawnableItems;

    public class SpawnableItem(Type type, Item item, int minSpawn, int maxSpawn, int rarity, int minValue, int maxValue)
    {
        public Type Type { get; } = type;
        public Item Item { get; } = item;
        public int MinSpawn { get; } = minSpawn;
        public int MaxSpawn { get; } = maxSpawn;
        public int Rarity { get; } = rarity;
        public int MinValue { get; } = minValue;
        public int MaxValue { get; } = maxValue;

        public override bool Equals(object obj) => obj is SpawnableItem other && other.Item == Item;
        public override int GetHashCode() => Item?.GetHashCode() ?? 0;
    }
}
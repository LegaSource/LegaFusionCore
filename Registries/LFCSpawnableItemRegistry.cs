using LethalLib.Modules;
using System;
using System.Collections.Generic;

namespace LegaFusionCore.Registries;

public static class LFCSpawnableItemRegistry
{
    private static readonly HashSet<SpawnableItem> spawnableItems = [];

    public static void Add(Type type, Item item, int minSpawn, int maxSpawn, int rarity, int value = 0)
    {
        PhysicsProp script = item.spawnPrefab.AddComponent(type) as PhysicsProp;
        script.grabbable = true;
        script.grabbableToEnemies = true;
        script.itemProperties = item;

        NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab);
        LethalLib.Modules.Utilities.FixMixerGroups(item.spawnPrefab);
        Items.RegisterItem(item);
        _ = spawnableItems.Add(new SpawnableItem(type, item, minSpawn, maxSpawn, rarity, value));
    }
    public static IReadOnlyCollection<SpawnableItem> GetAll() => spawnableItems;

    public class SpawnableItem(Type type, Item item, int minSpawn, int maxSpawn, int rarity, int value)
    {
        public Type Type { get; } = type;
        public Item Item { get; } = item;
        public int MinSpawn { get; } = minSpawn;
        public int MaxSpawn { get; } = maxSpawn;
        public int Rarity { get; } = rarity;
        public int Value { get; } = value;

        public override bool Equals(object obj) => obj is SpawnableItem other && other.Item == Item;
        public override int GetHashCode() => Item?.GetHashCode() ?? 0;
    }
}
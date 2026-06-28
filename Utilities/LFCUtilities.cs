using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LegaFusionCore.Utilities;

public static class LFCUtilities
{
    ////////////////////////////////////////////////// METHODES COMMUNES //////////////////////////////////////////////////
    public static PlayerControllerB LocalPlayer => GameNetworkManager.Instance?.localPlayerController;
    public static bool IsServer => LocalPlayer != null && (LocalPlayer.IsServer || LocalPlayer.IsHost);
    public static bool ShouldBeLocalPlayer(PlayerControllerB player) => player != null && player == GameNetworkManager.Instance?.localPlayerController;
    public static bool ShouldNotBeLocalPlayer(PlayerControllerB player) => player != null && player != GameNetworkManager.Instance?.localPlayerController;
    public static ulong EncodePlayerId(ulong playerClientId) => (1UL << 63) | playerClientId;

    public static bool TryGetComponentInParent<T>(this GameObject gameObject, out T result) where T : Component
    {
        result = gameObject.GetComponentInParent<T>();
        return result != null;
    }

    public static bool TryGetComponentInChildren<T>(this GameObject gameObject, out T result) where T : Component
    {
        result = gameObject.GetComponentInChildren<T>();
        return result != null;
    }

    public static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            (list[randomIndex], list[i]) = (list[i], list[randomIndex]);
        }
    }

    public static string GetGameObjectName(GameObject gObject)
    {
        if (gObject == null) return string.Empty;

        string name = gObject.name ?? string.Empty;
        const string clone = "(Clone)";

        if (name.EndsWith(clone, StringComparison.Ordinal))
            name = name[..^clone.Length];

        return name.Trim();
    }

    public static bool HasNameFromList(string name, string listName)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(listName))
            return false;

        foreach (string raw in listName.Split(','))
        {
            string item = raw.Trim();
            if (item.Length == 0) continue;

            if (string.Equals(item, name, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    public static void UpdateTimer(ref float timer, float cooldown, bool isActive, Action onReady)
    {
        if (isActive)
        {
            timer += Time.deltaTime;
            if (timer >= cooldown)
            {
                timer = 0f;
                onReady?.Invoke();
            }
        }
    }

    public static GameObject GetPrefabFromName(string name)
    {
        GameObject prefab = null;
        foreach (NetworkPrefabsList networkPrefabList in NetworkManager.Singleton.NetworkConfig.Prefabs.NetworkPrefabsLists ?? Enumerable.Empty<NetworkPrefabsList>())
        {
            foreach (NetworkPrefab networkPrefab in networkPrefabList.PrefabList ?? Enumerable.Empty<NetworkPrefab>())
            {
                if (!networkPrefab.Prefab.name.Equals(name)) continue;

                prefab = networkPrefab.Prefab;
                if (prefab != null) break;
            }
        }
        return prefab;
    }

    public static T GetSafeComponent<T>(GameObject gameObject) where T : Component
        => gameObject == null || gameObject is not UnityEngine.Object obj || !obj ? null : gameObject.GetComponent<T>();

    ////////////////////////////////////////////////// SERIALIZABLE //////////////////////////////////////////////////

    [Serializable]
    public class SerializableEntry<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;
    }

    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<SerializableEntry<TKey, TValue>> entries)
    {
        Dictionary<TKey, TValue> dictionary = [];

        foreach (SerializableEntry<TKey, TValue> entry in entries)
            dictionary[entry.Key] = entry.Value;

        return dictionary;
    }
}

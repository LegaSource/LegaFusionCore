using System.Collections.Generic;

namespace LegaFusionCore.Registries;

public static class LFCPlayerActionRegistry
{
    private static readonly Dictionary<string, HashSet<string>> lockRegistry = [];

    public static void AddLock(string actionName, string tag)
    {
        if (!lockRegistry.TryGetValue(actionName, out HashSet<string> tagSet))
        {
            tagSet = [];
            lockRegistry[actionName] = tagSet;
        }

        if (tagSet.Add(tag))
        {
            // Si c'est le premier tag -> on désactive réellement
            if (tagSet.Count == 1)
                SetActionEnabled(actionName, false);
        }
    }

    public static void RemoveLock(string actionName, string tag)
    {
        if (!lockRegistry.TryGetValue(actionName, out HashSet<string> tagSet)) return;

        if (tagSet.Remove(tag))
        {
            // Si plus aucun tag -> on réactive l’action
            if (tagSet.Count == 0)
                SetActionEnabled(actionName, true);
        }
    }

    public static bool IsLocked(string actionName) => lockRegistry.TryGetValue(actionName, out HashSet<string> tagSet) && tagSet.Count > 0;

    private static void SetActionEnabled(string actionName, bool enabled)
    {
        UnityEngine.InputSystem.InputAction action = IngamePlayerSettings.Instance.playerInput.actions.FindAction(actionName, false);
        if (action == null)
        {
            LegaFusionCore.mls.LogWarning($"Action '{actionName}' not found");
            return;
        }

        if (enabled) action.Enable();
        else action.Disable();
    }
}

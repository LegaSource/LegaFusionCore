using System.Collections.Generic;
using System.Linq;

namespace LegaFusionCore.Registries;

public class LFCObjectStateRegistry
{
    private static readonly Dictionary<FlashlightItem, HashSet<string>> flickeringFlashlightRegistry = [];

    public static void AddFlickeringFlashlight(FlashlightItem flashlight, string tag)
    {
        if (!flickeringFlashlightRegistry.TryGetValue(flashlight, out HashSet<string> tagSet))
        {
            tagSet = [];
            flickeringFlashlightRegistry[flashlight] = tagSet;
        }

        if (tagSet.Add(tag))
        {
            // Si c'est le premier tag -> on active réellement
            if (tagSet.Count == 1)
                SetFlickeringFlashlightEnabled(flashlight, true);
        }
    }

    public static void RemoveFlickeringFlashlight(FlashlightItem flashlight, string tag)
    {
        if (!flickeringFlashlightRegistry.TryGetValue(flashlight, out HashSet<string> tagSet)) return;

        if (tagSet.Remove(tag))
        {
            // Si plus aucun tag -> on désactive
            if (tagSet.Count == 0)
                SetFlickeringFlashlightEnabled(flashlight, false);
        }
    }

    public static void ClearFlickeringFlashlight()
    {
        foreach (FlashlightItem flashlight in flickeringFlashlightRegistry.Keys.ToList())
        {
            if (!flickeringFlashlightRegistry.TryGetValue(flashlight, out HashSet<string> tagSet)) continue;

            tagSet.ToList().ForEach(t => RemoveFlickeringFlashlight(flashlight, t));
            _ = flickeringFlashlightRegistry.Remove(flashlight);
        }
    }

    public static bool IsFlickeringFlashlight(FlashlightItem flashlight) => flickeringFlashlightRegistry.TryGetValue(flashlight, out HashSet<string> tagSet) && tagSet.Count > 0;
    private static void SetFlickeringFlashlightEnabled(FlashlightItem flashlight, bool enabled)
    {
        if (enabled)
        {
            flashlight.flashlightAudio.PlayOneShot(flashlight.flashlightFlicker);
            WalkieTalkie.TransmitOneShotAudio(flashlight.flashlightAudio, flashlight.flashlightFlicker, 0.8f);
            flashlight.flashlightInterferenceLevel = 1;
            return;
        }
        flashlight.flashlightInterferenceLevel = 0;
    }
}

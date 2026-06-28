using System.Collections.Generic;
using System.Linq;

namespace LegaFusionCore.Registries;

public class LFCObjectStateRegistry
{
    private static readonly Dictionary<FlashlightItem, HashSet<string>> FlickeringFlashlightRegistry = [];

    public static void AddFlickeringFlashlight(FlashlightItem flashlight, string tag)
    {
        if (!FlickeringFlashlightRegistry.TryGetValue(flashlight, out HashSet<string> tagSet))
        {
            tagSet = [];
            FlickeringFlashlightRegistry[flashlight] = tagSet;
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
        if (FlickeringFlashlightRegistry.TryGetValue(flashlight, out HashSet<string> tagSet))
        {
            if (tagSet.Remove(tag))
            {
                // Si plus aucun tag -> on désactive
                if (tagSet.Count == 0)
                    SetFlickeringFlashlightEnabled(flashlight, false);
            }
        }
    }

    public static void ClearFlickeringFlashlight()
    {
        foreach (FlashlightItem flashlight in FlickeringFlashlightRegistry.Keys.ToList())
        {
            if (FlickeringFlashlightRegistry.TryGetValue(flashlight, out HashSet<string> tagSet))
            {
                tagSet.ToList().ForEach(t => RemoveFlickeringFlashlight(flashlight, t));
                _ = FlickeringFlashlightRegistry.Remove(flashlight);
            }
        }
    }

    public static bool IsFlickeringFlashlight(FlashlightItem flashlight) => FlickeringFlashlightRegistry.TryGetValue(flashlight, out HashSet<string> tagSet) && tagSet.Count > 0;
    private static void SetFlickeringFlashlightEnabled(FlashlightItem flashlight, bool enabled)
    {
        if (flashlight != null)
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
}

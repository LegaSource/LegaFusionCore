using System.Collections.Generic;

namespace LegaFusionCore.Behaviours.Addons;

public static class AddonTargetDatabase
{
    public enum AddonTargetType
    {
        ALL,
        FLASHLIGHT,
        KNIFE,
        SHOVEL,
        SPRAY_PAINT,
        WALKIE_TALKIE,
        BOOMBOX,
        SHOTGUN
    }

    public static readonly List<string> flashlightNames =
    [
        "Pro-flashlight",
        "Flashlight"
    ];

    public static readonly List<string> knifeNames =
    [
        "Kitchen Knife",
        "Poison Dagger"
    ];

    public static readonly List<string> shovelNames =
    [
        "Shovel"
    ];

    public static readonly List<string> sprayPaintNames =
    [
        "Spray Paint"
    ];

    public static readonly List<string> walkieTalkieNames =
    [
        "Walkie-Talkie"
    ];

    public static readonly List<string> boomboxNames =
    [
        "Boombox"
    ];

    public static readonly List<string> shotgunNames =
    [
        "Shotgun"
    ];
}

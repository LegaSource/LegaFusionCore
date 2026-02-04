using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LegaFusionCore.ModsCompat;

public static class GoodItemScanSoftCompat
{
    public static void Patch(Harmony harmony)
    {
        Type scannerType = Type.GetType("GoodItemScan.Scanner, GoodItemScan");
        if (scannerType != null)
        {
            MethodInfo scanNodes = AccessTools.Method(scannerType, "ScanNodes");
            if (scanNodes != null)
            {
                HarmonyMethod prefix = new HarmonyMethod(AccessTools.Method(typeof(GoodItemScanSoftCompat), nameof(ScanNodes)));
                _ = harmony.Patch(scanNodes, prefix: prefix);
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void ScanNodes(ref ScanNodeProperties[] scanNodes)
    {
        if (scanNodes != null && scanNodes.Length > 0)
            scanNodes = scanNodes.Where(n => n != null && n.TryGetComponent(out Collider collider) && collider.enabled).ToArray();
    }
}

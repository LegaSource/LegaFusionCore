using GameNetcodeStuff;
using LegaFusionCore.Utilities;
using UnityEngine;

namespace LegaFusionCore.Managers;

public static class LFCPlayerManager
{
    public static void HealPlayerOnLocalClient(PlayerControllerB player, int regenHP)
    {
        if (LFCUtilities.ShouldBeLocalPlayer(player) && player.isPlayerDead)
        {
            player.health = Mathf.Min(player.health + regenHP, 100);
            HUDManager.Instance.UpdateHealthUI(player.health, hurtPlayer: false);
            player.DamagePlayerClientRpc(-regenHP, player.health);
            if (player.criticallyInjured && player.health >= 10) player.MakeCriticallyInjured(enable: false);
        }
    }
}

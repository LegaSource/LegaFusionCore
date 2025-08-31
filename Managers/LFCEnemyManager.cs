using System;
using System.Linq;

namespace LegaFusionCore.Managers;

public static class LFCEnemyManager
{
    public static bool CanDie(EnemyAI enemy)
    {
        if (enemy?.enemyType == null) return false;

        string[] enemiesNotTagged = ["Spring", "Jester", "Clay Surgeon", "Red Locust Bees", "Earth Leviathan", "Girl", "Blob", "Butler Bees", "RadMech", "Docile Locust Bees", "Puffer"];
        return enemy.enemyType.canDie && !enemiesNotTagged.Contains(enemy.enemyType.enemyName);
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LegaFusionCore.Behaviours;

public class LFCEnemySpeedBehaviour : MonoBehaviour
{
    private struct EnemySpeedData(float speedFactor, float originalSpeed)
    {
        public float speedFactor = speedFactor;
        public float originalSpeed = originalSpeed;
    }

    public EnemyAI Enemy;
    private readonly Dictionary<string, EnemySpeedData> EnemySpeedFactor = [];

    private float FinalSpeed
    {
        get
        {
            float multiplier = 1f + EnemySpeedFactor.Values.Select(e => e.speedFactor).Sum();
            float maxSpeed = EnemySpeedFactor.Values.Select(e => e.originalSpeed).Max();
            return maxSpeed * multiplier;
        }
    }

    public void AddSpeedData(string id, float speedFactor, float originalSpeed)
    {
        if (!HasSpeedData(id))
            EnemySpeedFactor[id] = new EnemySpeedData(speedFactor, originalSpeed);
    }

    public void RemoveSpeedData(string id)
    {
        if (EnemySpeedFactor.TryGetValue(id, out EnemySpeedData enemySpeedData))
        {
            if (Enemy.agent.speed > 0f)
            {
                Enemy.agent.speed = enemySpeedData.originalSpeed;
                if (Enemy is SandSpiderAI spider)
                    spider.spiderSpeed = Enemy.agent.speed;
            }
        }
        _ = EnemySpeedFactor.Remove(id);
    }

    public bool HasSpeedData(string id) => EnemySpeedFactor.TryGetValue(id, out _);

    private void ApplySpeedData()
    {
        if (Enemy != null && Enemy.agent != null && EnemySpeedFactor.Count != 0)
        {
            Enemy.agent.speed = Mathf.Min(Enemy.agent.speed, FinalSpeed);
            if (Enemy is SandSpiderAI spider)
                spider.spiderSpeed = Enemy.agent.speed;
        }
    }

    private void Update() => ApplySpeedData();
    private void LateUpdate() => ApplySpeedData();
}

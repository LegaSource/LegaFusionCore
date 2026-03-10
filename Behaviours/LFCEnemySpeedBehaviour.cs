using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Tools.NetworkProfiler.Runtime;
using UnityEngine;

namespace LegaFusionCore.Behaviours;

public class LFCEnemySpeedBehaviour : MonoBehaviour
{
    private struct EnemySpeedData(float speedFactor, float originalSpeed)
    {
        public float speedFactor = speedFactor;
        public float originalSpeed = originalSpeed;
    }

    public EnemyAI enemy;
    private readonly Dictionary<string, EnemySpeedData> enemySpeedFactor = [];

    private float FinalSpeed
    {
        get
        {
            float multiplier = 1f + enemySpeedFactor.Values.Select(e => e.speedFactor).Sum();
            float maxSpeed = enemySpeedFactor.Values.Select(e => e.originalSpeed).Max();
            return maxSpeed * multiplier;
        }
    }

    public void AddSpeedData(string id, float speedFactor, float originalSpeed)
    {
        if (!HasSpeedData(id))
            enemySpeedFactor[id] = new EnemySpeedData(speedFactor, originalSpeed);
    }

    public void RemoveSpeedData(string id)
    {
        if (enemySpeedFactor.TryGetValue(id, out EnemySpeedData enemySpeedData))
        {
            if (enemy.agent.speed > 0f)
            {
                enemy.agent.speed = enemySpeedData.originalSpeed;
                if (enemy is SandSpiderAI spider)
                    spider.spiderSpeed = enemy.agent.speed;
            }
        }
        _ = enemySpeedFactor.Remove(id);
    }

    public bool HasSpeedData(string id) => enemySpeedFactor.TryGetValue(id, out _);

    private void ApplySpeedData()
    {
        if (enemy != null && enemy.agent != null && enemySpeedFactor.Count != 0)
        {
            enemy.agent.speed = Mathf.Min(enemy.agent.speed, FinalSpeed);
            if (enemy is SandSpiderAI spider)
                spider.spiderSpeed = enemy.agent.speed;
        }
    }

    private void Update() => ApplySpeedData();
    private void LateUpdate() => ApplySpeedData();
}

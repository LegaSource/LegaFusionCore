using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Tools.NetworkProfiler.Runtime;
using UnityEngine;

namespace LegaFusionCore.Behaviours;

public class EnemySpeedBehaviour : MonoBehaviour
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
            float speed = Mathf.Max(0.5f, maxSpeed * multiplier);
            return speed;
        }
    }

    public void AddSpeedData(string id, float speedFactor, float originalSpeed)
    {
        if (enemySpeedFactor.TryGetValue(id, out _)) return;
        enemySpeedFactor[id] = new EnemySpeedData(speedFactor, originalSpeed);
    }

    public void RemoveSpeedData(string id) => _ = enemySpeedFactor.Remove(id);

    private void ApplySpeedData()
    {
        if (enemy == null || enemy.agent == null || enemySpeedFactor.Count == 0) return;

        enemy.agent.speed = FinalSpeed;
        if (enemy is SandSpiderAI spider) spider.spiderSpeed = enemy.agent.speed;
    }

    private void Update() => ApplySpeedData();
    private void LateUpdate() => ApplySpeedData();
}

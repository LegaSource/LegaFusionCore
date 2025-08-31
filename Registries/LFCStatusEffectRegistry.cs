using LegaFusionCore.Managers;
using LegaFusionCore.Patches;
using LegaFusionCore.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LegaFusionCore.Registries;

public class LFCStatusEffectRegistry : MonoBehaviour
{
    public enum StatusEffectType
    {
        BLEEDING,
        FROST,
        FEAR
    }

    private class StatusEffect
    {
        public StatusEffectType EffectType;
        public int PlayerWhoHit;
        public int Duration;
        public float EndTime;
        public int TotalDamage;
        public int DamagePerTick;
        public float NextTickTime;

        public Action OnApply;
        public Action OnExpire;
        public Action OnTick;

        public float RemainingTime => EndTime - Time.time;

        public StatusEffect(StatusEffectType effectType, int playerWhoHit, int duration, int totalDamage, Action onApply, Action onExpire, Action onTick)
        {
            EffectType = effectType;
            PlayerWhoHit = playerWhoHit;
            Duration = duration;
            EndTime = Time.time + duration;
            TotalDamage = totalDamage;
            OnApply = onApply;
            OnExpire = onExpire;
            OnTick = onTick;

            DamagePerTick = duration > 0 ? totalDamage / duration : 0;
            NextTickTime = Time.time + 1f;
        }
    }

    private static readonly Dictionary<GameObject, Dictionary<StatusEffectType, StatusEffect>> activeEffects = [];

    private static LFCStatusEffectRegistry _instance;
    public static LFCStatusEffectRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject statusEffect = new GameObject("StatusEffect");
                _instance = statusEffect.AddComponent<LFCStatusEffectRegistry>();
                DontDestroyOnLoad(statusEffect);
            }
            return _instance;
        }
    }

    private void Update()
    {
        List<(GameObject, StatusEffectType)> expiredStatus = [];

        foreach (KeyValuePair<GameObject, Dictionary<StatusEffectType, StatusEffect>> kv in activeEffects)
        {
            GameObject entity = kv.Key;
            Dictionary<StatusEffectType, StatusEffect> effects = kv.Value;

            foreach (KeyValuePair<StatusEffectType, StatusEffect> effectKvp in effects)
            {
                StatusEffect effect = effectKvp.Value;

                // Tick toutes les secondes
                if (Time.time >= effect.NextTickTime)
                {
                    effect.OnTick?.Invoke();
                    if (effect.DamagePerTick != 0) ApplyDamage(entity, effect.EffectType, effect.DamagePerTick, effect.PlayerWhoHit);
                    effect.NextTickTime += 1f;
                }

                // Vérifie l'expiration
                if (Time.time >= effect.EndTime)
                {
                    effect.OnExpire?.Invoke();
                    expiredStatus.Add((entity, effectKvp.Key));
                }
            }
        }

        expiredStatus.ForEach(e => RemoveStatus(e.Item1, e.Item2));
    }

    private static void ApplyDamage(GameObject entity, StatusEffectType effectType, int damage, int playerWhoHit)
    {
        EnemyAI enemy = LFCUtilities.GetSafeComponent<EnemyAI>(entity);
        if (enemy == null || enemy.isEnemyDead || !LFCEnemyManager.CanDie(enemy)) return;

        EnemyAIPatch.DamageEnemy(enemy, damage, playerWhoHit, true);

        switch (effectType)
        {
            case StatusEffectType.BLEEDING:
                LFCGlobalManager.PlayParticle($"{LegaFusionCore.modName}{LegaFusionCore.bloodParticle.name}", enemy.transform.position, Quaternion.identity);
                break;
        }
    }

    public static void ApplyStatus(GameObject entity, StatusEffectType effectType, int playerWhoHit, int duration, int totalDamage = 0, Action onApply = null, Action onExpire = null, Action onTick = null)
    {
        _ = Instance;
        if (!activeEffects.ContainsKey(entity)) activeEffects[entity] = [];

        Dictionary<StatusEffectType, StatusEffect> effects = activeEffects[entity];

        if (effects.TryGetValue(effectType, out StatusEffect existingStatus))
        {
            // Remplace seulement si la nouvelle durée est plus longue
            if (duration > existingStatus.RemainingTime)
            {
                existingStatus.OnExpire?.Invoke();
                effects[effectType] = new StatusEffect(effectType, playerWhoHit, duration, totalDamage, onApply, onExpire, onTick);
                onApply?.Invoke();
            }
        }
        else
        {
            effects[effectType] = new StatusEffect(effectType, playerWhoHit, duration, totalDamage, onApply, onExpire, onTick);
            onApply?.Invoke();
        }
    }

    public static void RemoveStatus(GameObject target, StatusEffectType effectType)
    {
        if (activeEffects.TryGetValue(target, out Dictionary<StatusEffectType, StatusEffect> effects) && effects.TryGetValue(effectType, out StatusEffect effect))
        {
            effect.OnExpire?.Invoke();
            _ = effects.Remove(effectType);
        }
    }

    public static bool HasStatus(GameObject target, StatusEffectType effectType)
        => activeEffects.TryGetValue(target, out Dictionary<StatusEffectType, StatusEffect> effects) && effects.ContainsKey(effectType);
}
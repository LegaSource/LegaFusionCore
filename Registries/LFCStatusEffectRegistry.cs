using GameNetcodeStuff;
using LegaFusionCore.Behaviours;
using LegaFusionCore.Behaviours.Shaders;
using LegaFusionCore.Managers;
using LegaFusionCore.Patches;
using LegaFusionCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LegaFusionCore.Registries;

public class LFCStatusEffectRegistry : MonoBehaviour
{
    public enum StatusEffectType
    {
        BLEEDING,
        FROST,
        POISON,
        FEAR
    }

    public abstract class StatusEffect
    {
        public StatusEffectType EffectType { get; }
        public int PlayerWhoHit { get; }
        public int Duration { get; }
        public float EndTime { get; private set; }
        public int TotalDamage { get; }
        public int DamagePerTick { get; }
        public float NextTickTime { get; private set; }

        public Action OnApply;
        public Action OnExpire;
        public Action OnTick;

        public float RemainingTime => EndTime - Time.time;

        protected StatusEffect(StatusEffectType effectType, int playerWhoHit, int duration, int totalDamage, Action onApply, Action onExpire, Action onTick)
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

        public virtual void Apply(GameObject entity) => OnApply?.Invoke();

        public virtual void Tick(GameObject entity)
        {
            OnTick?.Invoke();
            if (DamagePerTick != 0) ApplyDamage(entity, DamagePerTick, PlayerWhoHit);
            NextTickTime += 1f;
        }

        public virtual void Expire(GameObject entity) => OnExpire?.Invoke();

        public bool ShouldTick() => Time.time >= NextTickTime;
        public bool IsExpired() => Time.time >= EndTime;

        protected void ApplyDamage(GameObject entity, int damage, int playerWhoHit)
        {
            if (entity == null) return;

            EnemyAI enemy = LFCUtilities.GetSafeComponent<EnemyAI>(entity);
            if (enemy != null && !enemy.isEnemyDead && LFCEnemyManager.CanDie(enemy))
                EnemyAIPatch.DamageEnemy(enemy, damage, playerWhoHit, playHitSFX: true);

            PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
            if (player != null && !player.isPlayerDead)
                player.DamagePlayer(damage, hasDamageSFX: true, callRPC: true, CauseOfDeath.Unknown);
        }
    }

    public class BleedingEffect(int playerWhoHit, int duration, int totalDamage, Action onApply = null, Action onExpire = null, Action onTick = null)
        : StatusEffect(StatusEffectType.BLEEDING, playerWhoHit, duration, totalDamage, onApply, onExpire, onTick)
    {
        public override void Apply(GameObject entity)
        {
            base.Apply(entity);

            PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
            if (LFCUtilities.ShouldNotBeLocalPlayer(player))
                CustomPassManager.SetupAuraForObjects([entity.gameObject], LegaFusionCore.bloodShader, $"{LegaFusionCore.modName}{LegaFusionCore.bloodShader.name}");
        }

        public override void Tick(GameObject entity)
        {
            base.Tick(entity);
            LFCGlobalManager.PlayParticle($"{LegaFusionCore.modName}{LegaFusionCore.bloodParticle.name}", entity.transform.position, Quaternion.identity);
        }

        public override void Expire(GameObject entity)
        {
            base.Expire(entity);
            CustomPassManager.RemoveAuraFromObjects([entity.gameObject], $"{LegaFusionCore.modName}{LegaFusionCore.bloodShader.name}");
        }
    }

    public class FrostEffect(int playerWhoHit, int duration, int totalDamage, Action onApply = null, Action onExpire = null, Action onTick = null)
        : StatusEffect(StatusEffectType.FROST, playerWhoHit, duration, totalDamage, onApply, onExpire, onTick)
    {
        public override void Apply(GameObject entity)
        {
            base.Apply(entity);

            EnemyAI enemy = LFCUtilities.GetSafeComponent<EnemyAI>(entity);
            if (enemy != null && !enemy.isEnemyDead)
            {
                EnemySpeedBehaviour speedBehaviour = enemy.GetComponent<EnemySpeedBehaviour>();
                speedBehaviour?.AddSpeedData(StatusEffectType.FROST.ToString(), -0.67f, enemy.agent.speed);
                return;
            }

            PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
            if (player == null || player.isPlayerDead) return;

            if (LFCUtilities.ShouldBeLocalPlayer(player)) LFCStatRegistry.AddModifier(Constants.STAT_SPEED, $"{LegaFusionCore.modName}{LegaFusionCore.frostShader.name}", -100f);
            else CustomPassManager.SetupAuraForObjects([entity.gameObject], LegaFusionCore.frostShader, $"{LegaFusionCore.modName}{LegaFusionCore.frostShader.name}");
        }

        public override void Expire(GameObject entity)
        {
            base.Expire(entity);
            CustomPassManager.RemoveAuraFromObjects([entity.gameObject], $"{LegaFusionCore.modName}{LegaFusionCore.frostShader.name}");

            EnemyAI enemy = LFCUtilities.GetSafeComponent<EnemyAI>(entity);
            if (enemy != null)
            {
                EnemySpeedBehaviour speedBehaviour = enemy.GetComponent<EnemySpeedBehaviour>();
                speedBehaviour?.RemoveSpeedData(StatusEffectType.FROST.ToString());
                return;
            }

            PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
            if (LFCUtilities.ShouldBeLocalPlayer(player)) LFCStatRegistry.RemoveModifier(Constants.STAT_SPEED, $"{LegaFusionCore.modName}{LegaFusionCore.frostShader.name}");
        }
    }

    public class PoisonEffect(int playerWhoHit, int duration, int totalDamage, Action onApply = null, Action onExpire = null, Action onTick = null)
        : StatusEffect(StatusEffectType.POISON, playerWhoHit, duration, totalDamage, onApply, onExpire, onTick)
    {
        public override void Apply(GameObject entity)
        {
            base.Apply(entity);

            PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
            if (LFCUtilities.ShouldNotBeLocalPlayer(player))
                CustomPassManager.SetupAuraForObjects([entity.gameObject], LegaFusionCore.poisonShader, $"{LegaFusionCore.modName}{LegaFusionCore.poisonShader.name}");
        }

        public override void Expire(GameObject entity)
        {
            base.Expire(entity);
            CustomPassManager.RemoveAuraFromObjects([entity.gameObject], $"{LegaFusionCore.modName}{LegaFusionCore.poisonShader.name}");
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
        List<GameObject> deadEntities = [];

        foreach (KeyValuePair<GameObject, Dictionary<StatusEffectType, StatusEffect>> kv in activeEffects)
        {
            GameObject entity = kv.Key;
            if (entity == null)
            {
                deadEntities.Add(entity);
                continue;
            }
            Dictionary<StatusEffectType, StatusEffect> effects = kv.Value;

            foreach (KeyValuePair<StatusEffectType, StatusEffect> effectKvp in effects)
            {
                StatusEffect effect = effectKvp.Value;
                if (effect.ShouldTick()) effect.Tick(entity);
                if (effect.IsExpired()) expiredStatus.Add((entity, effectKvp.Key));
            }
        }

        deadEntities.ForEach(e => activeEffects.Remove(e));
        expiredStatus.ForEach(e => RemoveStatus(e.Item1, e.Item2));
    }

    public static void ApplyStatus(GameObject entity, StatusEffectType type, int playerWhoHit, int duration, int totalDamage = 0, Action onApply = null, Action onExpire = null, Action onTick = null)
    {
        StatusEffect effect = type switch
        {
            StatusEffectType.BLEEDING => new BleedingEffect(playerWhoHit, duration, totalDamage, onApply, onExpire, onTick),
            StatusEffectType.FROST => new FrostEffect(playerWhoHit, duration, totalDamage, onApply, onExpire, onTick),
            StatusEffectType.POISON => new PoisonEffect(playerWhoHit, duration, totalDamage, onApply, onExpire, onTick),
            _ => null
        };

        if (effect != null) ApplyStatus(entity, effect);
    }

    public static void ApplyStatus(GameObject entity, StatusEffect effect)
    {
        _ = Instance;
        if (!activeEffects.ContainsKey(entity)) activeEffects[entity] = [];

        Dictionary<StatusEffectType, StatusEffect> effects = activeEffects[entity];

        if (effects.TryGetValue(effect.EffectType, out StatusEffect existingStatus))
        {
            // Remplace seulement si la nouvelle durée est plus longue
            if (effect.RemainingTime > existingStatus.RemainingTime)
            {
                existingStatus.Expire(entity);
                effects[effect.EffectType] = effect;
                effect.Apply(entity);
            }
        }
        else
        {
            effects[effect.EffectType] = effect;
            effect.Apply(entity);
        }
    }

    public static void RemoveStatus(GameObject entity, StatusEffectType effectType)
    {
        if (activeEffects.TryGetValue(entity, out Dictionary<StatusEffectType, StatusEffect> effects) && effects.TryGetValue(effectType, out StatusEffect effect))
        {
            effect.Expire(entity);
            _ = effects.Remove(effectType);
        }
    }

    public static bool HasStatus(GameObject target, StatusEffectType effectType)
        => activeEffects.TryGetValue(target, out Dictionary<StatusEffectType, StatusEffect> effects) && effects.ContainsKey(effectType);

    public static void ClearStatus()
    {
        foreach (GameObject entity in activeEffects.Keys.ToList())
        {
            if (!activeEffects.TryGetValue(entity, out Dictionary<StatusEffectType, StatusEffect> effects)) continue;

            effects.Keys.ToList().ForEach(e => RemoveStatus(entity, e));
            _ = activeEffects.Remove(entity);
        }
    }
}
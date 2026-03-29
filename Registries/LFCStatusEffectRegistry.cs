using GameNetcodeStuff;
using LegaFusionCore.Behaviours;
using LegaFusionCore.Behaviours.Shaders;
using LegaFusionCore.Managers;
using LegaFusionCore.Managers.NetworkManagers;
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
        LIGHTNING,
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
        public int PassedTime { get; private set; }
        public float NextTickTime { get; private set; }

        public EnemyAI EnemyWhoHit;
        public Action OnApply;
        public Action OnExpire;
        public Action OnTick;

        public float RemainingTime => EndTime - Time.time;

        protected StatusEffect(StatusEffectType effectType, int playerWhoHit, int duration, int totalDamage, EnemyAI enemyWhoHit, Action onApply, Action onExpire, Action onTick)
        {
            EffectType = effectType;
            PlayerWhoHit = playerWhoHit;
            Duration = duration;
            EndTime = Time.time + duration;
            TotalDamage = totalDamage;
            EnemyWhoHit = enemyWhoHit;
            OnApply = onApply;
            OnExpire = onExpire;
            OnTick = onTick;

            DamagePerTick = duration > 0 ? totalDamage / duration : 0;
            PassedTime = 0;
            NextTickTime = Time.time + 1f;
        }

        public virtual void Apply(GameObject entity) => OnApply?.Invoke();

        public virtual void Tick(GameObject entity)
        {
            OnTick?.Invoke();
            if (DamagePerTick != 0) ApplyDamage(entity, DamagePerTick, PlayerWhoHit);
            PassedTime += 1;
            NextTickTime += 1f;
        }

        public virtual void Expire(GameObject entity) => OnExpire?.Invoke();

        public bool ShouldTick() => Time.time >= NextTickTime;
        public bool IsExpired() => Time.time >= EndTime;

        protected void ApplyDamage(GameObject entity, int damage, int playerWhoHit)
        {
            if (entity != null)
            {
                EnemyAI enemy = LFCUtilities.GetSafeComponent<EnemyAI>(entity);
                if (enemy != null && !enemy.isEnemyDead && LFCEnemyManager.CanDie(enemy))
                    LFCEnemyDamageBehaviour.DamageEnemy(enemy, damage, playerWhoHit, playHitSFX: true);

                PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
                if (player != null && !player.isPlayerDead)
                    player.DamagePlayer(damage, hasDamageSFX: true, callRPC: true, CauseOfDeath.Unknown);
            }
        }
    }

    public class BleedingEffect(int playerWhoHit, int duration, int totalDamage, EnemyAI enemyWhoHit = null, Action onApply = null, Action onExpire = null, Action onTick = null)
        : StatusEffect(StatusEffectType.BLEEDING, playerWhoHit, duration, totalDamage, enemyWhoHit, onApply, onExpire, onTick)
    {
        public override void Apply(GameObject entity)
        {
            base.Apply(entity);

            PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
            if (player == null || LFCUtilities.ShouldNotBeLocalPlayer(player))
                CustomPassManager.SetupAuraForObjects([entity.gameObject], LegaFusionCore.bloodShader, $"{LegaFusionCore.modName}{LegaFusionCore.bloodShader.name}");
        }

        public override void Tick(GameObject entity)
        {
            base.Tick(entity);
            LFCGlobalManager.PlayParticle($"{LegaFusionCore.modName}{Constants.BLOOD_PARTICLES}", entity.transform.position, Quaternion.identity);
        }

        public override void Expire(GameObject entity)
        {
            base.Expire(entity);
            CustomPassManager.RemoveAuraFromObjects([entity.gameObject], $"{LegaFusionCore.modName}{LegaFusionCore.bloodShader.name}");
        }
    }

    public class FrostEffect(int playerWhoHit, int duration, int totalDamage, EnemyAI enemyWhoHit = null, Action onApply = null, Action onExpire = null, Action onTick = null)
        : StatusEffect(StatusEffectType.FROST, playerWhoHit, duration, totalDamage, enemyWhoHit, onApply, onExpire, onTick)
    {
        public override void Apply(GameObject entity)
        {
            base.Apply(entity);

            EnemyAI enemy = LFCUtilities.GetSafeComponent<EnemyAI>(entity);
            if (enemy != null && !enemy.isEnemyDead)
            {
                enemy.GetComponent<LFCEnemySpeedBehaviour>()?.AddSpeedData(StatusEffectType.FROST.ToString(), -0.67f, enemy.agent.speed);
                CustomPassManager.SetupAuraForObjects([entity.gameObject], LegaFusionCore.frostShader, $"{LegaFusionCore.modName}{LegaFusionCore.frostShader.name}");
                return;
            }

            PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
            if (player != null && !player.isPlayerDead)
            {
                if (LFCUtilities.ShouldBeLocalPlayer(player))
                    LFCStatRegistry.AddModifier(Constants.STAT_SPEED, $"{LegaFusionCore.modName}{LegaFusionCore.frostShader.name}", -100f);
                else
                    CustomPassManager.SetupAuraForObjects([entity.gameObject], LegaFusionCore.frostShader, $"{LegaFusionCore.modName}{LegaFusionCore.frostShader.name}");
            }
        }

        public override void Expire(GameObject entity)
        {
            base.Expire(entity);
            CustomPassManager.RemoveAuraFromObjects([entity.gameObject], $"{LegaFusionCore.modName}{LegaFusionCore.frostShader.name}");

            EnemyAI enemy = LFCUtilities.GetSafeComponent<EnemyAI>(entity);
            if (enemy != null)
            {
                enemy.GetComponent<LFCEnemySpeedBehaviour>()?.RemoveSpeedData(StatusEffectType.FROST.ToString());
                return;
            }

            PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
            if (LFCUtilities.ShouldBeLocalPlayer(player))
                LFCStatRegistry.RemoveModifier(Constants.STAT_SPEED, $"{LegaFusionCore.modName}{LegaFusionCore.frostShader.name}");
        }
    }

    public class PoisonEffect(int playerWhoHit, int duration, int totalDamage, EnemyAI enemyWhoHit = null, Action onApply = null, Action onExpire = null, Action onTick = null)
        : StatusEffect(StatusEffectType.POISON, playerWhoHit, duration, totalDamage, enemyWhoHit, onApply, onExpire, onTick)
    {
        public override void Apply(GameObject entity)
        {
            base.Apply(entity);

            PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
            if (player == null || LFCUtilities.ShouldNotBeLocalPlayer(player))
                CustomPassManager.SetupAuraForObjects([entity.gameObject], LegaFusionCore.poisonShader, $"{LegaFusionCore.modName}{LegaFusionCore.poisonShader.name}");
        }

        public override void Expire(GameObject entity)
        {
            base.Expire(entity);
            CustomPassManager.RemoveAuraFromObjects([entity.gameObject], $"{LegaFusionCore.modName}{LegaFusionCore.poisonShader.name}");
        }
    }

    public class LightningEffect(int playerWhoHit, int duration, int totalDamage, EnemyAI enemyWhoHit = null, Action onApply = null, Action onExpire = null, Action onTick = null)
        : StatusEffect(StatusEffectType.LIGHTNING, playerWhoHit, duration, totalDamage, enemyWhoHit, onApply, onExpire, onTick)
    {
        private readonly HashSet<ulong> affectedIds = [];
        private readonly Collider[] overlapBuffer = new Collider[64];

        public float AoERadius = 3f;
        public int AoEMask = 1084754248;

        public override void Apply(GameObject entity)
        {
            PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
            if (player == null || LFCUtilities.ShouldNotBeLocalPlayer(player))
                CustomPassManager.SetupAuraForObjects([entity.gameObject], LegaFusionCore.lightningShader, $"{LegaFusionCore.modName}{LegaFusionCore.lightningShader.name}");
        }

        public override void Tick(GameObject entity)
        {
            base.Tick(entity);

            if (PassedTime == Duration / 2 || PassedTime == Duration)
            {
                LFCGlobalManager.PlayParticle($"{LegaFusionCore.modName}{LegaFusionCore.lightningExplosionParticle.name}", entity.transform.position, entity.transform.rotation);
                LFCGlobalManager.PlayAudio($"{LegaFusionCore.modName}{LegaFusionCore.lightningExplosionAudio.name}", entity.transform.position);

                if (!LFCUtilities.IsServer)
                    return;

                PlayerControllerB targetedPlayer = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
                PlayerControllerB playerWhoHit = PlayerWhoHit != -1 ? StartOfRound.Instance.allPlayerObjects[PlayerWhoHit].GetComponent<PlayerControllerB>() : null;
                int count = Physics.OverlapSphereNonAlloc(entity.transform.position, AoERadius, overlapBuffer, AoEMask, QueryTriggerInteraction.Collide);

                for (int i = 0; i < count; i++)
                {
                    Collider collider = overlapBuffer[i];
                    if (collider == null) continue;

                    if (collider.gameObject.TryGetComponentInParent(out PlayerControllerB player) && !player.isPlayerDead)
                    {
                        if (player != targetedPlayer && player != playerWhoHit && affectedIds.Add((1UL << 63) | player.playerClientId))
                        {
                            if (!HasStatus(player.gameObject, StatusEffectType.LIGHTNING))
                            {
                                LFCNetworkManager.Instance.ApplyStatusEveryoneRpc(playerId: playerWhoHit != null ? (int)playerWhoHit.playerClientId : -1,
                                    targetId: (int)player.playerClientId,
                                    type: (int)StatusEffectType.LIGHTNING,
                                    duration: Duration,
                                    totalDamage: targetedPlayer != null ? TotalDamage : TotalDamage / 10,
                                    enemyWhoHitObj: EnemyWhoHit != null ? EnemyWhoHit.NetworkObject : default);
                            }
                            LFCNetworkManager.Instance.DamagePlayerEveryoneRpc((int)player.playerClientId, 10, hasDamageSFX: true, callRPC: true, (int)CauseOfDeath.Electrocution);
                        }
                        continue;
                    }

                    if (collider.gameObject.TryGetComponentInParent(out EnemyAICollisionDetect collisionDetect))
                    {
                        EnemyAI enemy = collisionDetect.mainScript;
                        if (enemy != null && !enemy.isEnemyDead && enemy.NetworkObject != null && enemy != EnemyWhoHit && affectedIds.Add(enemy.NetworkObjectId))
                        {
                            if (!HasStatus(enemy.gameObject, StatusEffectType.LIGHTNING))
                            {
                                LFCNetworkManager.Instance.ApplyStatusEveryoneRpc(playerId: playerWhoHit != null ? (int)playerWhoHit.playerClientId : -1,
                                    enemyObj: enemy.NetworkObject,
                                    type: (int)StatusEffectType.LIGHTNING,
                                    duration: Duration,
                                    totalDamage: targetedPlayer != null ? TotalDamage * 10 : TotalDamage,
                                    enemyWhoHitObj: EnemyWhoHit != null ? EnemyWhoHit.NetworkObject : default);
                            }
                            enemy.HitEnemyOnLocalClient(force: 1, playerWhoHit: playerWhoHit, playHitSFX: true);
                        }
                    }
                }
            }
        }

        public override void Expire(GameObject entity)
        {
            base.Expire(entity);
            CustomPassManager.RemoveAuraFromObjects([entity.gameObject], $"{LegaFusionCore.modName}{LegaFusionCore.lightningShader.name}");
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

        foreach (KeyValuePair<GameObject, Dictionary<StatusEffectType, StatusEffect>> kv in activeEffects.ToList())
        {
            GameObject entity = kv.Key;
            if (entity == null)
            {
                deadEntities.Add(entity);
                continue;
            }
            Dictionary<StatusEffectType, StatusEffect> effects = kv.Value;

            foreach (KeyValuePair<StatusEffectType, StatusEffect> effectKvp in effects.ToList())
            {
                StatusEffect effect = effectKvp.Value;
                if (effect.ShouldTick()) effect.Tick(entity);
                if (effect.IsExpired()) expiredStatus.Add((entity, effectKvp.Key));
            }
        }

        deadEntities.ForEach(e => activeEffects.Remove(e));
        expiredStatus.ForEach(e => RemoveStatus(e.Item1, e.Item2));
    }

    public static void ApplyStatus(GameObject entity, StatusEffectType type, int playerWhoHit, int duration, int totalDamage = 0, EnemyAI enemyWhoHit = null, Action onApply = null, Action onExpire = null, Action onTick = null)
    {
        StatusEffect effect = type switch
        {
            StatusEffectType.BLEEDING => new BleedingEffect(playerWhoHit, duration, totalDamage, enemyWhoHit, onApply, onExpire, onTick),
            StatusEffectType.FROST => new FrostEffect(playerWhoHit, duration, totalDamage, enemyWhoHit, onApply, onExpire, onTick),
            StatusEffectType.POISON => new PoisonEffect(playerWhoHit, duration, totalDamage, enemyWhoHit, onApply, onExpire, onTick),
            StatusEffectType.LIGHTNING => new LightningEffect(playerWhoHit, duration, totalDamage, enemyWhoHit, onApply, onExpire, onTick),
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
            if (activeEffects.TryGetValue(entity, out Dictionary<StatusEffectType, StatusEffect> effects))
            {
                effects.Keys.ToList().ForEach(e => RemoveStatus(entity, e));
                _ = activeEffects.Remove(entity);
            }
        }
    }
}
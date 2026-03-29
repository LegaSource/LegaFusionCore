using GameNetcodeStuff;
using LegaFusionCore.Managers;
using LegaFusionCore.Utilities;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LegaFusionCore.Behaviours;

public class LFCBouncyAoEProjectile : NetworkBehaviour, IHittable
{
    public Rigidbody rigidbody;

    public int throwingPlayer = -1;
    public int bouncesLeft = 1;
    protected bool deactivated;

    private Vector3 lastVelocity;
    private readonly HashSet<ulong> affectedIds = [];
    private readonly Collider[] overlapBuffer = new Collider[64];

    public float InsideSpeed = 30f;
    public float OutsideSpeed = 40f;
    public float EntityThrowAngle = 45f;

    public int MaxBounces = 2;
    public float BounceDamping = 0.85f;
    public float PushOut = 0.05f;
    public float ExplosionNormalOffset = 0.2f;

    public float PlayerThrowSpeed = 30f;
    public float PlayerThrowUpVelocity = 3f;
    public float PlayerThrowMinY = -2f;

    public float AoERadius = 2f;
    public float AoEDuration = 2f;
    public float AoETick = 0.2f;
    public int AoEMask = 1084754248;

    protected virtual void PlayExplosionFx(Vector3 position, Quaternion rotation) { }
    protected virtual void PlayExplosionSfx(Vector3 position) { }

    protected virtual void OnAffectPlayerServer(PlayerControllerB player) { }
    protected virtual void OnAffectEnemyServer(EnemyAI enemy) { }

    protected virtual bool CanAffectPlayer(PlayerControllerB player) => true;
    protected virtual bool CanAffectEnemy(EnemyAI enemy)
        => enemy != null && !enemy.isEnemyDead && enemy.NetworkObject != null;

    private void FixedUpdate()
    {
        if (rigidbody != null)
            lastVelocity = rigidbody.velocity;
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ThrowFromPositionEveryoneRpc(ulong entityId, Vector3 startPosition, Vector3 direction, bool isOutside)
    {
        if (deactivated || rigidbody == null) return;
        if (LFCUtilities.IsServer)
            bouncesLeft = Random.Range(0, MaxBounces + 1);

        affectedIds.Clear();
        _ = affectedIds.Add(entityId);

        transform.position = startPosition;
        rigidbody.position = startPosition;
        rigidbody.velocity = Vector3.zero;

        float speed = isOutside ? OutsideSpeed : InsideSpeed;
        rigidbody.AddForce(ComputeArcVelocity(direction, speed, EntityThrowAngle), ForceMode.VelocityChange);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ThrowFromPlayerEveryoneRpc(int playerId, Vector3 startPosition, Vector3 direction)
    {
        if (deactivated || rigidbody == null) return;
        if (LFCUtilities.IsServer)
            bouncesLeft = Random.Range(0, MaxBounces + 1);

        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        throwingPlayer = (int)player.playerClientId;

        affectedIds.Clear();
        _ = affectedIds.Add(LFCUtilities.EncodePlayerId(player.playerClientId));

        if (direction.y < PlayerThrowMinY)
            direction = new Vector3(direction.x, PlayerThrowMinY, direction.z).normalized;

        transform.position = startPosition;
        rigidbody.position = startPosition;
        rigidbody.velocity = Vector3.zero;

        rigidbody.AddForce(direction * PlayerThrowSpeed, ForceMode.VelocityChange);
        rigidbody.AddForce(Vector3.up * PlayerThrowUpVelocity, ForceMode.VelocityChange);
    }

    private static Vector3 ComputeArcVelocity(Vector3 direction, float speed, float angleDeg)
    {
        // Séparation des composantes horizontales et verticales
        Vector3 horizontal = new Vector3(direction.x, 0f, direction.z);
        float horizontalDistance = horizontal.magnitude;
        if (horizontalDistance <= 0.0001f)
            return Vector3.up * speed;

        // Calcul de l'angle de lancement (en radians) pour créer un arc
        float angle = angleDeg * Mathf.Deg2Rad;
        float time = horizontalDistance / (speed * Mathf.Cos(angle));

        // Calcul des vitesses initiales
        float verticalVelocity = (direction.y / time) - (0.5f * Physics.gravity.y * time);
        Vector3 horizontalVelocity = horizontal.normalized * (speed * Mathf.Cos(angle));
        return horizontalVelocity + (Vector3.up * verticalVelocity);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null || deactivated || !LFCUtilities.IsServer || rigidbody == null) return;
        if (collision.collider != null && (collision.collider.gameObject.TryGetComponentInParent(out PlayerControllerB _) || collision.collider.gameObject.TryGetComponentInParent(out EnemyAI _)))
            return;

        ContactPoint cp = collision.GetContact(0);
        Vector3 point = cp.point;
        Vector3 normal = cp.normal;

        if (bouncesLeft > 0)
        {
            bouncesLeft--;

            Vector3 newPos = point + (normal * PushOut);
            Vector3 newVel = Vector3.Reflect(lastVelocity, normal) * BounceDamping;

            rigidbody.position = newPos;
            rigidbody.velocity = newVel;

            BounceEveryoneRpc(newPos, newVel);
            return;
        }

        ExplodeServerRpc(point + (normal * ExplosionNormalOffset), Quaternion.LookRotation(normal));
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider != null && !deactivated)
        {
            if (collider.TryGetComponent(out PlayerControllerB player) && LFCUtilities.ShouldBeLocalPlayer(player) && !affectedIds.Contains(LFCUtilities.EncodePlayerId(player.playerClientId)))
                ExplodeServerRpc(transform.position, Quaternion.identity);
            else if (LFCUtilities.IsServer && collider.TryGetComponent(out EnemyAICollisionDetect collisionDetect) && collisionDetect.mainScript != null && !affectedIds.Contains(collisionDetect.mainScript.NetworkObjectId))
                ExplodeServerRpc(transform.position, Quaternion.identity);
        }
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void ExplodeServerRpc(Vector3 position, Quaternion rotation)
    {
        if (!deactivated)
        {
            PlayHitAudioEveryoneRpc($"{LegaFusionCore.modName}{LegaFusionCore.hitProjectileAudio.name}", position);
            DeactivateProjectile();
            ExplodeEveryoneRpc(position, rotation);
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    private void BounceEveryoneRpc(Vector3 position, Vector3 velocity)
    {
        if (!deactivated && rigidbody != null)
        {
            rigidbody.position = position;
            rigidbody.velocity = velocity;
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    private void PlayHitAudioEveryoneRpc(string tag, Vector3 pos)
        => LFCGlobalManager.PlayAudio(tag, pos);

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    private void ExplodeEveryoneRpc(Vector3 position, Quaternion rotation)
    {
        if (LFCUtilities.IsServer)
            _ = StartCoroutine(AoECoroutine(position, AoEDuration));
        else
            DeactivateProjectile();

        PlayExplosionFx(position, rotation);
        PlayExplosionSfx(position);
    }

    private IEnumerator AoECoroutine(Vector3 position, float duration)
    {
        float timePassed = 0f;
        while (timePassed < duration)
        {
            int count = Physics.OverlapSphereNonAlloc(position, AoERadius, overlapBuffer, AoEMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                Collider collider = overlapBuffer[i];
                if (collider == null) continue;

                if (collider.gameObject.TryGetComponentInParent(out PlayerControllerB player) && !player.isPlayerDead)
                {
                    if (CanAffectPlayer(player) && affectedIds.Add(LFCUtilities.EncodePlayerId(player.playerClientId)))
                        OnAffectPlayerServer(player);
                    continue;
                }

                if (collider.gameObject.TryGetComponentInParent(out EnemyAICollisionDetect collisionDetect))
                {
                    EnemyAI enemy = collisionDetect.mainScript;
                    if (CanAffectEnemy(enemy) && affectedIds.Add(enemy.NetworkObjectId))
                        OnAffectEnemyServer(enemy);
                }
            }

            yield return new WaitForSeconds(AoETick);
            timePassed += AoETick;
        }

        Destroy(gameObject);
    }

    protected virtual void DeactivateProjectile()
    {
        deactivated = true;

        foreach (ParticleSystem particleSystem in GetComponentsInChildren<ParticleSystem>())
            Destroy(particleSystem);
        foreach (MeshRenderer meshRenderer in GetComponentsInChildren<MeshRenderer>())
            Destroy(meshRenderer);
        foreach (TrailRenderer trailRenderer in GetComponentsInChildren<TrailRenderer>())
            Destroy(trailRenderer);
        foreach (Collider collider in GetComponentsInChildren<Collider>())
            Destroy(collider);
    }

    public bool Hit(int force, Vector3 hitDirection, PlayerControllerB playerWhoHit = null, bool playHitSFX = false, int hitID = -1)
    {
        if (playerWhoHit != null && playerWhoHit.currentlyHeldObjectServer is Shovel)
            ThrowFromPlayerEveryoneRpc((int)playerWhoHit.playerClientId, transform.position, playerWhoHit.transform.forward);
        return true;
    }
}

using LegaFusionCore.ModsCompat;
using LegaFusionCore.Utilities;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace LegaFusionCore.Registries;

public class LFCVisibilityRegistry
{
    private static readonly Dictionary<GameObject, EntityState> VisibilityStates = [];

    private sealed class EntityState
    {
        public readonly HashSet<string> tags = [];
        public readonly HashSet<Renderer> disabledRenderers = [];
        public readonly HashSet<Light> disabledLights = [];
        public readonly HashSet<Rigidbody> disabledGravitys = [];
        public readonly HashSet<PlayerPhysicsRegion> disabledPhysicsRegions = [];
        public readonly HashSet<Collider> disabledColliders = [];
        public readonly HashSet<TextMeshProUGUI> disabledTextMeshes = [];
        public readonly HashSet<DecalProjector> disabledDecals = [];
        public readonly HashSet<InteractTrigger> disabledInteractTriggers = [];
        public readonly HashSet<AnimatedObjectTrigger> disabledAnimatedTriggers = [];
        public readonly HashSet<ParticleSystem> disabledParticles = [];
        public readonly Dictionary<AudioSource, float> audioVolumes = [];
    }

    public static void Hide(GameObject entity, string tag)
    {
        if (entity != null)
        {
            if (string.IsNullOrWhiteSpace(tag))
                tag = "default";
            CleanupDeadEntries();

            if (!VisibilityStates.TryGetValue(entity, out EntityState state))
            {
                state = new EntityState();
                VisibilityStates[entity] = state;
            }

            HideInternal(entity, state);
            _ = state.tags.Add(tag);
        }
    }

    public static void Restore(GameObject entity, string tag)
    {
        if (entity != null && VisibilityStates.TryGetValue(entity, out EntityState state))
        {
            if (string.IsNullOrWhiteSpace(tag))
                tag = "default";

            _ = state.tags.Remove(tag);
            if (state.tags.Count == 0)
                RestoreInternal(entity, state);
        }
    }

    // Force restore pour reset/cleanup
    public static void ForceRestore(GameObject entity)
    {
        if (entity != null && VisibilityStates.TryGetValue(entity, out EntityState state))
            RestoreInternal(entity, state);
    }

    public static bool IsHidden(GameObject entity) => entity != null && VisibilityStates.ContainsKey(entity);

    private static void HideInternal(GameObject entity, EntityState state)
    {
        if (LFCUtilities.LocalPlayer != null && entity.TryGetComponentInParent(out VehicleController vehicleController))
        {
            vehicleController.startKeyIgnitionTrigger.SetActive(false);
            vehicleController.removeKeyIgnitionTrigger.SetActive(false);
            vehicleController.backDoorContainer.SetActive(false);
            vehicleController.destroyedTruckMesh.SetActive(false);
            vehicleController.truckDestroyedExplosion.SetActive(false);
            vehicleController.backLightsContainer.SetActive(false);
            vehicleController.frontCabinLightContainer.SetActive(false);
            vehicleController.headlightsContainer.SetActive(false);
            vehicleController.SetVehicleCollisionForPlayer(false, LFCUtilities.LocalPlayer);
        }
        foreach (Renderer renderer in entity.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer != null && renderer.enabled)
            {
                renderer.enabled = false;
                _ = state.disabledRenderers.Add(renderer);
            }
        }
        foreach (Light light in entity.GetComponentsInChildren<Light>(true))
        {
            if (light != null && light.enabled)
            {
                light.enabled = false;
                _ = state.disabledLights.Add(light);
            }
        }
        foreach (Rigidbody rigidbody in entity.GetComponentsInChildren<Rigidbody>(true))
        {
            if (rigidbody != null && rigidbody.useGravity)
            {
                rigidbody.useGravity = false;
                _ = state.disabledGravitys.Add(rigidbody);
            }
        }
        foreach (PlayerPhysicsRegion playerPhysicsRegion in entity.GetComponentsInChildren<PlayerPhysicsRegion>(true))
        {
            if (playerPhysicsRegion != null && !playerPhysicsRegion.disablePhysicsRegion)
            {
                playerPhysicsRegion.disablePhysicsRegion = true;
                _ = state.disabledPhysicsRegions.Add(playerPhysicsRegion);
            }
        }
        foreach (Collider collider in entity.GetComponentsInChildren<Collider>(true))
        {
            if (collider != null && collider.enabled)
            {
                collider.enabled = false;
                _ = state.disabledColliders.Add(collider);
            }
        }
        foreach (TextMeshProUGUI textMesh in entity.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (textMesh != null && textMesh.enabled)
            {
                textMesh.enabled = false;
                _ = state.disabledTextMeshes.Add(textMesh);
            }
        }
        foreach (DecalProjector decal in entity.GetComponentsInChildren<DecalProjector>(true))
        {
            if (decal != null && decal.enabled)
            {
                decal.enabled = false;
                _ = state.disabledDecals.Add(decal);
            }
        }
        foreach (InteractTrigger interactTrigger in entity.GetComponentsInChildren<InteractTrigger>(true))
        {
            if (interactTrigger != null && interactTrigger.interactable)
            {
                interactTrigger.interactable = false;
                _ = state.disabledInteractTriggers.Add(interactTrigger);
            }
        }
        foreach (ParticleSystem particle in entity.GetComponentsInChildren<ParticleSystem>(true))
        {
            if (particle != null && particle.isPlaying)
            {
                particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                _ = state.disabledParticles.Add(particle);
            }
        }
        foreach (AudioSource audioSource in entity.GetComponentsInChildren<AudioSource>(true))
        {
            if (audioSource != null && audioSource.volume > 0f)
            {
                state.audioVolumes[audioSource] = audioSource.volume;
                audioSource.volume = 0f;
            }
        }
    }

    private static void RestoreInternal(GameObject entity, EntityState state)
    {
        if (LFCUtilities.LocalPlayer != null && entity.TryGetComponentInParent(out VehicleController vehicleController))
        {
            if (!vehicleController.carDestroyed)
            {
                vehicleController.startKeyIgnitionTrigger.SetActive(true);
                vehicleController.removeKeyIgnitionTrigger.SetActive(true);
                vehicleController.backDoorContainer.SetActive(true);
                vehicleController.backLightsContainer.SetActive(true);
                vehicleController.frontCabinLightContainer.SetActive(true);
                vehicleController.headlightsContainer.SetActive(true);
            }
            else
            {
                vehicleController.destroyedTruckMesh.SetActive(true);
            }
            vehicleController.SetVehicleCollisionForPlayer(true, LFCUtilities.LocalPlayer);
        }
        foreach (Renderer renderer in state.disabledRenderers)
            if (renderer != null) renderer.enabled = true;
        foreach (Light light in state.disabledLights)
            if (light != null) { light.enabled = true; CullFactorySoftCompat.RefreshLightPosition(light); }
        foreach (Collider collider in state.disabledColliders)
            if (collider != null) collider.enabled = true;
        foreach (PlayerPhysicsRegion playerPhysicsRegion in state.disabledPhysicsRegions)
            if (playerPhysicsRegion != null) playerPhysicsRegion.disablePhysicsRegion = false;
        foreach (Rigidbody rigidbody in state.disabledGravitys)
            if (rigidbody != null) rigidbody.useGravity = true;
        foreach (TextMeshProUGUI textMesh in state.disabledTextMeshes)
            if (textMesh != null) textMesh.enabled = true;
        foreach (DecalProjector decal in state.disabledDecals)
            if (decal != null) decal.enabled = true;
        foreach (InteractTrigger interactTrigger in state.disabledInteractTriggers)
            if (interactTrigger != null) interactTrigger.interactable = true;
        foreach (ParticleSystem particle in state.disabledParticles)
            if (particle != null && particle.gameObject != null) particle.Play(true);
        foreach (KeyValuePair<AudioSource, float> kv in state.audioVolumes)
            if (kv.Key != null) kv.Key.volume = kv.Value;
        if (entity.TryGetComponentInParent(out GrabbableObject grabbableObject))
            CullFactorySoftCompat.RefreshGrabbableObjectPosition(grabbableObject);

        state.tags.Clear();
        _ = VisibilityStates.Remove(entity);
    }

    private static void CleanupDeadEntries()
    {
        List<GameObject> toRemove = null;

        foreach (KeyValuePair<GameObject, EntityState> kv in VisibilityStates)
        {
            if (kv.Key == null)
            {
                toRemove ??= [];
                toRemove.Add(kv.Key);
            }
        }

        if (toRemove == null) return;

        foreach (GameObject gameObject in toRemove)
            _ = VisibilityStates.Remove(gameObject);
    }
}
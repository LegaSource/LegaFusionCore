using LegaFusionCore.Registries;
using UnityEngine;

namespace LegaFusionCore.Managers;

public static class LFCGlobalManager
{
    public static void PlayParticle(string tag, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = LFCPrefabRegistry.GetPrefab(tag);
        if (prefab == null)
        {
            LegaFusionCore.mls.LogWarning($"[PlayParticle] No prefab found for the tag: {tag}");
            return;
        }

        GameObject particleObject = Object.Instantiate(prefab, position, rotation);
        particleObject.SetActive(true);

        ParticleSystem particleSystem = particleObject.GetComponent<ParticleSystem>();
        if (!particleSystem.main.playOnAwake) particleSystem.Play();

        Object.Destroy(particleObject, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax);
    }

    public static void PlayAudio(string tag, Vector3 position)
    {
        GameObject prefab = LFCPrefabRegistry.GetPrefab(tag);
        if (prefab == null)
        {
            LegaFusionCore.mls.LogWarning($"[PlayAudio] No prefab found for the tag: {tag}");
            return;
        }

        GameObject audioObject = Object.Instantiate(prefab, position, Quaternion.identity);
        AudioSource audioSource = audioObject.GetComponent<AudioSource>();
        if (audioSource == null || audioSource.clip == null)
        {
            LegaFusionCore.mls.LogWarning($"[PlayAudio] The prefab {tag} does not have an AudioSource or clip assigned to it.");
            Object.Destroy(audioObject);
            return;
        }

        if (!audioSource.playOnAwake) audioSource.Play();
        Object.Destroy(audioObject, audioSource.clip.length);
    }
}

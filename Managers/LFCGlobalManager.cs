using LegaFusionCore.Registries;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace LegaFusionCore.Managers;

public static class LFCGlobalManager
{
    public static void PlayParticle(string tag, Vector3 position, Quaternion rotation, float scaleFactor = 1f)
    {
        GameObject prefab = LFCPrefabRegistry.GetPrefab(tag);
        if (prefab == null)
        {
            LegaFusionCore.mls.LogWarning($"[PlayParticle] No prefab found for the tag: {tag}");
            return;
        }

        GameObject particleObject = Object.Instantiate(prefab, position, rotation);
        particleObject.transform.localScale = prefab.transform.localScale * scaleFactor;
        particleObject.SetActive(true);

        ParticleSystem particleSystem = particleObject.GetComponent<ParticleSystem>();
        MainModule main = particleSystem.main;

        if (scaleFactor != 1f)
        {
            // Taille des particules
            main.startSizeMultiplier *= scaleFactor;
            main.startSpeedMultiplier *= scaleFactor;

            // Nombre de particules selon la taille
            EmissionModule emission = particleSystem.emission;
            if (emission.burstCount > 0)
            {
                for (int i = 0; i < emission.burstCount; i++)
                {
                    Burst burst = emission.GetBurst(i);
                    burst.count = new MinMaxCurve(burst.count.constant * scaleFactor);
                    emission.SetBurst(i, burst);
                }
            }
            emission.rateOverTimeMultiplier *= scaleFactor;

            // Limite de particules
            main.maxParticles = Mathf.RoundToInt(main.maxParticles * scaleFactor);
        }

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

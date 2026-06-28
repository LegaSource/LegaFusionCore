using LegaFusionCore.Registries;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace LegaFusionCore.Managers;

public static class LFCGlobalManager
{
    public static void PlayParticle(string tag, Vector3 position, Quaternion rotation, bool scaleMain = true, float scaleFactor = 1f, bool active = true)
    {
        GameObject prefab = LFCPrefabRegistry.GetPrefab(tag);
        if (prefab == null)
        {
            LegaFusionCore.mls.LogWarning($"[PlayParticle] No prefab found for the tag: {tag}");
            return;
        }
        PlayParticle(prefab, position, rotation, scaleMain, scaleFactor, active);
    }

    public static void PlayParticle(GameObject prefab, Vector3 position, Quaternion rotation, bool scaleMain = true, float scaleFactor = 1f, bool active = true)
    {
        GameObject particleObject = Object.Instantiate(prefab, position, rotation);
        particleObject.transform.localScale = prefab.transform.localScale * scaleFactor;
        particleObject.SetActive(active);

        ParticleSystem particleSystem = particleObject.GetComponent<ParticleSystem>();
        if (particleSystem == null)
        {
            LegaFusionCore.mls.LogWarning($"[PlayParticle] Prefab {prefab.name} has no ParticleSystem.");
            Object.Destroy(particleObject);
            return;
        }
        MainModule main = particleSystem.main;

        if (scaleMain && scaleFactor != 1f)
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

        if (!particleSystem.main.playOnAwake)
            particleSystem.Play();

        Object.Destroy(particleObject, main.duration);
    }

    public static void PlayAudio(string tag, Vector3 position, bool active = true)
    {
        GameObject prefab = LFCPrefabRegistry.GetPrefab(tag);
        if (prefab == null)
        {
            LegaFusionCore.mls.LogWarning($"[PlayAudio] No prefab found for the tag: {tag}");
            return;
        }
        PlayAudio(prefab, position, active);
    }

    public static void PlayAudio(GameObject prefab, Vector3 position, bool active = true)
    {
        GameObject audioObject = Object.Instantiate(prefab, position, Quaternion.identity);
        AudioSource audioSource = audioObject.GetComponent<AudioSource>();
        if (audioSource == null || audioSource.clip == null)
        {
            LegaFusionCore.mls.LogWarning($"[PlayAudio] Prefab {prefab.name} has no AudioSource or clip.");
            Object.Destroy(audioObject);
            return;
        }
        audioObject.SetActive(active);

        if (!audioSource.playOnAwake)
            audioSource.Play();

        Object.Destroy(audioObject, audioSource.clip.length);
    }
}

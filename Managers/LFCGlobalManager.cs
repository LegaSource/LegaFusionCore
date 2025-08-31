using LegaFusionCore.Registries;
using UnityEngine;

namespace LegaFusionCore.Managers;

public static class LFCGlobalManager
{
    public static void PlayParticle(string tag, Vector3 position, Quaternion rotation)
    {
        GameObject particleObject = Object.Instantiate(LFCPrefabRegistry.GetPrefab(tag), position, rotation);
        particleObject.SetActive(true);

        ParticleSystem particleSystem = particleObject.GetComponent<ParticleSystem>();
        if (!particleSystem.main.playOnAwake) particleSystem.Play();

        Object.Destroy(particleObject, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax);
    }
}

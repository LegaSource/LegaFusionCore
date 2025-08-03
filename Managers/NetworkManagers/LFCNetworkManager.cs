using LegaFusionCore.Registries;
using Unity.Netcode;
using UnityEngine;

namespace LegaFusionCore.Managers.NetworkManagers;

public partial class LFCNetworkManager : NetworkBehaviour
{
    public static LFCNetworkManager Instance;

    public void Awake()
        => Instance = this;

    [ClientRpc]
    public void PlayParticleClientRpc(string tag, Vector3 position, Quaternion rotation)
    {
        GameObject particleObject = Instantiate(LFCPrefabRegistry.GetPrefab(tag), position, rotation);
        ParticleSystem particleSystem = particleObject.GetComponent<ParticleSystem>();
        Destroy(particleObject, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax);
    }
}

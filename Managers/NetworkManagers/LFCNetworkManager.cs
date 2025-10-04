using Unity.Netcode;
using UnityEngine;

namespace LegaFusionCore.Managers.NetworkManagers;

public partial class LFCNetworkManager : NetworkBehaviour
{
    public static LFCNetworkManager Instance;

    public void Awake() => Instance = this;

    [ClientRpc]
    public void PlayParticleClientRpc(string tag, Vector3 position, Quaternion rotation) => LFCGlobalManager.PlayParticle(tag, position, rotation);
}

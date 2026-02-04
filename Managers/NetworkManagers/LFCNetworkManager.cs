using Unity.Netcode;
using UnityEngine;

namespace LegaFusionCore.Managers.NetworkManagers;

public partial class LFCNetworkManager : NetworkBehaviour
{
    public static LFCNetworkManager Instance;

    public void Awake() => Instance = this;

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void PlayParticleEveryoneRpc(string tag, Vector3 position, Quaternion rotation, float scaleFactor = 1f, bool active = true)
        => LFCGlobalManager.PlayParticle(tag, position, rotation, scaleFactor, active);

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void PlayAudioEveryoneRpc(string tag, Vector3 position, bool active = true) => LFCGlobalManager.PlayAudio(tag, position, active);
}

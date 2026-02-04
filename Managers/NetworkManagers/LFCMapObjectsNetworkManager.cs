using GameNetcodeStuff;
using Unity.Netcode;

namespace LegaFusionCore.Managers.NetworkManagers;

public partial class LFCNetworkManager
{
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void AttachMapObjectEveryoneRpc(int playerId, NetworkObjectReference obj)
    {
        if (obj.TryGet(out NetworkObject networkObject))
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            LFCMapObjectsManager.AttachMapObjectForEveryone(player, networkObject.gameObject);
        }
    }
}

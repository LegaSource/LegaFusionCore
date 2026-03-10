using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace LegaFusionCore.Behaviours;

public class LFCDecalMaskModfier : MonoBehaviour
{
    public DecalProjector decalProjector;
    public LayerMask includeLayer;
    private readonly Collider[] overlapColliders = new Collider[512];

    public void Start()
    {
        int count = Physics.OverlapBoxNonAlloc(decalProjector.transform.position, decalProjector.size, overlapColliders, decalProjector.transform.rotation, includeLayer, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            Collider collider = overlapColliders[i];
            if (collider != null)
            {
                foreach (Renderer renderer in collider.GetComponentsInParent<Renderer>())
                {
                    if (renderer != null)
                        renderer.renderingLayerMask |= ConvertToRendererDecalMask();
                }
            }
        }
    }

    public uint ConvertToRendererDecalMask()
    {
        uint raw = (uint)decalProjector.decalLayerMask;
        return raw != 0 && raw <= 255 ? raw << 8 : raw;
    }
}

using UnityEngine;
public class QRPlacementTracker : MonoBehaviour
{
    private void Update()
    {
#if META_BUILD

        ColocationManager.Instance.SetMetaLocalTransform(
            transform.position,
            transform.rotation
        );

#elif XREAL_BUILD

        ColocationManager.Instance.SetXrealLocalTransform(
            transform.position,
            transform.rotation
        );

#endif
    }
}
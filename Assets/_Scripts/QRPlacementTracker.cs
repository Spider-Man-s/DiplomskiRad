using UnityEngine;
public class QRPlacementTracker : MonoBehaviour
{
    private void Update()
    {
        Vector3 right =
            transform.rotation * Vector3.right;

        Vector3 flatRight =
            Vector3.ProjectOnPlane(
                right,
                Vector3.up
            ).normalized;

        float projectedZ =
            Mathf.Atan2(
                flatRight.z,
                flatRight.x
            ) * Mathf.Rad2Deg;

#if META_BUILD

        ColocationManager.Instance.SetMetaLocalTransform(
            transform.position,
            transform.rotation,
            projectedZ
        );

#elif XREAL_BUILD

        ColocationManager.Instance.SetXrealLocalTransform(
            transform.position,
            transform.rotation,
            projectedZ
        );

#endif
    }
}
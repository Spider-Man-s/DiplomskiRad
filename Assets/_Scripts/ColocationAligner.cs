using Fusion;
using UnityEngine;

public class ColocationAligner : NetworkBehaviour
{
    [Header("XR Origin Root")]
    [SerializeField]
    private Transform xrOrigin;

    private Transform localMarker;

    [Networked]
    private Vector3 MetaMarkerPosition { get; set; }

    [Networked]
    private Quaternion MetaMarkerRotation { get; set; }

    [Networked]
    private Vector3 XrealMarkerPosition { get; set; }

    [Networked]
    private Quaternion XrealMarkerRotation { get; set; }

    [Networked]
    private bool MetaReady { get; set; }

    [Networked]
    private bool XrealReady { get; set; }

    private bool aligned;

    private void Update()
    {
        if (aligned)
            return;

        if (localMarker == null)
            return;

#if META_BUILD

        HandleMetaTracking();

#elif XREAL_BUILD

        HandleXrealTracking();

#endif

        if (!MetaReady || !XrealReady)
            return;

#if META_BUILD

        aligned = true;
        Debug.Log("Meta alignment complete.");

#elif XREAL_BUILD

        AlignXrealToMeta();

        aligned = true;

        Debug.Log("XREAL aligned to Meta.");

#endif
    }

#if META_BUILD

    private void HandleMetaTracking()
    {
        if (MetaReady)
            return;

        MetaMarkerPosition = localMarker.position;
        MetaMarkerRotation = localMarker.rotation;

        MetaReady = true;

        Debug.Log($"Meta marker locked at {MetaMarkerPosition}");
    }

#endif

#if XREAL_BUILD

    private void HandleXrealTracking()
    {
        if (XrealReady)
            return;

        XrealMarkerPosition = localMarker.position;
        XrealMarkerRotation = localMarker.rotation;

        XrealReady = true;

        Debug.Log($"XREAL marker locked at {XrealMarkerPosition}");
    }

#endif

    private void AlignXrealToMeta()
    {
        // POSITION OFFSET
        Vector3 positionOffset =
            MetaMarkerPosition - XrealMarkerPosition;

        xrOrigin.position += positionOffset;

        // YAW ROTATION ONLY
        Vector3 metaForward =
            Vector3.ProjectOnPlane(
                MetaMarkerRotation * Vector3.forward,
                Vector3.up
            ).normalized;

        Vector3 xrealForward =
            Vector3.ProjectOnPlane(
                XrealMarkerRotation * Vector3.forward,
                Vector3.up
            ).normalized;

        float angle =
            Vector3.SignedAngle(
                xrealForward,
                metaForward,
                Vector3.up
            );

        xrOrigin.RotateAround(
            localMarker.position,
            Vector3.up,
            angle
        );
    }

    // Called by QR/image tracking system
    public void SetLocalMarker(Transform markerTransform)
    {
        if (localMarker != null)
            return;

        localMarker = markerTransform;

        Debug.Log($"Local marker assigned: {markerTransform.position}");
    }
}
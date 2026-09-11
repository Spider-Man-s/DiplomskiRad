using System;
using UnityEngine;

/// <summary>
/// Local-only coordinate conversion service.
///
/// Each device has its own stable tracking/device-origin space created when the XR scene starts.
/// The user aligns the same virtual calibration box to the same physical box. The selected
/// SharedReference transform (bottom-left-front corner by default) is captured relative to that
/// device's tracking space and becomes Shared Space (0,0,0 / identity).
///
/// Nothing in this class is networked. Every device owns a different Local<->Shared transform.
/// Networked spatial state should be expressed in Shared Space.
/// </summary>
public class SharedSpaceManager : MonoBehaviour
{
    public static SharedSpaceManager Instance { get; private set; }

    [Header("Stable local tracking/device origin")]
    [Tooltip("The fixed tracking-space root created when the XR scene starts. Do NOT assign the HMD camera. Leave null if Unity world space itself is the stable tracking space.")]
    [SerializeField] private Transform trackingOrigin;

    public Transform TrackingOrigin => trackingOrigin;
    public bool IsCalibrated { get; private set; }

    /// <summary>
    /// Pose of Shared Space origin, expressed in this device's tracking space.
    /// This is local-only and intentionally different on Meta and XREAL.
    /// </summary>
    public Pose SharedOriginInTrackingSpace { get; private set; } = new Pose(Vector3.zero, Quaternion.identity);

    public event Action Calibrated;
    public event Action CalibrationInvalidated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        WarnIfTrackingOriginHasScale();
    }

    /// <summary>
    /// Capture the world-space pose of the agreed physical/virtual reference and define it as
    /// Shared Space origin (0,0,0) with identity rotation.
    /// </summary>
    public void CalibrateFromWorldReference(Transform sharedReference)
    {
        if (sharedReference == null)
        {
            Debug.LogError("SharedSpaceManager: sharedReference is null.");
            return;
        }

        CalibrateFromWorldReference(new Pose(sharedReference.position, sharedReference.rotation));
    }

    public void CalibrateFromWorldReference(Pose worldReferencePose)
    {
        SharedOriginInTrackingSpace = WorldToTracking(worldReferencePose);
        IsCalibrated = true;

        Debug.Log(
            $"Shared Space calibrated. Local tracking-space reference pose: " +
            $"position={SharedOriginInTrackingSpace.position}, " +
            $"rotation={SharedOriginInTrackingSpace.rotation.eulerAngles}");

        Calibrated?.Invoke();
    }

    /// <summary>
    /// Use this if the XR runtime recenters/relocalizes and changes the stable tracking origin.
    /// The user should align the calibration box again afterwards.
    /// </summary>
    public void InvalidateCalibration()
    {
        if (!IsCalibrated)
            return;

        IsCalibrated = false;
        CalibrationInvalidated?.Invoke();
        Debug.LogWarning("Shared Space calibration invalidated. Recalibration is required.");
    }

    // ---------- World <-> Shared ----------

    public Pose WorldToShared(Pose worldPose)
    {
        EnsureCalibrated();
        return TrackingToShared(WorldToTracking(worldPose));
    }

    public Pose SharedToWorld(Pose sharedPose)
    {
        EnsureCalibrated();
        return TrackingToWorld(SharedToTracking(sharedPose));
    }

    public Vector3 WorldPointToShared(Vector3 worldPoint)
    {
        Pose p = WorldToShared(new Pose(worldPoint, Quaternion.identity));
        return p.position;
    }

    public Vector3 SharedPointToWorld(Vector3 sharedPoint)
    {
        Pose p = SharedToWorld(new Pose(sharedPoint, Quaternion.identity));
        return p.position;
    }

    public Vector3 WorldDirectionToShared(Vector3 worldDirection)
    {
        EnsureCalibrated();

        Vector3 trackingDirection = trackingOrigin != null
            ? Quaternion.Inverse(trackingOrigin.rotation) * worldDirection
            : worldDirection;

        return Quaternion.Inverse(SharedOriginInTrackingSpace.rotation) * trackingDirection;
    }

    public Vector3 SharedDirectionToWorld(Vector3 sharedDirection)
    {
        EnsureCalibrated();

        Vector3 trackingDirection = SharedOriginInTrackingSpace.rotation * sharedDirection;
        return trackingOrigin != null
            ? trackingOrigin.rotation * trackingDirection
            : trackingDirection;
    }

    // ---------- Tracking <-> Shared ----------

    public Pose TrackingToShared(Pose trackingPose)
    {
        EnsureCalibrated();

        Quaternion inverseReferenceRotation = Quaternion.Inverse(SharedOriginInTrackingSpace.rotation);

        Vector3 sharedPosition = inverseReferenceRotation *
                                 (trackingPose.position - SharedOriginInTrackingSpace.position);

        Quaternion sharedRotation = inverseReferenceRotation * trackingPose.rotation;

        return new Pose(sharedPosition, sharedRotation);
    }

    public Pose SharedToTracking(Pose sharedPose)
    {
        EnsureCalibrated();

        Vector3 trackingPosition = SharedOriginInTrackingSpace.position +
                                   SharedOriginInTrackingSpace.rotation * sharedPose.position;

        Quaternion trackingRotation = SharedOriginInTrackingSpace.rotation * sharedPose.rotation;

        return new Pose(trackingPosition, trackingRotation);
    }

    // ---------- World <-> Tracking ----------

    public Pose WorldToTracking(Pose worldPose)
    {
        if (trackingOrigin == null)
            return worldPose;

        Vector3 trackingPosition = trackingOrigin.InverseTransformPoint(worldPose.position);
        Quaternion trackingRotation = Quaternion.Inverse(trackingOrigin.rotation) * worldPose.rotation;
        return new Pose(trackingPosition, trackingRotation);
    }

    public Pose TrackingToWorld(Pose trackingPose)
    {
        if (trackingOrigin == null)
            return trackingPose;

        Vector3 worldPosition = trackingOrigin.TransformPoint(trackingPose.position);
        Quaternion worldRotation = trackingOrigin.rotation * trackingPose.rotation;
        return new Pose(worldPosition, worldRotation);
    }

    private void EnsureCalibrated()
    {
        if (!IsCalibrated)
            throw new InvalidOperationException(
                "SharedSpaceManager is not calibrated. Align and confirm the physical reference box first.");
    }

    private void WarnIfTrackingOriginHasScale()
    {
        if (trackingOrigin == null)
            return;

        Vector3 s = trackingOrigin.lossyScale;
        if (!ApproximatelyOne(s.x) || !ApproximatelyOne(s.y) || !ApproximatelyOne(s.z))
        {
            Debug.LogWarning(
                $"SharedSpaceManager: trackingOrigin has non-unit scale {s}. " +
                "The co-location transform is intended to be rigid (translation + rotation only). " +
                "Use scale (1,1,1) on the tracking origin and its relevant parents.");
        }
    }

    private static bool ApproximatelyOne(float value) => Mathf.Abs(value - 1f) < 0.0001f;
}

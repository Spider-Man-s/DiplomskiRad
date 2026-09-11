using UnityEngine;

/// <summary>
/// Optional debug component. After calibration it verifies that SharedReference maps back to
/// approximately (0,0,0) / identity in Shared Space.
/// </summary>
public class SharedSpaceDebugVerifier : MonoBehaviour
{
    [SerializeField] private Transform sharedReference;

    [ContextMenu("Verify Calibration")]
    public void VerifyCalibration()
    {
        SharedSpaceManager manager = SharedSpaceManager.Instance;
        if (manager == null || !manager.IsCalibrated || sharedReference == null)
        {
            Debug.LogWarning("Cannot verify: manager not calibrated or reference missing.");
            return;
        }

        Pose shared = manager.WorldToShared(new Pose(sharedReference.position, sharedReference.rotation));
        float positionErrorMm = shared.position.magnitude * 1000f;
        float rotationErrorDeg = Quaternion.Angle(shared.rotation, Quaternion.identity);

        Debug.Log(
            $"Shared reference verification: position error={positionErrorMm:F3} mm, " +
            $"rotation error={rotationErrorDeg:F4} deg. " +
            "Immediately after capture these should be effectively zero.");
    }
}

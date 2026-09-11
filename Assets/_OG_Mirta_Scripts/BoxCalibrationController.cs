using UnityEngine;

/// <summary>
/// Manual coarse/fine alignment controller for the virtual calibration box.
///
/// Translation nudges use the calibration box's own local axes.
/// Rotation uses the box center as the pivot, while SharedReference remains at the chosen corner.
/// Wire UI buttons directly to the public methods below.
/// </summary>
public class BoxCalibrationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoxCalibrationRig calibrationRig;
    [SerializeField] private SharedSpaceManager sharedSpaceManager;
    [SerializeField] private ColocationSessionState sessionState;

    [Header("Build/device role")]
    [SerializeField] private ColocationDeviceRole editorFallbackRole = ColocationDeviceRole.Meta;

    [Header("Fine adjustment")]
    [SerializeField, Min(0.0001f)] private float positionStepMeters = 0.005f;
    [SerializeField, Min(0.01f)] private float rotationStepDegrees = 0.5f;

    [Header("After confirm")]
    [SerializeField] private bool hideCalibrationBoxAfterConfirm = true;

    private Pose initialWorldPose;
    private bool initialPoseCaptured;

    public float PositionStepMeters => positionStepMeters;
    public float RotationStepDegrees => rotationStepDegrees;

    private void Start()
    {
        if (calibrationRig == null)
            calibrationRig = GetComponent<BoxCalibrationRig>();

        if (sharedSpaceManager == null)
            sharedSpaceManager = SharedSpaceManager.Instance;

        if (calibrationRig == null)
        {
            Debug.LogError("BoxCalibrationController: BoxCalibrationRig is missing.");
            enabled = false;
            return;
        }

        initialWorldPose = new Pose(calibrationRig.transform.position, calibrationRig.transform.rotation);
        initialPoseCaptured = true;
    }

    // ----- Position nudges along BOX LOCAL axes -----

    public void PositionXPlus()  => NudgePosition(Vector3.right,  positionStepMeters);
    public void PositionXMinus() => NudgePosition(Vector3.right, -positionStepMeters);
    public void PositionYPlus()  => NudgePosition(Vector3.up,     positionStepMeters);
    public void PositionYMinus() => NudgePosition(Vector3.up,    -positionStepMeters);
    public void PositionZPlus()  => NudgePosition(Vector3.forward, positionStepMeters);
    public void PositionZMinus() => NudgePosition(Vector3.forward,-positionStepMeters);

    // ----- Rotation nudges around BOX CENTER -----

    public void RotationXPlus()  => NudgeRotation(Vector3.right,   rotationStepDegrees);
    public void RotationXMinus() => NudgeRotation(Vector3.right,  -rotationStepDegrees);
    public void RotationYPlus()  => NudgeRotation(Vector3.up,      rotationStepDegrees);
    public void RotationYMinus() => NudgeRotation(Vector3.up,     -rotationStepDegrees);
    public void RotationZPlus()  => NudgeRotation(Vector3.forward, rotationStepDegrees);
    public void RotationZMinus() => NudgeRotation(Vector3.forward,-rotationStepDegrees);

    // ----- Precision presets -----

    public void SetCoarseSteps()
    {
        positionStepMeters = 0.010f; // 10 mm
        rotationStepDegrees = 1.0f;
    }

    public void SetMediumSteps()
    {
        positionStepMeters = 0.005f; // 5 mm
        rotationStepDegrees = 0.5f;
    }

    public void SetFineSteps()
    {
        positionStepMeters = 0.001f; // 1 mm
        rotationStepDegrees = 0.1f;
    }

    public void SetPositionStepMeters(float meters)
    {
        positionStepMeters = Mathf.Max(0.0001f, meters);
    }

    public void SetRotationStepDegrees(float degrees)
    {
        rotationStepDegrees = Mathf.Max(0.01f, degrees);
    }

    public void ResetAlignment()
    {
        if (!initialPoseCaptured)
            return;

        calibrationRig.transform.SetPositionAndRotation(initialWorldPose.position, initialWorldPose.rotation);
    }

    /// <summary>
    /// Call after the virtual box is visually aligned with the physical Quest 3 box.
    /// </summary>
    public void ConfirmAlignment()
    {
        if (sharedSpaceManager == null)
            sharedSpaceManager = SharedSpaceManager.Instance;

        if (sharedSpaceManager == null)
        {
            Debug.LogError("BoxCalibrationController: SharedSpaceManager is missing.");
            return;
        }

        Transform reference = calibrationRig.SharedReference;
        if (reference == null)
        {
            Debug.LogError("BoxCalibrationController: SharedReference is missing.");
            return;
        }

        sharedSpaceManager.CalibrateFromWorldReference(reference);

        if (sessionState != null)
            sessionState.ReportLocalCalibration(ResolveDeviceRole());

        if (hideCalibrationBoxAfterConfirm)
            calibrationRig.gameObject.SetActive(false);
    }

    private void NudgePosition(Vector3 localAxis, float amount)
    {
        Transform root = calibrationRig.transform;
        Vector3 worldDirection = root.TransformDirection(localAxis).normalized;
        root.position += worldDirection * amount;
    }

    private void NudgeRotation(Vector3 localAxis, float degrees)
    {
        calibrationRig.transform.Rotate(localAxis, degrees, Space.Self);
    }

    private ColocationDeviceRole ResolveDeviceRole()
    {
#if META_BUILD
        return ColocationDeviceRole.Meta;
#elif XREAL_BUILD
        return ColocationDeviceRole.Xreal;
#else
        return editorFallbackRole;
#endif
    }
}

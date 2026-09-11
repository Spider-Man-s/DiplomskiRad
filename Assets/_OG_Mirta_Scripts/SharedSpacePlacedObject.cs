using UnityEngine;

/// <summary>
/// For scene content whose canonical pose is authored directly in Shared Space and does not need
/// transform replication. It repositions itself locally after calibration.
///
/// Example: a fixed virtual workbench 0.5 m right and 0.2 m above the shared reference corner.
/// </summary>
public class SharedSpacePlacedObject : MonoBehaviour
{
    [SerializeField] private Vector3 sharedPosition;
    [SerializeField] private Vector3 sharedEulerDegrees;

    private SharedSpaceManager manager;

    private void Start()
    {
        manager = SharedSpaceManager.Instance;
        if (manager == null)
        {
            Debug.LogError($"{name}: SharedSpaceManager is missing.");
            return;
        }

        manager.Calibrated += ApplySharedPose;

        if (manager.IsCalibrated)
            ApplySharedPose();
    }

    private void OnDisable()
    {
        if (manager != null)
            manager.Calibrated -= ApplySharedPose;
    }

    public void ApplySharedPose()
    {
        if (manager == null || !manager.IsCalibrated)
            return;

        Pose sharedPose = new Pose(sharedPosition, Quaternion.Euler(sharedEulerDegrees));
        Pose worldPose = manager.SharedToWorld(sharedPose);
        transform.SetPositionAndRotation(worldPose.position, worldPose.rotation);
    }
}

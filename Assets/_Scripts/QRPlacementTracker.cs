using UnityEngine;

public class QRPlacementTracker : MonoBehaviour
{
    public Vector3 CurrentPosition { get; private set; }
    public float CurrentProjectedZ { get; private set; }

    private void Update()
    {
        Vector3 right = transform.rotation * Vector3.right;
        Vector3 flatRight = Vector3.ProjectOnPlane(right, Vector3.up).normalized;
        float projectedZ = Mathf.Atan2(flatRight.z, flatRight.x) * Mathf.Rad2Deg;

        CurrentPosition = transform.position;
        CurrentProjectedZ = projectedZ;
    }
}
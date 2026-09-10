using UnityEngine;
using TMPro;
public class QRPlacementTracker : MonoBehaviour
{
    public Vector3 CurrentPosition { get; private set; }
    public float CurrentProjectedZ { get; private set; }
    [Header("Debug UI")]
    [SerializeField] private TMP_Text debugText;
    private void Update()
    {
        Vector3 right = transform.rotation * Vector3.right;
        Vector3 flatRight = Vector3.ProjectOnPlane(right, Vector3.up).normalized;
        float projectedZ = Mathf.Atan2(flatRight.z, flatRight.x) * Mathf.Rad2Deg;

        CurrentPosition = transform.position;
        CurrentProjectedZ = projectedZ;

        if (debugText != null)
        {
            debugText.text =
                $"QR Pos: ({CurrentPosition.x:F3}, {CurrentPosition.y:F3}, {CurrentPosition.z:F3})\n" +
                $"Projected Z (yaw): {CurrentProjectedZ:F2}°";
        }
    }
}
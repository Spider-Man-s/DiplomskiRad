using UnityEngine;

/// <summary>
/// Defines the canonical calibration-box geometry.
///
/// Assumptions for the CalibrationBoxRoot local axes:
/// +X = physical box right
/// +Y = physical box up
/// +Z = physical box back
/// Therefore the bottom-left-front corner is (-X/2, -Y/2, -Z/2).
///
/// The root transform is the manipulation/rotation pivot and should be at the box center.
/// SharedReference is a child at the bottom-left-front corner and defines Shared Space origin.
/// </summary>
public class BoxCalibrationRig : MonoBehaviour
{
    [Header("MEASURE YOUR REAL BOX")]
    [Tooltip("Exact real-world box dimensions in meters: X=width, Y=height, Z=depth. Do not guess these values.")]
    [SerializeField] private Vector3 boxSizeMeters = new Vector3(0.40f, 0.12f, 0.25f);

    [Header("Shared reference")]
    [SerializeField] private Transform sharedReference;

    [Header("Optional visualization")]
    [SerializeField] private WireframeBoxRenderer wireframeRenderer;

    public Vector3 BoxSizeMeters => boxSizeMeters;
    public Transform SharedReference => sharedReference;

    private void Awake()
    {
        EnsureReferenceExists();
        ApplyGeometry();
    }

    private void OnValidate()
    {
        boxSizeMeters.x = Mathf.Max(0.001f, boxSizeMeters.x);
        boxSizeMeters.y = Mathf.Max(0.001f, boxSizeMeters.y);
        boxSizeMeters.z = Mathf.Max(0.001f, boxSizeMeters.z);

        if (sharedReference != null)
            ApplyGeometry();
    }

    [ContextMenu("Apply Box Geometry")]
    public void ApplyGeometry()
    {
        EnsureReferenceExists();

        // Bottom-left-front corner in the canonical box frame.
        sharedReference.SetParent(transform, false);
        sharedReference.localPosition = new Vector3(
            -boxSizeMeters.x * 0.5f,
            -boxSizeMeters.y * 0.5f,
            -boxSizeMeters.z * 0.5f);
        sharedReference.localRotation = Quaternion.identity;
        sharedReference.localScale = Vector3.one;

        if (wireframeRenderer != null)
        {
            wireframeRenderer.BoxSizeMeters = boxSizeMeters;
            if (Application.isPlaying)
                wireframeRenderer.Rebuild();
        }
    }

    private void EnsureReferenceExists()
    {
        if (sharedReference != null)
            return;

        Transform existing = transform.Find("SharedReference");
        if (existing != null)
        {
            sharedReference = existing;
            return;
        }

        GameObject go = new GameObject("SharedReference");
        sharedReference = go.transform;
        sharedReference.SetParent(transform, false);
    }
}

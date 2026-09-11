using UnityEngine;

/// <summary>
/// Tiny adapter so platform-specific interaction systems only need to invoke UnityEvents.
/// Hook OnGrabStarted to select/grab-start and OnGrabEnded to select/grab-end.
/// </summary>
public class SharedSpaceGrabBridge : MonoBehaviour
{
    [SerializeField] private SharedSpaceNetworkTransform sharedTransform;

    private void Awake()
    {
        if (sharedTransform == null)
            sharedTransform = GetComponent<SharedSpaceNetworkTransform>();
    }

    public void OnGrabStarted()
    {
        if (sharedTransform != null)
            sharedTransform.BeginLocalDrive();
    }

    public void OnGrabEnded()
    {
        if (sharedTransform != null)
            sharedTransform.EndLocalDrive();
    }
}

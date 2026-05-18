using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class OnImageDetected : MonoBehaviour
{

    [SerializeField] ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] GameObject boxToSpawn;

    void OnEnable() => m_TrackedImageManager.trackedImagesChanged += OnChanged;

    void OnDisable() => m_TrackedImageManager.trackedImagesChanged -= OnChanged;

    void OnChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var newImage in eventArgs.added)
        {
            Instantiate(
                boxToSpawn,
                newImage.transform.position,
                newImage.transform.rotation
            );
        }

        foreach (var updatedImage in eventArgs.updated)
        {
            // Handle updated event
        }

        foreach (var removedImage in eventArgs.removed)
        {
            // Handle removed event
        }
    }
}

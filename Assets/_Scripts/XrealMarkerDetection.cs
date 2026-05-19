using UnityEngine;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
public class XrealMarkerDetection : MonoBehaviour
{



    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private GameObject boxPrefab;

    private GameObject spawnedBox;

    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (ARTrackedImage image in args.added)
        {
            HandleMarkerFound(image);
        }

        foreach (ARTrackedImage image in args.updated)
        {
            if (image.trackingState == TrackingState.Tracking)
            {
                HandleMarkerFound(image);
            }
        }

        foreach (ARTrackedImage image in args.removed)
        {
            Debug.Log("Marker removed: " + GetMarkerId(image));

            if (spawnedBox != null)
            {
                spawnedBox.SetActive(false);
            }
        }
    }

    private void HandleMarkerFound(ARTrackedImage image)
    {
        int markerId = GetMarkerId(image);

        Debug.Log("Marker detected. ID: " + markerId);
        Debug.Log("Reference name: " + image.referenceImage.name);
        Debug.Log("Tracking state: " + image.trackingState);
        Debug.Log("Position: " + image.transform.position);

        if (spawnedBox == null)
        {
            spawnedBox = Instantiate(boxPrefab);
        }

        spawnedBox.SetActive(true);
        spawnedBox.transform.position = image.transform.position;
        spawnedBox.transform.rotation = image.transform.rotation;
    }

    private int GetMarkerId(ARTrackedImage image)
    {
        return (int)image.trackableId.subId2 & 0xFF;
    }

}

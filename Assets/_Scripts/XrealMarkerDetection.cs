using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class XrealMarkerDetection : MonoBehaviour
{
    [SerializeField]
    private ARTrackedImageManager trackedImageManager;

    [SerializeField]
    private GameObject ballPrefab;
    private bool locked;

    private readonly Dictionary<string, GameObject> spawnedObjects =
        new Dictionary<string, GameObject>();

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
            CreateMarkerObject(image);
        }

        foreach (ARTrackedImage image in args.updated)
        {
            UpdateMarkerObject(image);
        }

        foreach (ARTrackedImage image in args.removed)
        {
            RemoveMarkerObject(image);
        }
    }

    private void CreateMarkerObject(ARTrackedImage image)
    {
        string id = image.trackableId.ToString();

        if (spawnedObjects.ContainsKey(id))
            return;

        Debug.Log($"Marker detected: {image.referenceImage.name}");
        Debug.Log($"Position: {image.transform.position}");
        Debug.Log($"Rotation: {image.transform.rotation.eulerAngles}");

        GameObject obj = Instantiate(
            ballPrefab,
            image.transform.position - image.transform.up * 0.1f,
            image.transform.rotation
        );

        // This is the important part from the demo
        obj.transform.SetParent(image.transform, false);

        spawnedObjects[id] = obj;
    }

    private void UpdateMarkerObject(ARTrackedImage image)
    {
        if (locked)
            return;

        if (image.trackingState != TrackingState.Tracking)
            return;

        locked = true;
        string id = image.trackableId.ToString();

        if (!spawnedObjects.TryGetValue(id, out GameObject obj))
            return;

        if (image.trackingState == TrackingState.Tracking)
        {
            obj.SetActive(true);

            // Optional explicit updates
            obj.transform.position = image.transform.position -
             image.transform.up * 0.1f;
            obj.transform.rotation = image.transform.rotation;
        }
        else
        {
            obj.SetActive(false);
        }
    }

    private void RemoveMarkerObject(ARTrackedImage image)
    {
        string id = image.trackableId.ToString();

        if (!spawnedObjects.TryGetValue(id, out GameObject obj))
            return;

        Destroy(obj);
        spawnedObjects.Remove(id);

        Debug.Log($"Marker removed: {id}");
    }
}
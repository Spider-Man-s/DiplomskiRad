using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class MetaQRCodes : MonoBehaviour
{
    [SerializeField] private GameObject boxPrefab;
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 0.02f, 0f);

    private readonly Dictionary<MRUKTrackable, GameObject> spawnedObjects = new();

    private IEnumerator Start()
    {
        while (MRUK.Instance == null)
        {
            yield return null;
        }

        MRUK.Instance.SceneSettings.TrackableAdded.AddListener(OnTrackableAdded);
        MRUK.Instance.SceneSettings.TrackableRemoved.AddListener(OnTrackableRemoved);
    }

    private void OnDestroy()
    {
        if (MRUK.Instance == null)
            return;

        MRUK.Instance.SceneSettings.TrackableAdded.RemoveListener(OnTrackableAdded);
        MRUK.Instance.SceneSettings.TrackableRemoved.RemoveListener(OnTrackableRemoved);
    }

    private void OnTrackableAdded(MRUKTrackable trackable)
    {
        if (trackable.TrackableType != OVRAnchor.TrackableType.QRCode)
            return;

        Debug.Log("QR detected: " + trackable.MarkerPayloadString);

        GameObject box = Instantiate(boxPrefab, trackable.transform);

        box.transform.localPosition = localOffset;
        box.transform.localRotation = Quaternion.identity;

        spawnedObjects[trackable] = box;
    }

    private void OnTrackableRemoved(MRUKTrackable trackable)
    {
        if (!spawnedObjects.TryGetValue(trackable, out GameObject obj))
            return;

        Destroy(obj);
        spawnedObjects.Remove(trackable);
    }
}
using System.Collections;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class MetaQRCodes : MonoBehaviour
{
    [SerializeField] private GameObject boxPrefab;
    [SerializeField]
    private ColocationAligner aligner;

    [SerializeField]
    private Vector3 correctionOffset =
        new Vector3(-0.23f, -0.05f, 1.22f);

    private bool spawned;

    private IEnumerator Start()
    {
        while (MRUK.Instance == null)
        {
            yield return null;
        }

        MRUK.Instance.SceneSettings.TrackableAdded.AddListener(OnTrackableAdded);
    }

    private void OnDestroy()
    {
        if (MRUK.Instance == null)
            return;

        MRUK.Instance.SceneSettings.TrackableAdded.RemoveListener(OnTrackableAdded);
    }

    private void OnTrackableAdded(MRUKTrackable trackable)
    {
        if (spawned)
            return;

        if (trackable.TrackableType != OVRAnchor.TrackableType.QRCode)
            return;

        Debug.Log("QR detected: " + trackable.MarkerPayloadString);

        Vector3 correctedPosition =
            trackable.transform.position + correctionOffset;

        Quaternion qrRotation =
            trackable.transform.rotation;

        Debug.Log($"Raw QR Position: {trackable.transform.position}");
        Debug.Log($"Corrected Position: {correctedPosition}");

        GameObject obj = Instantiate(
            boxPrefab,
            correctedPosition,
            qrRotation
        );

        aligner.SetLocalMarker(obj.transform);
        spawned = true;
    }

    private void Update()
    {
        foreach (var trackable in FindObjectsOfType<MRUKTrackable>())
        {
            if (trackable.TrackableType != OVRAnchor.TrackableType.QRCode)
                continue;

            Debug.DrawRay(
                trackable.transform.position,
                trackable.transform.up * 0.2f,
                Color.green
            );

            Debug.DrawRay(
                trackable.transform.position,
                trackable.transform.forward * 0.2f,
                Color.blue
            );

            Debug.DrawRay(
                trackable.transform.position,
                trackable.transform.right * 0.2f,
                Color.red
            );
        }
    }
}
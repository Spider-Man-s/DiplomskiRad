using Fusion;
using UnityEngine;

public class ManualColocationAligner : NetworkBehaviour
{
    [Header("XR Origin")]
    [SerializeField]
    private Transform xrOrigin;

    [Header("Scene Placement Object Name")]
    [SerializeField]
    private string placementObjectName = "QR_Code";

    [Header("Table")]
    [SerializeField]
    private NetworkObject tablePrefab;

    [SerializeField]
    private Vector3 tableOffset =
        new Vector3(0.662f, 0.161f, 0.183f);

    [SerializeField]
    private Vector3 tableRotation;

    private bool localConfirmed;
    private bool aligned;
    private bool tableSpawned;

    public void ConfirmPlacement()
    {
        if (localConfirmed)
            return;

        GameObject placementObject =
            GameObject.Find(placementObjectName);

        if (placementObject == null)
        {
            Debug.LogError(
                $"Could not find object: {placementObjectName}"
            );

            return;
        }

        Transform t = placementObject.transform;

#if META_BUILD

        ColocationManager.Instance.SubmitMeta(
            t.position,
            t.rotation
        );

        Debug.Log($"META LOCAL: {t.position}");

#elif XREAL_BUILD

        ColocationManager.Instance.SubmitXreal(
            t.position,
            t.rotation
        );

        Debug.Log($"XREAL LOCAL: {t.position}");

#endif

        localConfirmed = true;
    }

    private void Update()
    {
        if (aligned)
            return;

        if (!localConfirmed)
            return;

        if (ColocationManager.Instance == null)
            return;

        Debug.Log(
            $"MetaReady={ColocationManager.Instance.MetaReady} " +
            $"XrealReady={ColocationManager.Instance.XrealReady}"
        );

        if (!ColocationManager.Instance.MetaReady ||
            !ColocationManager.Instance.XrealReady)
            return;

        Debug.Log("BOTH PLAYERS READY");

#if META_BUILD

        aligned = true;

        Debug.Log("META alignment complete.");

        SpawnSharedTable();

#elif XREAL_BUILD

        AlignXrealToMeta();

        aligned = true;

        Debug.Log("XREAL aligned to META.");

#endif
    }

    private void AlignXrealToMeta()
    {
        Vector3 metaPosition =
            ColocationManager.Instance.MetaPosition;

        Quaternion metaRotation =
            ColocationManager.Instance.MetaRotation;

        Vector3 xrealPosition =
            ColocationManager.Instance.XrealPosition;

        Quaternion xrealRotation =
            ColocationManager.Instance.XrealRotation;

        Vector3 positionOffset =
            metaPosition - xrealPosition;

        xrOrigin.position += positionOffset;

        Vector3 metaForward =
            Vector3.ProjectOnPlane(
                metaRotation * Vector3.forward,
                Vector3.up
            ).normalized;

        Vector3 xrealForward =
            Vector3.ProjectOnPlane(
                xrealRotation * Vector3.forward,
                Vector3.up
            ).normalized;

        float angle =
            Vector3.SignedAngle(
                xrealForward,
                metaForward,
                Vector3.up
            );

        xrOrigin.RotateAround(
            Vector3.zero,
            Vector3.up,
            angle
        );

        Debug.Log($"OFFSET: {positionOffset}");
        Debug.Log($"ANGLE: {angle}");
    }

    private void SpawnSharedTable()
    {
        if (tableSpawned)
            return;

        Vector3 metaPosition =
            ColocationManager.Instance.MetaPosition;

        Quaternion metaRotation =
            ColocationManager.Instance.MetaRotation;

        Vector3 forward =
            Vector3.ProjectOnPlane(
                metaRotation * Vector3.forward,
                Vector3.up
            ).normalized;

        Quaternion flatRotation =
            Quaternion.LookRotation(
                forward,
                Vector3.up
            );

        Vector3 worldPosition =
            metaPosition +
            flatRotation * tableOffset;

        Quaternion worldRotation =
            Quaternion.Euler(tableRotation);

        Runner.Spawn(
            tablePrefab,
            worldPosition,
            worldRotation
        );

        tableSpawned = true;

        Debug.Log(
            $"TABLE SPAWNED AT: {worldPosition}"
        );
    }
}
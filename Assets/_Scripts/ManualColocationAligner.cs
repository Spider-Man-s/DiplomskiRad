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

    // SHARED DATA
    private Vector3 metaPosition;
    private Quaternion metaRotation;
    private bool metaReady;

    private Vector3 xrealPosition;
    private Quaternion xrealRotation;
    private bool xrealReady;

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

        localConfirmed = true;

#if META_BUILD

        metaPosition = t.position;
        metaRotation = t.rotation;
        metaReady = true;

        Debug.Log($"META LOCAL: {metaPosition}");

        RPC_SendPlacement(
            metaPosition,
            metaRotation,
            true
        );

#elif XREAL_BUILD

        xrealPosition = t.position;
        xrealRotation = t.rotation;
        xrealReady = true;

        Debug.Log($"XREAL LOCAL: {xrealPosition}");

        RPC_SendPlacement(
            xrealPosition,
            xrealRotation,
            false
        );

#endif
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_SendPlacement(
        Vector3 position,
        Quaternion rotation,
        bool isMeta
    )
    {
        if (isMeta)
        {
            metaPosition = position;
            metaRotation = rotation;
            metaReady = true;

            Debug.Log($"RECEIVED META: {metaPosition}");
        }
        else
        {
            xrealPosition = position;
            xrealRotation = rotation;
            xrealReady = true;

            Debug.Log($"RECEIVED XREAL: {xrealPosition}");
        }
    }

    private void Update()
    {
        if (aligned)
            return;

        if (!metaReady || !xrealReady)
            return;

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
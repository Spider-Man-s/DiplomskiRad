using Fusion;
using UnityEngine;

public class ManualColocationAligner : NetworkBehaviour
{
    [Header("XR Origin")]
    [SerializeField]
    private Transform xrOrigin;

    [Header("Scene Placement Object Name")]
    [SerializeField]
    private string placementObjectName = "QRCodePlacement";

    [Header("Table")]
    [SerializeField]
    private NetworkObject tablePrefab;

    [SerializeField]
    private Vector3 tableOffset =
        new Vector3(0.662f, 0.161f, 0.183f);

    [SerializeField]
    private Vector3 tableRotation;

    [Networked]
    private Vector3 MetaPlacementPosition { get; set; }

    [Networked]
    private Quaternion MetaPlacementRotation { get; set; }

    [Networked]
    private bool MetaPlacementReady { get; set; }

    [Networked]
    private Vector3 XrealPlacementPosition { get; set; }

    [Networked]
    private Quaternion XrealPlacementRotation { get; set; }

    [Networked]
    private bool XrealPlacementReady { get; set; }

    private bool localConfirmed;
    private bool aligned;
    private bool tableSpawned;

    // Called by UI button
    public void ConfirmPlacement()
    {
        if (localConfirmed)
            return;

        GameObject placementObject =
            GameObject.Find(placementObjectName);

        if (placementObject == null)
        {
            Debug.LogError(
                $"Could not find object named: {placementObjectName}"
            );

            return;
        }

        Transform placementTransform =
            placementObject.transform;

        localConfirmed = true;

#if META_BUILD

        MetaPlacementPosition = placementTransform.position;
        MetaPlacementRotation = placementTransform.rotation;
        MetaPlacementReady = true;

        Debug.Log(
            $"META placement locked: {MetaPlacementPosition}"
        );

#elif XREAL_BUILD

        XrealPlacementPosition = placementTransform.position;
        XrealPlacementRotation = placementTransform.rotation;
        XrealPlacementReady = true;

        Debug.Log(
            $"XREAL placement locked: {XrealPlacementPosition}"
        );

#endif
    }

    private void Update()
    {
        if (aligned)
            return;

        if (!localConfirmed)
            return;

        if (!MetaPlacementReady || !XrealPlacementReady)
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
            MetaPlacementPosition - XrealPlacementPosition;

        xrOrigin.position += positionOffset;

        Vector3 metaForward =
            Vector3.ProjectOnPlane(
                MetaPlacementRotation * Vector3.forward,
                Vector3.up
            ).normalized;

        Vector3 xrealForward =
            Vector3.ProjectOnPlane(
                XrealPlacementRotation * Vector3.forward,
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

        Debug.Log(
            $"Applied position offset: {positionOffset}"
        );

        Debug.Log(
            $"Applied yaw correction: {angle}"
        );
    }

    private void SpawnSharedTable()
    {
        if (tableSpawned)
            return;

        Vector3 forward =
            Vector3.ProjectOnPlane(
                MetaPlacementRotation * Vector3.forward,
                Vector3.up
            ).normalized;

        Quaternion flatRotation =
            Quaternion.LookRotation(
                forward,
                Vector3.up
            );

        Vector3 worldPosition =
            MetaPlacementPosition +
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
            $"Spawned shared table at {worldPosition}"
        );
    }
}
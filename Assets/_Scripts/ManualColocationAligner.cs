using Fusion;
using UnityEngine;

public class ManualColocationAligner : NetworkBehaviour
{
    [Header("XR Origin")]
    [SerializeField]
    private Transform xrOrigin;

    [Header("Table")]
    [SerializeField]
    private NetworkObject tablePrefab;

    [SerializeField]
    private Vector3 tableOffset =
        new Vector3(0.662f, 0.162f, 0.184f);

    [SerializeField]
    private Vector3 tableRotation;

    private bool localConfirmed;
    private bool aligned;
    private bool tableSpawned;

    public void ConfirmPlacement()
    {
        if (localConfirmed)
            return;

        localConfirmed = true;

#if META_BUILD

        ColocationManager.Instance.ConfirmMeta();

#elif XREAL_BUILD

        ColocationManager.Instance.ConfirmXreal();

#endif

        Debug.Log("Local placement confirmed.");
    }

    private void Update()
    {
        if (aligned)
            return;

        if (!localConfirmed)
            return;

        if (ColocationManager.Instance == null)
            return;

        if (!ColocationManager.Instance.MetaReady ||
            !ColocationManager.Instance.XrealReady)
            return;

        Debug.Log("Both players confirmed.");

#if META_BUILD

        aligned = true;

        SpawnSharedTable();

        Debug.Log("META alignment complete.");

#elif XREAL_BUILD

        AlignXrealToMeta();

        aligned = true;

        Debug.Log("XREAL aligned to META.");

#endif
    }

    private void AlignXrealToMeta()
    {
        Vector3 metaQRPos =
            ColocationManager.Instance.MetaPosition;

        Quaternion metaQRRot =
            ColocationManager.Instance.MetaRotation;

        Vector3 xrealQRPos =
            ColocationManager.Instance.XrealPosition;

        Quaternion xrealQRRot =
            ColocationManager.Instance.XrealRotation;

        Quaternion rotationDelta =
            metaQRRot *
            Quaternion.Inverse(xrealQRRot);

        Vector3 xrOriginOffset =
            xrOrigin.position - xrealQRPos;

        xrOriginOffset =
            rotationDelta * xrOriginOffset;

        xrOrigin.rotation =
            rotationDelta * xrOrigin.rotation;

        xrOrigin.position =
            metaQRPos + xrOriginOffset;

        Debug.Log($"META QR: {metaQRPos}");
        Debug.Log($"XREAL QR: {xrealQRPos}");
        Debug.Log($"NEW XR ORIGIN: {xrOrigin.position}");
    }

    public void SpawnSharedTable()
    {


        Vector3 qrPosition =
            ColocationManager.Instance.MetaPosition;

        Quaternion qrRotation =
            ColocationManager.Instance.MetaRotation;

        // Get QR red axis
        Vector3 right =
            qrRotation * Vector3.right;

        // Project it onto the floor
        Vector3 flatRight =
            Vector3.ProjectOnPlane(
                right,
                Vector3.up
            ).normalized;

        // Convert projected red vector into angle
        float projectedZ =
            Mathf.Atan2(
                flatRight.z,
                flatRight.x
            ) * Mathf.Rad2Deg;

        // Table stays flat, only Z changes
        Quaternion tableRotation =
            Quaternion.Euler(
                90f,
                0f,
                projectedZ
            );

        Runner.Spawn(
            tablePrefab,
            qrPosition,
            tableRotation
        );

        tableSpawned = true;

        Debug.DrawRay(
            qrPosition,
            flatRight,
            Color.red,
            5f
        );

        Debug.Log($"QR Position: {qrPosition}");
        Debug.Log($"Projected Z: {projectedZ}");
    }
}
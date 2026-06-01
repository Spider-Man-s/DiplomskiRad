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
    private NetworkObject boxPartPrefab;

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

      //  AlignXrealToMeta();

        aligned = true;

        Debug.Log("XREAL aligned to META.");

#endif
    }

    private void AlignXrealToMeta()
    {
        Vector3 metaQRPos =
            ColocationManager.Instance.MetaPosition;

        Vector3 xrealQRPos =
            ColocationManager.Instance.XrealPosition;

        float metaZ =
            ColocationManager.Instance.MetaProjectedZ;

        float xrealZ =
            ColocationManager.Instance.XrealProjectedZ;

        float rotationOffset =
            metaZ - xrealZ;

        // Rotate XR Origin around its QR
        xrOrigin.RotateAround(
            xrealQRPos,
            Vector3.up,
            rotationOffset
        );

        // Re-read QR position after rotation
        xrealQRPos =
            ColocationManager.Instance.XrealPosition;

        Vector3 positionOffset =
            metaQRPos - xrealQRPos;

        xrOrigin.position += positionOffset;

        Debug.Log($"Meta QR: {metaQRPos}");
        Debug.Log($"XREAL QR: {xrealQRPos}");
        Debug.Log($"Rotation Offset: {rotationOffset}");
        Debug.Log($"Position Offset: {positionOffset}");
    }

    public void SpawnSharedTable()
    {
        if (tableSpawned)
            return;

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

        NetworkObject table = Runner.Spawn(
             tablePrefab,
             qrPosition,
             tableRotation
         );
        BoxSideSpawn[] spawns =
            table.GetComponentsInChildren<BoxSideSpawn>();

        foreach (BoxSideSpawn spawn in spawns)
        {
            Runner.Spawn(
                boxPartPrefab,
                spawn.transform.position,
                spawn.transform.rotation
            );

            Debug.Log(
                $"Spawned box at {spawn.name}"
            );
            tableSpawned = true;


            Debug.Log($"QR Position: {qrPosition}");
            Debug.Log($"Projected Z: {projectedZ}");
        }
    }
}
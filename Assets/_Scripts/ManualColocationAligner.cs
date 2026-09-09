using Fusion;
using UnityEngine;

public class ManualColocationAligner : NetworkBehaviour
{
    [SerializeField] private Transform xrOrigin;
    [SerializeField] private QRPlacementTracker qrTracker;

    [SerializeField] private NetworkObject tablePrefab;
    [SerializeField] private NetworkObject boxPartPrefab;
    [SerializeField] private Vector3 tableOffset = new Vector3(0.662f, 0.162f, 0.184f);

    private const float TargetYaw = 0f; // must match on both platforms

    private bool localConfirmed;
    private bool aligned;
    private bool tableSpawned;

    public void ConfirmPlacement()
    {
        if (localConfirmed) return;
        localConfirmed = true;

        // Snapshot the pose right now, before it can drift further
        ColocationManager.Instance.CaptureLocalQr(
            qrTracker.CurrentPosition,
            qrTracker.CurrentProjectedZ
        );
        Debug.Log("Local QR placement captured: " +
                  $"Position={qrTracker.CurrentPosition}, ProjectedZ={qrTracker.CurrentProjectedZ}");

#if META_BUILD
        ColocationManager.Instance.ConfirmMeta();
#elif XREAL_BUILD
        ColocationManager.Instance.ConfirmXreal();
#endif

        Debug.Log("Local placement confirmed and QR snapshot captured.");
    }

    private void Update()
    {
        if (aligned) return;
        if (!localConfirmed) return;
        if (ColocationManager.Instance == null) return;
        if (!ColocationManager.Instance.MetaReady || !ColocationManager.Instance.XrealReady) return;

        AlignToSharedFrame();
        aligned = true;

#if META_BUILD
        SpawnSharedTable();
#endif
    }

    private void AlignToSharedFrame()
    {
        Vector3 qrPos = ColocationManager.Instance.LocalQrPosition;
        float qrYaw = ColocationManager.Instance.LocalProjectedZ;

        float rotationOffset = TargetYaw - qrYaw;

        xrOrigin.RotateAround(qrPos, Vector3.up, rotationOffset);

        Vector3 positionOffset = Vector3.zero - qrPos;
        positionOffset.y = 0f; // keep vertical if devices don't share floor calibration; remove if you want Y snapped too
        xrOrigin.position += positionOffset;

        Debug.Log($"QR pos: {qrPos}, yaw: {qrYaw}, rotationOffset: {rotationOffset}, posOffset: {positionOffset}");
    }

    public void SpawnSharedTable()
    {
        if (tableSpawned) return;

        Quaternion tableRotation = Quaternion.Euler(90f, 0f, TargetYaw); //target yaw zamijeniti s kutom qr koda myb
        NetworkObject table = Runner.Spawn(tablePrefab, ColocationManager.Instance.LocalQrPosition + tableOffset, tableRotation);

        BoxSideSpawn[] spawns = table.GetComponentsInChildren<BoxSideSpawn>();
        foreach (BoxSideSpawn spawn in spawns)
        {
            Runner.Spawn(boxPartPrefab, spawn.transform.position, spawn.transform.rotation);
            Debug.Log($"Spawned box at {spawn.name}");
        }
        tableSpawned = true;

        Debug.Log($"Table spawned at fixed offset {tableOffset} from shared origin.");
    }

    public void ReturnToMainMenu()
    {
        FusionBoot.Instance.ReturnToMainMenu();
    }
}
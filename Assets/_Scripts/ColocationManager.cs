using Fusion;
using UnityEngine;

public class ColocationManager : NetworkBehaviour
{
    public static ColocationManager Instance { get; private set; }

    // Local-only — no longer networked, each device only needs its own
    public Vector3 LocalQrPosition { get; private set; }
    public float LocalProjectedZ { get; private set; }
    public bool LocalQrCaptured { get; private set; }

    public bool MetaReady { get; private set; }
    public bool XrealReady { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Called once, at confirm time — see QRPlacementTracker change below
    public void CaptureLocalQr(Vector3 position, float projectedZ)
    {
        LocalQrPosition = position;
        LocalProjectedZ = projectedZ;
        LocalQrCaptured = true;
    }

    public void ConfirmMeta() => RPC_Confirm(true);
    public void ConfirmXreal() => RPC_Confirm(false);

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_Confirm(bool isMeta)
    {
        if (isMeta) MetaReady = true;
        else XrealReady = true;

        Debug.Log($"MetaReady={MetaReady} XrealReady={XrealReady}");
    }
}
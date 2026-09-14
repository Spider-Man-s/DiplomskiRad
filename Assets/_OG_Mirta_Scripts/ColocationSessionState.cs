using Fusion;
using UnityEngine;

public enum ColocationDeviceRole : byte
{
    Meta = 0,
    Xreal = 1
}

/// <summary>
/// Fusion 2 network state used only to tell both peers whether each device has completed its
/// LOCAL calibration. Calibration poses are deliberately NOT networked.
///
/// Put this on one spawned/scene NetworkObject that has State Authority.
/// </summary>
public class ColocationSessionState : NetworkBehaviour
{
    [Networked] public NetworkBool MetaReady { get; private set; }
    [Networked] public NetworkBool XrealReady { get; private set; }

    TableSpawner tableSpawner;

    public bool BothReady => MetaReady && XrealReady;

    private void Start()
    {
        tableSpawner = GetComponent<TableSpawner>();
        if (tableSpawner == null)
            Debug.LogError("TableSpawner component is missing.");

    }

    public void ReportLocalCalibration(ColocationDeviceRole role)
    {
        if (Object == null)
        {
            Debug.LogWarning("ColocationSessionState: NetworkObject is not spawned yet.");
            return;
        }

        if (Object.HasStateAuthority)
        {
            SetReady(role);
        }
        else
        {
            RPC_ReportReady((byte)role);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_ReportReady(byte role)
    {
        SetReady((ColocationDeviceRole)role);
    }

    private void SetReady(ColocationDeviceRole role)
    {
        if (!Object.HasStateAuthority)
            return;

        if (role == ColocationDeviceRole.Meta)
            MetaReady = true;
        else
            XrealReady = true;

        Debug.Log($"Colocation ready state: Meta={MetaReady}, XREAL={XrealReady}");
        Handshake(BothReady); //spawnaj stol ako su oboje ready
    }

    private void Handshake(bool BothReady)
    {
#if META_BUILD
        if (BothReady && tableSpawner!=null)
           tableSpawner.SpawnSharedTable();
#endif
    }
}

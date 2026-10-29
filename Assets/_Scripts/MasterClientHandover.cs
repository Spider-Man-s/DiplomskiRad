using Fusion;
using UnityEngine;

/// <summary>
/// Put on a scene NetworkObject with "Is Master Client Object" ticked in its
/// Shared Mode Settings, so the master always has state authority over it.
/// </summary>
public class MasterClientHandover : NetworkBehaviour
{
    public static MasterClientHandover Instance { get; private set; }

    public override void Spawned() => Instance = this;

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Hook this to your button.</summary>
    public void RequestMaster()
    {
        if (Runner.IsSharedModeMasterClient)
            return; // already master

        RPC_RequestMaster();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestMaster(RpcInfo info = default)
    {
        if (!Runner.IsSharedModeMasterClient)
            return;

        // Auto-approve. To let the master accept/deny, show a UI here instead
        // and call SetMasterClient from the Accept button.
        Runner.SetMasterClient(info.Source);
    }
}
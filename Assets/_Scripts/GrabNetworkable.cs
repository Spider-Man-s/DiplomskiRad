using Fusion;
using UnityEngine;

public class GrabNetworkable : NetworkBehaviour
{
    [Networked] private PlayerRef Grabber { get; set; }
    [Networked] private Vector3 TargetPosition { get; set; }

    [SerializeField] private float followSpeed = 20f;

    // Called by local player when they start grabbing
    public void TryBeginGrab(Vector3 hitPoint)
    {
        if (Runner == null || !Runner.IsRunning)
            return;

        RPC_RequestGrab(Runner.LocalPlayer, hitPoint);
    }

    // Called by local player while dragging
    public void SendDragPosition(Vector3 worldPos)
    {
        if (Runner == null || !Runner.IsRunning)
            return;

        RPC_SendDragPosition(Runner.LocalPlayer, worldPos);
    }

    // Called by local player when they release
    public void EndGrab()
    {
        if (Runner == null || !Runner.IsRunning)
            return;

        RPC_EndGrab(Runner.LocalPlayer);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestGrab(PlayerRef player, Vector3 hitPoint)
    {
        // Host decides who can control it
        if (Grabber == PlayerRef.None)
        {
            Grabber = player;
            TargetPosition = transform.position;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SendDragPosition(PlayerRef player, Vector3 worldPos)
    {
        // Only current grabber may update the target
        if (Grabber == player)
        {
            TargetPosition = worldPos;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_EndGrab(PlayerRef player)
    {
        if (Grabber == player)
        {
            Grabber = PlayerRef.None;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (Grabber != PlayerRef.None)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                TargetPosition,
                followSpeed * Runner.DeltaTime
            );
        }
    }
}
using System;
using Fusion;
using UnityEngine;

/// <summary>
/// Central ready tracker for the session. Lives on a scene NetworkObject.
/// Players who are not in the map count as "not ready".
/// </summary>
public class ReadyManager : NetworkBehaviour, IPlayerLeft
{
    public static ReadyManager Instance { get; private set; }

    /// <summary>Fires on every client when a global recalibration reset happens.</summary>
    public static event Action ResetBroadcast;

    [Networked, Capacity(16)]
    private NetworkDictionary<PlayerRef, NetworkBool> ReadyMap => default;

    public override void Spawned()
    {
        Instance = this;

        var space = SharedSpaceManager.Instance;
        if (space != null)
        {
            space.CalibrationInvalidated += OnLocalCalibrationInvalidated;

            // Calibration was confirmed before this object finished spawning.
            if (space.IsCalibrated)
                SetLocalReady(true);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this)
            Instance = null;

        var space = SharedSpaceManager.Instance;
        if (space != null)
            space.CalibrationInvalidated -= OnLocalCalibrationInvalidated;
    }

    // ---------- Public API ----------

    /// <summary>Marks the local player ready / not ready.</summary>
    public void SetLocalReady(bool ready)
    {
        if (Object.HasStateAuthority)
            Apply(Runner.LocalPlayer, ready);
        else
            RPC_SetReady(ready);
    }

    /// <summary>Clears everyone's ready flag and makes every client invalidate its calibration.</summary>
    public void RequestResetAll()
    {
        if (Object.HasStateAuthority)
            DoReset();
        else
            RPC_RequestReset();
    }

    public bool IsReady(PlayerRef player) => ReadyMap.TryGet(player, out var r) && r;

    public int PlayerCount
    {
        get { int n = 0; foreach (var _ in Runner.ActivePlayers) n++; return n; }
    }

    public int ReadyCount
    {
        get { int n = 0; foreach (var p in Runner.ActivePlayers) if (IsReady(p)) n++; return n; }
    }

    /// <summary>True when at least minPlayers are in the session and every one is ready.</summary>
    public bool AllReady(int minPlayers = 1)
    {
        int count = 0;
        foreach (var p in Runner.ActivePlayers)
        {
            count++;
            if (!IsReady(p))
                return false;
        }
        return count >= Mathf.Max(1, minPlayers);
    }

    // ---------- Internals ----------

    private void OnLocalCalibrationInvalidated() => SetLocalReady(false);

    private void Apply(PlayerRef player, bool ready)
    {
        if (!Object.HasStateAuthority) return;

        ReadyMap.Set(player, ready);
        Debug.Log($"Player {player} ready = {ready} ({ReadyCount}/{PlayerCount})");
    }

    private void DoReset()
    {
        if (!Object.HasStateAuthority) return;

        ReadyMap.Clear();
        RPC_ResetBroadcast();
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (Object.HasStateAuthority)
            ReadyMap.Remove(player);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SetReady(bool ready, RpcInfo info = default) => Apply(info.Source, ready);

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestReset() => DoReset();

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ResetBroadcast()
    {
        var space = SharedSpaceManager.Instance;
        if (space != null)
            space.InvalidateCalibration();

        ResetBroadcast?.Invoke();
    }
}
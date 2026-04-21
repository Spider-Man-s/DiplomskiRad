using System;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FusionBoot : SingletonPersistent<FusionBoot>, INetworkRunnerCallbacks
{
    [Header("Boot")]
    [SerializeField] private GameObject menuRigRoot;
    [SerializeField] private Button joinButton;
    [SerializeField] private string sessionName = "testroom";

    [Header("Fusion")]
    [SerializeField] private NetworkObject playerPrefab;

    private SpawnPoint[] spawnPoints;
    private NetworkRunner _runner;

    private bool _sceneReady;
    private bool _spawnedLocalPlayer;

    private void Awake()
    {
        base.Awake();

        if (joinButton != null)
        {
            joinButton.interactable = true;
            joinButton.onClick.RemoveListener(OnJoinClicked);
            joinButton.onClick.AddListener(OnJoinClicked);
        }
    }

    private void OnJoinClicked()
    {
        if (joinButton != null)
            joinButton.interactable = false;

        StartGame();
    }

    private async void StartGame()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("FusionBoot: playerPrefab not assigned.");
            if (joinButton != null) joinButton.interactable = true;
            return;
        }

        if (_runner != null && _runner.IsRunning)
            return;

        _sceneReady = false;
        _spawnedLocalPlayer = false;

        _runner = new GameObject("NetworkRunner").AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);

        var sceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        DontDestroyOnLoad(_runner.gameObject);

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName,
            Scene = SceneRef.FromIndex(nextSceneIndex),
            SceneManager = sceneManager
        });

        if (!result.Ok)
        {
            Debug.LogError($"FusionBoot: StartGame failed: {result.ShutdownReason}");
            if (joinButton != null) joinButton.interactable = true;
            return;
        }

        if (menuRigRoot != null)
            Destroy(menuRigRoot);
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        spawnPoints = FindObjectsOfType<SpawnPoint>(true);
        _sceneReady = spawnPoints != null && spawnPoints.Length > 0;

        if (!_sceneReady)
        {
            Debug.LogError("FusionBoot: No SpawnPoint found in loaded scene.");
            return;
        }

        TrySpawnLocalPlayer(runner);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // In Shared Mode, spawn only for YOURSELF.
        if (player == runner.LocalPlayer)
        {
            TrySpawnLocalPlayer(runner);
        }
    }

    private void TrySpawnLocalPlayer(NetworkRunner runner)
    {
        if (_spawnedLocalPlayer)
            return;

        if (!_sceneReady)
            return;

        if (spawnPoints == null || spawnPoints.Length == 0)
            return;

        int index = Mathf.Abs(runner.LocalPlayer.RawEncoded) % spawnPoints.Length;
        Transform spawn = spawnPoints[index].transform;

        var playerObj = runner.Spawn(
            playerPrefab,
            spawn.position,
            spawn.rotation
        );

        runner.SetPlayerObject(runner.LocalPlayer, playerObj);
        _spawnedLocalPlayer = true;

        Debug.Log($"FusionBoot: Spawned local player at spawn index {index}");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        // Player object association lets us find/despawn the leaving player's avatar.
        if (runner.TryGetPlayerObject(player, out var obj))
        {
            if (obj != null && obj.HasStateAuthority)
            {
                runner.Despawn(obj);
            }
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"FusionBoot: Shutdown {shutdownReason}");
        if (joinButton != null)
            joinButton.interactable = true;

        _runner = null;
        _sceneReady = false;
        _spawnedLocalPlayer = false;
    }

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, System.Collections.Generic.List<SessionInfo> sessionList) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, System.Collections.Generic.Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}
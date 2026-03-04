using System;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using TMPro;
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

    private void Awake()
    {
        base.Awake();

        if (joinButton != null)
        {
            joinButton.interactable = true;
            joinButton.onClick.AddListener(OnJoinClicked);
        }
    }

    private void OnJoinClicked()
    {
        joinButton.interactable = false;
        StartGame();
    }

    private async void StartGame()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("FusionBoot: playerPrefab not assigned.");
            return;
        }

        if (_runner != null && _runner.IsRunning)
            return;

        _runner = new GameObject("NetworkRunner").AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);

        var sceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        DontDestroyOnLoad(_runner.gameObject);

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = sessionName,
            Scene = SceneRef.FromIndex(nextSceneIndex),
            SceneManager = sceneManager
        });

        if (!result.Ok)
        {
            Debug.LogError($"FusionBoot: StartGame failed: {result.ShutdownReason}");
            joinButton.interactable = true;
            return;
        }

        if (menuRigRoot != null)
            menuRigRoot.SetActive(false);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;

        int index = player.RawEncoded % spawnPoints.Length;

        Transform spawn = spawnPoints[index].transform;

        runner.Spawn(
            playerPrefab,
            spawn.position,
            spawn.rotation,
            player
        );
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer && runner.TryGetPlayerObject(player, out var obj))
            runner.Despawn(obj);
    }
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        spawnPoints = FindObjectsOfType<SpawnPoint>();
    }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"FusionBoot: Shutdown {shutdownReason}");
        if (joinButton != null)
            joinButton.interactable = true;
    }

    // Unused callbacks
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
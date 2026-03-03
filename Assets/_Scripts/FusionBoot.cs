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
    [SerializeField] private GameObject menuRigRoot;     // Boot XR rig root (disable after connect)
    [SerializeField] private Button joinButton;          // optional (can be null)
    [SerializeField] private string sessionName = "testroom";

    [Header("Fusion")]
    [SerializeField] private NetworkObject metaPlayerPrefab;
    [SerializeField] private NetworkObject xrealPlayerPrefab;

    private NetworkRunner _runner;
    private NetworkObject _selectedPlayerPrefab;

    private void Awake()
    {
        base.Awake(); // from your SingletonPersistent (keep if required)
        if (joinButton != null)
            joinButton.interactable = true;
    }

    // Hook this to the button in MetaBoot scene
    public void OnClick_Meta()
    {
        _selectedPlayerPrefab = metaPlayerPrefab;
        _ = StartGame();
    }

    // Hook this to the button in XrealBoot scene
    public void OnClick_Xreal()
    {
        _selectedPlayerPrefab = xrealPlayerPrefab;
        _ = StartGame();
    }

    private async Task StartGame()
    {
        if (_selectedPlayerPrefab == null)
        {
            Debug.LogError("FusionBoot: Selected player prefab is null. Assign prefabs in inspector.");
            return;
        }

        if (_runner != null && _runner.IsRunning)
        {
            Debug.Log("FusionBoot: Runner already running.");
            return;
        }

        if (joinButton != null)
            joinButton.interactable = false;

        // Create runner
        _runner = new GameObject("NetworkRunner").AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);

        // Scene manager required for Fusion scene loading
        var sceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        DontDestroyOnLoad(_runner.gameObject);

        // Load the next scene in build order (Boot -> Shared)
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
            if (joinButton != null)
                joinButton.interactable = true;
            return;
        }

        // Disable Boot XR rig so we don't have two cameras/rigs active
        if (menuRigRoot != null)
            menuRigRoot.SetActive(false);
    }

    // ---- Fusion Callbacks ----

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer)
            return;

        // Spawn the selected prefab for this server instance.
        // NOTE: This assumes the prefab exists in BOTH builds if Meta and Xreal clients connect together.
        runner.Spawn(_selectedPlayerPrefab, Vector3.zero, Quaternion.identity, player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer && runner.TryGetPlayerObject(player, out var obj))
            runner.Despawn(obj);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"FusionBoot: Shutdown: {shutdownReason}");
        if (joinButton != null)
            joinButton.interactable = true;
    }

    // Required but unused:
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, System.Collections.Generic.List<SessionInfo> sessionList) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, System.Collections.Generic.Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}
using System;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Voice.Unity;

public class FusionBoot : SingletonPersistent<FusionBoot>, INetworkRunnerCallbacks
{
    [Header("Boot")]
    [SerializeField] private GameObject menuRigRoot;
    [SerializeField] private Button joinButtonRemote;
    [SerializeField] private Button joinButtonColocation;
    [SerializeField] private string sessionName = "testroom";
    [SerializeField] private GameObject runnerPrefab;

    [Header("Voice Chat")]
    [SerializeField] private Toggle voiceChatToggle;

    [Header("Colocation Avatar Visuals")]
    [SerializeField] private Toggle colocationAvatarToggle;

    [Header("Fusion")]
    [SerializeField] private NetworkObject playerPrefab;

    private SpawnPoint[] spawnPoints;
    private NetworkRunner _runner;
    public NetworkRunner Runner => _runner;
    private bool _sceneReady;
    private bool _spawnedLocalPlayer;

    // TRUE = VC enabled
    private static bool VoiceChatEnabled = true;

    // TRUE = avatars visible in Colocation
    public static bool ColocationAvatarsVisible = false;

    private void Awake()
    {
        base.Awake();

        // ---------------- VOICE TOGGLE ----------------
        if (voiceChatToggle != null)
        {
            voiceChatToggle.isOn = VoiceChatEnabled;

            voiceChatToggle.onValueChanged.RemoveAllListeners();
            voiceChatToggle.onValueChanged.AddListener((value) =>
            {
                VoiceChatEnabled = value;
                ApplyVoiceSettings();
            });
        }

        // ---------------- COLOCATION AVATAR TOGGLE ----------------
        if (colocationAvatarToggle != null)
        {
            colocationAvatarToggle.isOn = ColocationAvatarsVisible;

            colocationAvatarToggle.onValueChanged.RemoveAllListeners();
            colocationAvatarToggle.onValueChanged.AddListener((value) =>
            {
                ColocationAvatarsVisible = value;
            });
        }

        // ---------------- BUTTONS ----------------
        if (joinButtonRemote != null)
        {
            joinButtonRemote.interactable = true;
            joinButtonRemote.onClick.RemoveListener(OnRemoteJoinClicked);
            joinButtonRemote.onClick.AddListener(OnRemoteJoinClicked);
        }

        if (joinButtonColocation != null)
        {
            joinButtonColocation.interactable = true;
            joinButtonColocation.onClick.RemoveListener(OnColocationJoinClicked);
            joinButtonColocation.onClick.AddListener(OnColocationJoinClicked);
        }
    }

    private void EnsureRunner()
    {
        if (_runner != null)
            return;

        var go = Instantiate(runnerPrefab);
        DontDestroyOnLoad(go);

        _runner = go.GetComponent<NetworkRunner>();

        if (_runner == null)
        {
            Debug.Log("RunnerPrefab missing NetworkRunner");
            return;
        }

        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);

        ApplyVoiceSettings();
    }

    private void ApplyVoiceSettings()
    {
        if (_runner == null)
            return;

        Recorder recorder = _runner.GetComponentInChildren<Recorder>(true);

        if (recorder == null)
        {
            Debug.Log("No Photon Voice Recorder found.");
            return;
        }

        recorder.TransmitEnabled = VoiceChatEnabled;
        recorder.RecordingEnabled = VoiceChatEnabled;

        Debug.Log($"Voice Chat Enabled: {VoiceChatEnabled}");
    }

    private void OnRemoteJoinClicked()
    {
        if (joinButtonRemote != null)
            joinButtonRemote.interactable = false;

        if (joinButtonColocation != null)
            joinButtonColocation.interactable = false;

        StartGame(1);
    }

    private void OnColocationJoinClicked()
    {
        if (joinButtonRemote != null)
            joinButtonRemote.interactable = false;

        if (joinButtonColocation != null)
            joinButtonColocation.interactable = false;
        Debug.Log("FusionBoot: Button clicked, Starting Colocation Game");
        StartGame(2);
    }

    private async void StartGame(int sceneIndex = -1)
    {
        if (playerPrefab == null)
        {
            Debug.Log("FusionBoot: playerPrefab not assigned.");

            if (joinButtonRemote != null)
                joinButtonRemote.interactable = true;

            if (joinButtonColocation != null)
                joinButtonColocation.interactable = true;

            return;
        }
        Debug.Log($"FusionBoot: Checkpoint 1 - Starting game with sceneIndex {sceneIndex}");
        if (_runner != null && _runner.IsRunning)
            return;

        _sceneReady = false;
        _spawnedLocalPlayer = false;
        Debug.Log("FusionBoot: Checkpoint 2 - Ensuring runner");
        EnsureRunner();
        Debug.Log("FusionBoot: Checkpoint 3 - Getting scene manager");
        var sceneManager = _runner.GetComponent<NetworkSceneManagerDefault>();
        Debug.Log($"FusionBoot: DNS test");
        try
        {
            var entry = await System.Net.Dns.GetHostEntryAsync("ns.photonengine.io");
            Debug.Log($"DNS OK: {entry.AddressList.Length} addresses, first = {entry.AddressList[0]}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DNS FAIL: {e}");
        }


        Debug.Log("FusionBoot: Checkpoint 4 - Starting game");
        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName,
            Scene = SceneRef.FromIndex(sceneIndex),
            SceneManager = sceneManager
        });
        Debug.Log($"FusionBoot: StartGame result: {result.Ok}, {result.ShutdownReason}");
        if (!result.Ok)
        {
            Debug.Log($"FusionBoot: StartGame failed: {result.ErrorMessage}");

            if (joinButtonRemote != null)
                joinButtonRemote.interactable = true;

            if (joinButtonColocation != null)
                joinButtonColocation.interactable = true;

            return;
        }
        Debug.Log("FusionBoot: Checkpoint 5 - StartGame succeeded");
        if (menuRigRoot != null)
            Destroy(menuRigRoot);
        Debug.Log("FusionBoot: Checkpoint 6 - Destroyed menuRigRoot");
        ApplyVoiceSettings();
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        spawnPoints = FindObjectsOfType<SpawnPoint>(true);
        _sceneReady = spawnPoints != null && spawnPoints.Length > 0;

        ApplyVoiceSettings();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player != runner.LocalPlayer)
            return;

        if (_spawnedLocalPlayer)
            return;

        if (!_sceneReady)
            return;

        TrySpawnLocalPlayer(runner);
    }

    private void TrySpawnLocalPlayer(NetworkRunner runner)
    {
        if (_spawnedLocalPlayer)
            return;

        if (!_sceneReady)
            return;

        Transform spawn = null;

#if META_BUILD
    GameObject metaSpawn = GameObject.Find("MetaSpawn");

    if (metaSpawn != null)
        spawn = metaSpawn.transform;

#elif XREAL_BUILD
    GameObject xrealSpawn = GameObject.Find("XrealSpawn");

    if (xrealSpawn != null)
        spawn = xrealSpawn.transform;

#else
        Debug.Log("No build symbol defined. Using first SpawnPoint.");

        if (spawnPoints != null && spawnPoints.Length > 0)
            spawn = spawnPoints[0].transform;
#endif

        if (spawn == null)
        {
            Debug.Log("Could not find spawn point for this platform.");
            return;
        }

        Debug.Log($"Spawning player at {spawn.name}");

        var playerObj = runner.Spawn(
            playerPrefab,
            spawn.position,
            spawn.rotation
        );

        runner.SetPlayerObject(runner.LocalPlayer, playerObj);

        _spawnedLocalPlayer = true;

        ApplyVoiceSettings();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
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

        if (joinButtonRemote != null)
            joinButtonRemote.interactable = true;

        if (joinButtonColocation != null)
            joinButtonColocation.interactable = true;

        _runner = null;
        _sceneReady = false;
        _spawnedLocalPlayer = false;
    }

    public async void ReturnToMainMenu()
    {
        if (_runner != null && _runner.IsRunning)
        {
            await _runner.Shutdown();
        }

        _runner = null;
        _sceneReady = false;
        _spawnedLocalPlayer = false;

        SceneManager.LoadScene(0);
    }

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress joinButtonRemoteAddress, NetConnectFailedReason reason) { }
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
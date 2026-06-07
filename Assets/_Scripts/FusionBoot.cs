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

    private bool _sceneReady;
    private bool _spawnedLocalPlayer;

    // TRUE = VC enabled
    private static bool VoiceChatEnabled = true;

    // TRUE = avatars visible in Colocation
    public static bool ColocationAvatarsVisible = false;

    private void Awake()
    {
        base.Awake();
        InitUI();
    }

    private void InitUI()
    {
        if (joinButtonRemote != null)
        {
            joinButtonRemote.onClick.RemoveAllListeners();
            joinButtonRemote.onClick.AddListener(OnRemoteJoinClicked);
            joinButtonRemote.interactable = true;
        }

        if (joinButtonColocation != null)
        {
            joinButtonColocation.onClick.RemoveAllListeners();
            joinButtonColocation.onClick.AddListener(OnColocationJoinClicked);
            joinButtonColocation.interactable = true;
        }

        if (voiceChatToggle != null)
        {
            voiceChatToggle.onValueChanged.RemoveAllListeners();
            voiceChatToggle.onValueChanged.AddListener(v =>
            {
                VoiceChatEnabled = v;
                ApplyVoiceSettings();
            });

            voiceChatToggle.isOn = VoiceChatEnabled;
        }

        if (colocationAvatarToggle != null)
        {
            colocationAvatarToggle.onValueChanged.RemoveAllListeners();
            colocationAvatarToggle.onValueChanged.AddListener(v =>
            {
                ColocationAvatarsVisible = v;
            });

            colocationAvatarToggle.isOn = ColocationAvatarsVisible;
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
            Debug.LogError("RunnerPrefab missing NetworkRunner");
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
            Debug.LogWarning("No Photon Voice Recorder found.");
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

        StartGame(2);
    }

    private async void StartGame(int sceneIndex = -1)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("FusionBoot: playerPrefab not assigned.");

            if (joinButtonRemote != null)
                joinButtonRemote.interactable = true;

            if (joinButtonColocation != null)
                joinButtonColocation.interactable = true;

            return;
        }

        if (_runner != null && _runner.IsRunning)
            return;

        _sceneReady = false;
        _spawnedLocalPlayer = false;

        EnsureRunner();

        var sceneManager = _runner.GetComponent<NetworkSceneManagerDefault>();

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName,
            Scene = SceneRef.FromIndex(sceneIndex),
            SceneManager = sceneManager
        });

        if (!result.Ok)
        {
            Debug.LogError($"FusionBoot: StartGame failed: {result.ShutdownReason}");

            if (joinButtonRemote != null)
                joinButtonRemote.interactable = true;

            if (joinButtonColocation != null)
                joinButtonColocation.interactable = true;

            return;
        }

        if (menuRigRoot != null)
            Destroy(menuRigRoot);

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
        Debug.LogWarning("No build symbol defined. Using first SpawnPoint.");

        if (spawnPoints != null && spawnPoints.Length > 0)
            spawn = spawnPoints[0].transform;
#endif

        if (spawn == null)
        {
            Debug.LogError("Could not find spawn point for this platform.");
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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
            RebindSceneReferences();
    }
    void RebindSceneReferences()
    {
        menuRigRoot = GameObject.FindGameObjectWithTag("RIG");

        joinButtonRemote = GameObject.FindGameObjectWithTag("JBR")?.GetComponent<Button>();
        joinButtonColocation = GameObject.FindGameObjectWithTag("JBC")?.GetComponent<Button>();
        voiceChatToggle = GameObject.FindGameObjectWithTag("VCT")?.GetComponent<Toggle>();

        InitUI();
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
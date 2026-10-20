using System.Collections;
using UnityEngine;

/// <summary>
/// Gate that enables gameplay only after this client is calibrated AND every player in the
/// session has confirmed their calibration (tracked by ReadyManager).
/// </summary>
public class ColocationReadyGate : MonoBehaviour
{
    [SerializeField, Min(1)] private int minPlayers = 1;

    TableSpawner tableSpawner;

    private bool opened;
    private bool sessionStarted;     // floor calibration + table spawn only run once
    private bool floorCalibrated = false;
    private bool isCalibrated = false;

    public static ColocationReadyGate Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable() => ReadyManager.ResetBroadcast += OnResetBroadcast;
    private void OnDisable() => ReadyManager.ResetBroadcast -= OnResetBroadcast;

    private void Start()
    {
        tableSpawner = GetComponent<TableSpawner>();
        if (tableSpawner == null)
            Debug.LogError("TableSpawner component is missing.");
    }

    private void OnResetBroadcast()
    {
        // Gate can open again once everyone has recalibrated.
        opened = false;
        isCalibrated = false;
    }

    private void Update()
    {
        if (opened)
            return;

        SharedSpaceManager manager = SharedSpaceManager.Instance;
        if (manager == null || !manager.IsCalibrated)
            return;

        if (FusionBoot.Instance == null || FusionBoot.Instance.Runner == null)
            return;

        var runner = FusionBoot.Instance.Runner;

        if (ReadyManager.Instance == null || !ReadyManager.Instance.AllReady(minPlayers))
            return;

        opened = true;
        isCalibrated = true;
        Debug.Log("Co-location ready gate opened.");

        if (!runner.IsSharedModeMasterClient || sessionStarted)
            return;

        Debug.Log("This client is the shared mode master client. Proceeding...");
        sessionStarted = true;
        BeginConfig();
    }

    public void SetFloorCalibrated(bool calibrated)
    {
        floorCalibrated = calibrated;
    }

    private void BeginConfig()
    {
        if (!floorCalibrated)
        {
            Debug.Log("Floor not calibrated. Showing floor calibration UI.");
            FloorCalibration.Instance.ShowFloor();
            StartCoroutine(WaitForCalibration());
        }
        else
        {
            Debug.Log("Floor already calibrated. Spawning shared table.");
            if (tableSpawner != null)
                tableSpawner.SpawnSharedTable();
        }
    }

    IEnumerator WaitForCalibration()
    {
        Debug.Log("Waiting for floor calibration...");
        yield return new WaitUntil(() => floorCalibrated);
        Debug.Log("Calibration complete! Advancing game state...");

        if (isCalibrated && floorCalibrated && tableSpawner != null)
        {
            Debug.Log("Both floor and device calibration complete. Spawning shared table.");
            tableSpawner.SpawnSharedTable();
        }
    }
}
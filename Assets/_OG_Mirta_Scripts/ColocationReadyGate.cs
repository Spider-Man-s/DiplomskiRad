using UnityEngine;

/// <summary>
/// Optional gate that enables gameplay objects only after this client is calibrated AND Fusion
/// reports that both Meta and XREAL have confirmed calibration.
/// </summary>
public class ColocationReadyGate : MonoBehaviour
{
    [SerializeField] private ColocationSessionState sessionState;
    [SerializeField] private GameObject[] enableWhenReady;
    TableSpawner tableSpawner;
    FloorCalibration floorCalibration;
    private bool opened;
    private bool floorCalibrated = false;
    private bool isCalibrated = false;

    private void Start()
    {
        SetTargets(false);
        tableSpawner = GetComponent<TableSpawner>();
        floorCalibration = GetComponent<FloorCalibration>();
        if (tableSpawner == null)
            Debug.LogError("TableSpawner component is missing.");
        if (floorCalibration == null)
            Debug.LogError("FloorCalibration component is missing.");
    }

    private void Update()
    {
        if (opened)
            return;

        SharedSpaceManager manager = SharedSpaceManager.Instance;
        if (manager == null || !manager.IsCalibrated)
            return;

        if (sessionState != null && !sessionState.BothReady)
            return;

        opened = true;
        isCalibrated = true;
        SetTargets(true);
        Debug.Log("Co-location ready gate opened.");
        BeginConfig();

    }

    private void SetTargets(bool active)
    {
        if (enableWhenReady == null)
            return;

        foreach (GameObject target in enableWhenReady)
        {
            if (target != null)
                target.SetActive(active);
        }
    }

    private void BeginConfig()
    {

        if (!floorCalibrated)
        {
            floorCalibration.ShowFloor();
            //wait for user to confirm height and set floorCalibrated to true
            floorCalibrated = true;
        }

        if (isCalibrated && floorCalibrated)
        {
#if META_BUILD
        if (tableSpawner != null)
            tableSpawner.SpawnSharedTable();
#endif
        }

    }

}

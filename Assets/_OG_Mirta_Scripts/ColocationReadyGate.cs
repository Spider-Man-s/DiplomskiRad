using UnityEngine;
using System.Collections;
/// <summary>
/// Optional gate that enables gameplay objects only after this client is calibrated AND Fusion
/// reports that both Meta and XREAL have confirmed calibration.
/// </summary>
public class ColocationReadyGate : MonoBehaviour
{
    [SerializeField] private ColocationSessionState sessionState;
    [SerializeField] private GameObject[] enableWhenReady;

    TableSpawner tableSpawner;

    private bool opened;
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
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        SetTargets(false);
        tableSpawner = GetComponent<TableSpawner>();
        if (tableSpawner == null)
            Debug.LogError("TableSpawner component is missing.");

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

#if META_BUILD
        BeginConfig();
#endif
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


    public void SetFloorCalibrated(bool calibrated)
    {
        floorCalibrated = calibrated;
    }








    //////////////////////////////////











    private void BeginConfig()
    {

        if (!floorCalibrated)
        {
            FloorCalibration.Instance.ShowFloor();
            StartCoroutine(WaitForCalibration());
        }

        if (isCalibrated && floorCalibrated)
        {
            if (tableSpawner != null)
                tableSpawner.SpawnSharedTable();

        }

    }

    IEnumerator WaitForCalibration()
    {
        Debug.Log("Waiting for floor calibration...");
        yield return new WaitUntil(() => floorCalibrated == true);
        Debug.Log("Calibration complete! Advancing game state...");
    }


}

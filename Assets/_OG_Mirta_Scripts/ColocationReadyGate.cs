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
    private bool opened;

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
        SetTargets(true);
        Debug.Log("Co-location ready gate opened.");

#if META_BUILD
        if (tableSpawner!=null)
           tableSpawner.SpawnSharedTable();
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

}

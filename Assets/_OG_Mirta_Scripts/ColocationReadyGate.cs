using UnityEngine;

/// <summary>
/// Optional gate that enables gameplay objects only after this client is calibrated AND Fusion
/// reports that both Meta and XREAL have confirmed calibration.
/// </summary>
public class ColocationReadyGate : MonoBehaviour
{
    [SerializeField] private ColocationSessionState sessionState;
    [SerializeField] private GameObject[] enableWhenReady;

    private bool opened;

    private void Start()
    {
        SetTargets(false);
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

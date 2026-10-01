using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
public class FloorCalibration : NetworkBehaviour
{
    [SerializeField] private NetworkObject floorPlanePrefab;
    [SerializeField] private NetworkObject UIHeightSliderPrefab;

    [SerializeField] private NetworkObject UIXZPrefab;

    public float BoxHeight = 0f;
    [SerializeField, Min(0.0001f)] private float positionStepMeters = 0.005f;
    public float PositionStepMeters => positionStepMeters;
    private NetworkObject floorPlane;
    private NetworkObject UIHeightSlider;
    private NetworkObject UIXZ;
    private bool floorVisible = false;
    // private float startHeight = 0f;




    public static FloorCalibration Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowFloor()
    {
        var spawner = SharedSpaceSpawner.Instance;
        if (spawner == null)
        {
            Debug.Log("FloorCalibration: SharedSpaceSpawner not attached to the network yet.");
            return;
        }
        Debug.Log("Spawning floor plane and UI elements.");

        floorPlane = spawner.SpawnAtSharedPose(floorPlanePrefab, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f));
        // startHeight = floorPlane.transform.position.y;
        floorVisible = true;
        UIHeightSlider = spawner.SpawnAtSharedPose(UIHeightSliderPrefab, new Vector3(0f, 0.2f, 0f), new Vector3(0f, 0f, 0f));
        MoveFloor(0f);

    }

    public void MoveFloor(float newHeight)
    {
        if (floorPlane == null)
        {
            Debug.LogError("Floor plane is not spawned.");
            return;
        }
        float finalHeight = -BoxHeight / 2f - newHeight;
        Debug.Log("Moving floor plane to height: " + finalHeight);
        floorPlane.GetComponent<SharedSpaceNetworkTransform>().RequestSharedPose(new Pose(new Vector3(0, finalHeight, 0), new Quaternion(0, 0, 0, 1)));
    }

    public void ConfirmHeight()
    {
        FusionBoot.Instance.Runner.Despawn(UIHeightSlider);
        UIXZ = SharedSpaceSpawner.Instance.SpawnAtSharedPose(UIXZPrefab, new Vector3(0f, 0.1f, 0f), new Vector3(0f, 0f, 0f));
    }

    public void ConfirmXZ()
    {
        FusionBoot.Instance.Runner.Despawn(UIXZ);
        floorVisible = false;
        floorPlane?.GetComponent<SharedSpaceNetworkTransform>()?.RPC_SetVisible(false);

        ColocationReadyGate.Instance.SetFloorCalibrated(true);
    }




    public void PositionXPlus() => NudgePosition(Vector3.right, positionStepMeters);
    public void PositionXMinus() => NudgePosition(Vector3.right, -positionStepMeters);
    public void PositionZPlus() => NudgePosition(Vector3.forward, positionStepMeters);
    public void PositionZMinus() => NudgePosition(Vector3.forward, -positionStepMeters);

    private void NudgePosition(Vector3 localAxis, float amount)
    {
        if (floorPlane == null) return;

        SharedSpaceManager manager = SharedSpaceManager.Instance;
        if (manager == null || !manager.IsCalibrated)
        {
            Debug.LogWarning("SharedSpaceManager not calibrated.");
            return;
        }

        Transform root = floorPlane.transform;
        Vector3 worldDirection = root.TransformDirection(localAxis).normalized;
        Vector3 newWorldPosition = root.position + worldDirection * amount;

        var sharedTransform = floorPlane.GetComponent<SharedSpaceNetworkTransform>();
        if (sharedTransform == null)
        {
            Debug.LogWarning("floorPlane has no SharedSpaceNetworkTransform.");
            return;
        }

        Pose newWorldPose = new Pose(newWorldPosition, root.rotation);
        Pose newSharedPose = manager.WorldToShared(newWorldPose);

        sharedTransform.RequestSharedPose(newSharedPose);
    }


}

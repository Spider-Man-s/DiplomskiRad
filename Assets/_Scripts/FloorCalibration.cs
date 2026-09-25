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
    private SharedSpaceSpawner spawner;
    private bool floorVisible = false;
    private float startHeight = 0f;




    public static FloorCalibration Instance { get; private set; }
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
        spawner = GetComponent<SharedSpaceSpawner>();
        if (spawner == null)
            Debug.LogError("SharedSpaceSpawner component is missing.");

    }
    public void ShowFloor()
    {
        floorPlane = spawner.SpawnAtSharedPose(floorPlanePrefab, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f));
        startHeight = floorPlane.transform.position.y;
        floorVisible = true;
        UIHeightSlider = spawner.SpawnAtSharedPose(UIHeightSliderPrefab, new Vector3(0f, 0.5f, 0f), new Vector3(0f, 0f, 0f));

    }

    public void MoveFloor(float newHeight)
    {
        if (floorPlane == null)
        {
            Debug.LogError("Floor plane is not spawned.");
            return;
        }
        float finalHeight = startHeight - BoxHeight / 2f - newHeight;

        floorPlane.transform.position = new Vector3(floorPlane.transform.position.x, finalHeight, floorPlane.transform.position.z);
    }

    public void ConfirmHeight()
    {
        Runner.Despawn(UIHeightSlider);
        UIXZ = spawner.SpawnAtSharedPose(UIXZPrefab, new Vector3(0f, 0.1f, 0f), new Vector3(0f, 0f, 0f));
    }

    public void ConfirmXZ()
    {
        Runner.Despawn(UIXZ);
        floorVisible = false;
        MeshRenderer meshRenderer = floorPlane.GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;

        ColocationReadyGate.Instance.SetFloorCalibrated(true);
    }




    public void PositionXPlus() => NudgePosition(Vector3.right, positionStepMeters);
    public void PositionXMinus() => NudgePosition(Vector3.right, -positionStepMeters);
    public void PositionZPlus() => NudgePosition(Vector3.forward, positionStepMeters);
    public void PositionZMinus() => NudgePosition(Vector3.forward, -positionStepMeters);

    private void NudgePosition(Vector3 localAxis, float amount)
    {
        Transform root = floorPlane.transform;
        Vector3 worldDirection = root.TransformDirection(localAxis).normalized;
        root.position += worldDirection * amount;
    }


}

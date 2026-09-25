using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
public class FloorCalibration : NetworkBehaviour
{
    [SerializeField] private NetworkObject floorPlanePrefab;
    [SerializeField] private NetworkObject UIHeightSliderPrefab;

    public float BoxHeight = 0f;
    private NetworkObject floorPlane;
    private NetworkObject UIHeightSlider;

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
        floorVisible = false;
        MeshRenderer meshRenderer = floorPlane.GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;
        Runner.Despawn(UIHeightSlider);
        ColocationReadyGate.Instance.SetFloorCalibrated(true);
        //dovuci slidere ili gumbe za xy po podu

    }


}

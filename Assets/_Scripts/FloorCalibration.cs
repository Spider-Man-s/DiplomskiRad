using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
public class FloorCalibration : NetworkBehaviour
{
    [SerializeField] private NetworkObject floorPlanePrefab;
    public Slider mySlider;
    public GameObject uiPanel;
    public float BoxHeight = 0f;
    private SharedSpaceSpawner spawner;
    public TMP_Text heightText;
    private float sliderValue;


    private void Start()
    {
        spawner = GetComponent<SharedSpaceSpawner>();
        if (spawner == null)
            Debug.LogError("SharedSpaceSpawner component is missing.");

        mySlider.onValueChanged.AddListener(OnSliderChanged);

    }
    void OnSliderChanged(float newValue)
    {
        heightText.text = "Set Table Height: " + newValue.ToString("F2") + " cm";
        sliderValue = newValue;
    }
    public void ConfirmHeight()
    {

        BoxHeight *= 0.5f;
        float totalHeight = BoxHeight + sliderValue;
        spawner.SpawnAtSharedPose(floorPlanePrefab, new Vector3(0f, -totalHeight, 0f), new Vector3(0f, 0f, 0f));
        uiPanel.SetActive(false);


    }


}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
public class FloorCalibration : NetworkBehaviour
{
    [SerializeField] private NetworkObject floorPlanePrefab;
    public Slider mySlider;
    public GameObject UIHeightSlider;

    public NetworkObject floorPlane;
    private SharedSpaceSpawner spawner;
    public TMP_Text heightText;
    private float sliderValue;
    private bool floorVisible = false;
    public float BoxHeight = 0f;
    private float totalHeight = 0f;

    private void Start()
    {
        spawner = GetComponent<SharedSpaceSpawner>();
        if (spawner == null)
            Debug.LogError("SharedSpaceSpawner component is missing.");



    }
    void OnHeightSliderChanged(float newValue)
    {
        heightText.text = "Set Table Height: " + newValue.ToString("F2") + " cm";
        sliderValue = newValue / 10f;
        if (floorVisible) //uzmi pocetnu visinu i onda racunaj, ovako ce biti preveliki step
        {
            totalHeight = BoxHeight / 2f + sliderValue;
            Vector3 pos = floorPlane.transform.position;
            pos.y = -totalHeight;
            floorPlane.transform.position = pos;
        }
    }

    public void ShowFloor()
    {
        floorPlane = spawner.SpawnAtSharedPose(floorPlanePrefab, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f));
        floorVisible = true;
        UIHeightSlider.SetActive(true); //spawn? nez kaj s lokalnim
        mySlider.onValueChanged.AddListener(OnHeightSliderChanged);
    }
    public void ConfirmHeight()
    {
        floorVisible = false;
        //ugasi floor renderer
        UIHeightSlider.SetActive(false);
        //dovuci slidere ili gumbe za xy po podu

    }


}

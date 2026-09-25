using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
public class FloorHeightUISetup : MonoBehaviour
{
    public Slider mySlider;
    public Button confirmButton;
    public TMP_Text heightText;

    private float sliderValue;

    private void Start()
    {
        confirmButton.onClick.AddListener(FloorCalibration.Instance.ConfirmHeight);
        mySlider.onValueChanged.AddListener(OnHeightSliderChanged);
    }

    void OnHeightSliderChanged(float newValue)
    {
        heightText.text = "Set Table Height: " + newValue.ToString("F2") + " cm";
        sliderValue = newValue / 100f;
        FloorCalibration.Instance.MoveFloor(sliderValue);
    }


}

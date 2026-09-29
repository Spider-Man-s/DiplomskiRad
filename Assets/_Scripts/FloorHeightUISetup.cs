using Fusion;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FloorHeightUISetup : NetworkBehaviour
{
    public Slider mySlider;
    public Button confirmButton;
    public TMP_Text heightText;

    [Networked, OnChangedRender(nameof(OnNetworkedValueChanged))]
    public float SliderValue { get; set; }

    private void Awake()
    {
        mySlider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void Start()
    {
        confirmButton.onClick.AddListener(FloorCalibration.Instance.ConfirmHeight);
    }

    public override void Spawned()
    {
        mySlider.SetValueWithoutNotify(SliderValue);
        UpdateText(SliderValue);
    }

    private void OnSliderChanged(float value)
    {
        if (!Object.HasStateAuthority)
        {
            Object.RequestStateAuthority();
        }

        SliderValue = value;
        UpdateText(value);
        FloorCalibration.Instance.MoveFloor(value / 100f);
    }

    private void OnNetworkedValueChanged()
    {
        if (Object.HasStateAuthority) return;

        mySlider.SetValueWithoutNotify(SliderValue);
        UpdateText(SliderValue);
    }

    private void UpdateText(float value)
    {
        heightText.text = "Set Table Height: " + value.ToString("F2") + " cm";
    }
}
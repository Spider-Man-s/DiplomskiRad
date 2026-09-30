using Fusion;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FloorHeightUISetup : NetworkBehaviour
{
    public Slider mySlider;
    public Button confirmButton;
    public TMP_Text heightText;

    [Networked, OnChangedRender(nameof(SnapSlider))]
    private float NetworkedSliderValue { get; set; }

    private void Awake()
    {
        mySlider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void Start()
    {
        confirmButton.onClick.AddListener(() => RPC_ConfirmHeight());
    }

    private void OnSliderChanged(float value)
    {
        UpdateText(value);
        RPC_SetFloorHeight(value);
    }

    private void UpdateText(float value)
    {
        heightText.text = "Set Table Height: " + value.ToString("F2") + " cm";
    }
    private void SnapSlider()
    {
        mySlider.SetValueWithoutNotify(NetworkedSliderValue);
        UpdateText(NetworkedSliderValue);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SetFloorHeight(float rawSliderValue)
    {
        NetworkedSliderValue = rawSliderValue;
        FloorCalibration.Instance.MoveFloor(rawSliderValue / 100f);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_ConfirmHeight()
    {
        FloorCalibration.Instance.ConfirmHeight();

    }

}
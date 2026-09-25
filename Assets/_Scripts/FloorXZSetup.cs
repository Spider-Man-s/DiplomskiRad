using UnityEngine;
using UnityEngine.UI;
public class FloorXZSetup : MonoBehaviour
{
    public Button confirmButton;
    public Button xPlusButton;
    public Button xMinusButton;
    public Button zPlusButton;
    public Button zMinusButton;

    private void Start()
    {
        confirmButton.onClick.AddListener(FloorCalibration.Instance.ConfirmXZ);
        xPlusButton.onClick.AddListener(FloorCalibration.Instance.PositionXPlus);
        xMinusButton.onClick.AddListener(FloorCalibration.Instance.PositionXMinus);
        zPlusButton.onClick.AddListener(FloorCalibration.Instance.PositionZPlus);
        zMinusButton.onClick.AddListener(FloorCalibration.Instance.PositionZMinus);
    }
}

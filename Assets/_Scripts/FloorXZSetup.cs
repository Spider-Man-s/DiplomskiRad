using UnityEngine;
using UnityEngine.UI;
using Fusion;
public class FloorXZSetup : NetworkBehaviour
{
    public Button confirmButton;
    public Button xPlusButton;
    public Button xMinusButton;
    public Button zPlusButton;
    public Button zMinusButton;

    private void Start()
    {
        confirmButton.onClick.AddListener(() => RPC_ConfirmXZ());
        xPlusButton.onClick.AddListener(() => RPC_PositionXPlus());
        xMinusButton.onClick.AddListener(() => RPC_PositionXMinus());
        zPlusButton.onClick.AddListener(() => RPC_PositionZPlus());
        zMinusButton.onClick.AddListener(() => RPC_PositionZMinus());
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PositionXPlus()
    {
        FloorCalibration.Instance.PositionXPlus();
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PositionXMinus()
    {
        FloorCalibration.Instance.PositionXMinus();
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PositionZPlus()
    {
        FloorCalibration.Instance.PositionZPlus();
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PositionZMinus()
    {
        FloorCalibration.Instance.PositionZMinus();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ConfirmXZ()
    {
        FloorCalibration.Instance.ConfirmXZ();
    }
}

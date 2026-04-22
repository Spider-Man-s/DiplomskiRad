using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Meta.XR.MultiplayerBlocks.Shared;

public class TransferOwnershipOnXRISelect : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _xriGrab;
    private ITransferOwnership _transferOwnership;

    private void Awake()
    {
        _xriGrab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (_xriGrab == null)
        {
            Debug.LogError("XRGrabInteractable missing.");
            enabled = false;
            return;
        }

        _transferOwnership = GetComponent<ITransferOwnership>();

        if (_transferOwnership == null)
        {
            Debug.LogError("ITransferOwnership component missing.");
            enabled = false;
            return;
        }

        _xriGrab.selectEntered.AddListener(OnGrab);
    }

    private void OnDestroy()
    {
        if (_xriGrab != null)
        {
            _xriGrab.selectEntered.RemoveListener(OnGrab);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        // Only request if we don't already own it
        if (!_transferOwnership.HasOwnership())
        {
            _transferOwnership.TransferOwnershipToLocalPlayer();
        }
    }
}
using Fusion;
using UnityEngine;

using Oculus.Interaction; // Meta SDK

public class GrabNetworkable : NetworkBehaviour
{
    [Header("XR Components")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable xriGrab;
    [SerializeField] private Grabbable metaGrab; // Meta

    private bool isGrabbed = false;
    private bool wasGrabbed = false;

    private bool hadAuthorityLastTick = false;

    private void Awake()
    {
        // Auto-detect components
        if (xriGrab == null)
            xriGrab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (metaGrab == null)
            metaGrab = GetComponent<Grabbable>();
        //  metaGrab.InjectOptionalVelocityCalculator(null);
    }

    // =========================
    // GRAB DETECTION (META + XRI)
    // =========================

    private void Update()
    {
        bool xri = xriGrab != null && xriGrab.isSelected;
        bool meta = metaGrab != null && metaGrab.SelectingPointsCount > 0;

        isGrabbed = xri || meta;

        // Grab START
        if (isGrabbed && !wasGrabbed)
        {
            TryBeginGrab();
        }
        // Grab END
        else if (!isGrabbed && wasGrabbed)
        {
            EndGrab();
        }

        wasGrabbed = isGrabbed;
    }

    // =========================
    // NETWORK CONTROL
    // =========================

    private void TryBeginGrab()
    {
        if (Runner == null || !Runner.IsRunning)
            return;

        if (!Object.HasStateAuthority)
        {
            Object.RequestStateAuthority(); // 🔥 race resolved by Fusion
        }
        else
        {
            EnableControl();
        }
    }

    private void EndGrab()
    {
        if (!Object.HasStateAuthority)
            return;

        DisableControl();
        Object.ReleaseStateAuthority();
    }

    // =========================
    // AUTHORITY TRACKING
    // =========================

    public override void FixedUpdateNetwork()
    {
        bool hasAuthority = Object.HasStateAuthority;

        if (hasAuthority != hadAuthorityLastTick)
        {
            if (hasAuthority && isGrabbed)
            {
                EnableControl();
            }
            else
            {
                DisableControl();
            }

            hadAuthorityLastTick = hasAuthority;
        }
    }

    // =========================
    // CONTROL GATING
    // =========================

    private void EnableControl()
    {
        // Allow XR systems to move object
        SetGrabEnabled(true);
    }

    private void DisableControl()
    {
        // Prevent movement if not owner
        SetGrabEnabled(false);
    }

    private void SetGrabEnabled(bool enabled)
    {
        if (xriGrab != null)
            xriGrab.enabled = enabled;

        if (metaGrab != null)
            metaGrab.enabled = enabled;
    }
}
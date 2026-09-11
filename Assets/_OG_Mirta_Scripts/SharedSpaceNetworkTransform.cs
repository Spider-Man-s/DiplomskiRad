using Fusion;
using UnityEngine;

/// <summary>
/// Canonical Fusion 2 transform replication for co-located content.
///
/// IMPORTANT:
/// - The networked position/rotation are ALWAYS Shared Space values.
/// - Do not put a normal Fusion NetworkTransform on the same object if it also writes the transform.
/// - The local Unity transform is converted to/from Shared Space through SharedSpaceManager.
///
/// For grabbed objects, call BeginLocalDrive() on grab start and EndLocalDrive() on grab end.
/// While locally driven, this component submits the current local-world pose in Shared Space.
/// If the local peer does not have State Authority, pose updates are forwarded to State Authority by RPC.
/// </summary>
[RequireComponent(typeof(NetworkObject))]
public class SharedSpaceNetworkTransform : NetworkBehaviour
{
    [Header("Behavior")]
    [Tooltip("If true, State Authority continuously treats its Unity transform as the source even when not explicitly grabbed. Enable for authoritative physics/movers. Leave false for static/resting objects whose network state should simply be rendered.")]
    [SerializeField] private bool stateAuthorityContinuouslyDrives;

    [Networked] private Vector3 SharedPosition { get; set; }
    [Networked] private Quaternion SharedRotation { get; set; }
    [Networked] private NetworkBool PoseInitialized { get; set; }

    private bool localDrive;

    public Pose NetworkSharedPose => new Pose(SharedPosition, SharedRotation);
    public bool IsLocallyDriven => localDrive;

    public override void Spawned()
    {
        SharedSpaceManager manager = SharedSpaceManager.Instance;

        // If this peer spawned the object after local calibration and no explicit shared pose has
        // yet been initialized, capture its current world pose as the initial shared pose.
        if (Object.HasStateAuthority && !PoseInitialized && manager != null && manager.IsCalibrated)
        {
            Pose shared = manager.WorldToShared(new Pose(transform.position, transform.rotation));
            SetNetworkPoseAsAuthority(shared);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!localDrive && !(Object.HasStateAuthority && stateAuthorityContinuouslyDrives))
            return;

        SharedSpaceManager manager = SharedSpaceManager.Instance;
        if (manager == null || !manager.IsCalibrated)
            return;

        Pose sharedPose = manager.WorldToShared(new Pose(transform.position, transform.rotation));

        if (Object.HasStateAuthority)
            SetNetworkPoseAsAuthority(sharedPose);
        else
            RPC_SubmitSharedPose(sharedPose.position, sharedPose.rotation);
    }

    public override void Render()
    {
        // A local interaction system is currently driving the visible transform, so don't fight it.
        if (localDrive)
            return;

        SharedSpaceManager manager = SharedSpaceManager.Instance;
        if (manager == null || !manager.IsCalibrated || !PoseInitialized)
            return;

        Pose worldPose = manager.SharedToWorld(NetworkSharedPose);
        transform.SetPositionAndRotation(worldPose.position, worldPose.rotation);
    }

    /// <summary>Wire this to your grab/select-start event.</summary>
    public void BeginLocalDrive()
    {
        localDrive = true;
    }

    /// <summary>Wire this to your grab/select-end event.</summary>
    public void EndLocalDrive()
    {
        SubmitCurrentWorldPoseNow();
        localDrive = false;
    }

    /// <summary>
    /// Useful for non-grab interactions that directly move the transform and want to submit once.
    /// </summary>
    public void SubmitCurrentWorldPoseNow()
    {
        SharedSpaceManager manager = SharedSpaceManager.Instance;
        if (manager == null || !manager.IsCalibrated || Object == null)
            return;

        Pose sharedPose = manager.WorldToShared(new Pose(transform.position, transform.rotation));

        if (Object.HasStateAuthority)
            SetNetworkPoseAsAuthority(sharedPose);
        else
            RPC_SubmitSharedPose(sharedPose.position, sharedPose.rotation);
    }

    /// <summary>
    /// Initialize/snap this NetworkObject using an already-shared pose. Must be called by State Authority.
    /// </summary>
    public void SetSharedPoseAsAuthority(Pose sharedPose)
    {
        if (Object == null || !Object.HasStateAuthority)
        {
            Debug.LogWarning("SetSharedPoseAsAuthority called without State Authority.");
            return;
        }

        SetNetworkPoseAsAuthority(sharedPose);

        SharedSpaceManager manager = SharedSpaceManager.Instance;
        if (manager != null && manager.IsCalibrated)
        {
            Pose worldPose = manager.SharedToWorld(sharedPose);
            transform.SetPositionAndRotation(worldPose.position, worldPose.rotation);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SubmitSharedPose(Vector3 sharedPosition, Quaternion sharedRotation)
    {
        if (!Object.HasStateAuthority)
            return;

        SetNetworkPoseAsAuthority(new Pose(sharedPosition, sharedRotation));
    }

    private void SetNetworkPoseAsAuthority(Pose sharedPose)
    {
        SharedPosition = sharedPose.position;
        SharedRotation = sharedPose.rotation;
        PoseInitialized = true;
    }
}

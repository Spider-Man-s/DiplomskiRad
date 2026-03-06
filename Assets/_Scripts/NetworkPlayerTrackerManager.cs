using Fusion;
using UnityEngine;

public class NetworkPlayerTrackerManager : NetworkBehaviour
{
    [Header("Network Visual Targets")]
    [SerializeField] private Transform netHead;
    [SerializeField] private Transform netLeftHand;
    [SerializeField] private Transform netRightHand;

    [Header("Rigs")]
    [SerializeField] private GameObject metaRig;
    [SerializeField] private GameObject xrealRig;

    [Header("Meta Tracked Transforms")]
    [SerializeField] private Transform metaHead;
    [SerializeField] private Transform metaLeftHand;
    [SerializeField] private Transform metaRightHand;

    [Header("Xreal Tracked Transforms")]
    [SerializeField] private Transform xrealHead;
    [SerializeField] private Transform xrealLeftHand;
    [SerializeField] private Transform xrealRightHand;

    private Transform srcHead;
    private Transform srcLeft;
    private Transform srcRight;

    // Host/state-authority-owned replicated pose
    [Networked] private Vector3 HeadPos { get; set; }
    [Networked] private Quaternion HeadRot { get; set; }

    [Networked] private Vector3 LeftPos { get; set; }
    [Networked] private Quaternion LeftRot { get; set; }

    [Networked] private Vector3 RightPos { get; set; }
    [Networked] private Quaternion RightRot { get; set; }

    public override void Spawned()
    {
        if (!Object.HasInputAuthority)
        {
            metaRig.SetActive(false);
            xrealRig.SetActive(false);
            return;
        }

#if META_BUILD
        ActivateMeta();
#elif XREAL_BUILD
        ActivateXreal();
#else
        metaRig.SetActive(false);
        xrealRig.SetActive(false);
        Debug.LogError("No build symbol defined. Define META_BUILD or XREAL_BUILD.");
#endif
    }

    private void ActivateMeta()
    {
        metaRig.SetActive(true);
        xrealRig.SetActive(false);

        srcHead = metaHead;
        srcLeft = metaLeftHand;
        srcRight = metaRightHand;
    }

    private void ActivateXreal()
    {
        metaRig.SetActive(false);
        xrealRig.SetActive(true);

        srcHead = xrealHead;
        srcLeft = xrealLeftHand;
        srcRight = xrealRightHand;
    }

    public override void FixedUpdateNetwork()
    {
        // Local owner samples XR tracking and sends to host
        if (Object.HasInputAuthority)
        {
            if (srcHead && srcLeft && srcRight)
            {
                RPC_SendPose(
                    srcHead.position, srcHead.rotation,
                    srcLeft.position, srcLeft.rotation,
                    srcRight.position, srcRight.rotation
                );
            }
        }

        // State authority can also set its own pose directly without RPC bounce
        if (Object.HasStateAuthority && Object.HasInputAuthority)
        {
            if (srcHead && srcLeft && srcRight)
            {
                HeadPos = srcHead.position;
                HeadRot = srcHead.rotation;

                LeftPos = srcLeft.position;
                LeftRot = srcLeft.rotation;

                RightPos = srcRight.position;
                RightRot = srcRight.rotation;
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, Channel = RpcChannel.Unreliable)]
    private void RPC_SendPose(
        Vector3 headPos, Quaternion headRot,
        Vector3 leftPos, Quaternion leftRot,
        Vector3 rightPos, Quaternion rightRot)
    {
        HeadPos = headPos;
        HeadRot = headRot;

        LeftPos = leftPos;
        LeftRot = leftRot;

        RightPos = rightPos;
        RightRot = rightRot;
    }

    public override void Render()
    {
        // Everyone renders the replicated pose from the host-owned networked state
        if (netHead)
        {
            netHead.position = HeadPos;
            netHead.rotation = HeadRot;
        }

        if (netLeftHand)
        {
            netLeftHand.position = LeftPos;
            netLeftHand.rotation = LeftRot;
        }

        if (netRightHand)
        {
            netRightHand.position = RightPos;
            netRightHand.rotation = RightRot;
        }
    }
}



/*

using UnityEngine;
using Fusion;

public class NetworkPlayerTrackerManager : NetworkBehaviour
{
    [Header("Network Targets")]
    [SerializeField] private Transform netHead;
    [SerializeField] private Transform netLeftHand;
    [SerializeField] private Transform netRightHand;

    [Header("Rigs")]
    [SerializeField] private GameObject metaRig;
    [SerializeField] private GameObject xrealRig;

    [Header("Meta Tracked Transforms")]
    [SerializeField] private Transform metaHead;
    [SerializeField] private Transform metaLeftHand;
    [SerializeField] private Transform metaRightHand;

    [Header("Xreal Tracked Transforms")]
    [SerializeField] private Transform xrealHead;
    [SerializeField] private Transform xrealLeftHand;
    [SerializeField] private Transform xrealRightHand;

    private Transform srcHead;
    private Transform srcLeft;
    private Transform srcRight;

    public override void Spawned()
    {

        if (!Object.HasInputAuthority)
        {
            metaRig.SetActive(false);
            xrealRig.SetActive(false);
            return;
        }

#if META_BUILD
        ActivateMeta();
#elif XREAL_BUILD
        ActivateXreal();
#else
        metaRig.SetActive(false);
        xrealRig.SetActive(false);
#endif
    }

    private void ActivateMeta()
    {
        metaRig.SetActive(true);
        xrealRig.SetActive(false);

        srcHead = metaHead;
        srcLeft = metaLeftHand;
        srcRight = metaRightHand;
    }

    private void ActivateXreal()
    {
        metaRig.SetActive(false);
        xrealRig.SetActive(true);

        srcHead = xrealHead;
        srcLeft = xrealLeftHand;
        srcRight = xrealRightHand;
    }

    private void LateUpdate()
    {
        if (!Object.HasInputAuthority)
            return;

        if (!srcHead || !srcLeft || !srcRight)
            return;

        netHead.position = srcHead.position;
        netHead.rotation = srcHead.rotation;

        netLeftHand.position = srcLeft.position;
        netLeftHand.rotation = srcLeft.rotation;

        netRightHand.position = srcRight.position;
        netRightHand.rotation = srcRight.rotation;
    }
}

*/
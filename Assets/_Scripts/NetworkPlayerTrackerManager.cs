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

    [Networked] private Vector3 HeadPos { get; set; }
    [Networked] private Quaternion HeadRot { get; set; }

    [Networked] private Vector3 LeftPos { get; set; }
    [Networked] private Quaternion LeftRot { get; set; }

    [Networked] private Vector3 RightPos { get; set; }
    [Networked] private Quaternion RightRot { get; set; }

    public override void Spawned()
    {
        if (!Object.HasStateAuthority)
        {
            metaRig.SetActive(false);
            xrealRig.SetActive(false);
            return;
        }

        netHead.GetComponent<MeshRenderer>().enabled = false;
        netLeftHand.GetComponent<MeshRenderer>().enabled = false;
        netRightHand.GetComponent<MeshRenderer>().enabled = false;

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
        if (!Object.HasStateAuthority)
            return;

        if (!srcHead || !srcLeft || !srcRight)
            return;

        HeadPos = srcHead.position;
        HeadRot = srcHead.rotation;

        LeftPos = srcLeft.position;
        LeftRot = srcLeft.rotation;

        RightPos = srcRight.position;
        RightRot = srcRight.rotation;
    }

    public override void Render()
    {
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
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
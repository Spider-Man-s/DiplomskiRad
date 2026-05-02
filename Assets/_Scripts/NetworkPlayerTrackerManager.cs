using Fusion;
using UnityEngine;

public class NetworkPlayerTrackerManager : NetworkBehaviour
{
    [Header("Network Visual Targets")]
    [SerializeField] private Transform netHead;

    [Header("Rigs")]
    [SerializeField] private GameObject metaRig;
    [SerializeField] private GameObject xrealRig;

    [Header("Tracked Head")]
    [SerializeField] private Transform metaHead;
    [SerializeField] private Transform xrealHead;

    private Transform srcHead;

    [Networked] private Vector3 HeadPos { get; set; }
    [Networked] private Quaternion HeadRot { get; set; }

    public override void Spawned()
    {
        if (!Object.HasStateAuthority)
        {
            metaRig.SetActive(false);
            xrealRig.SetActive(false);
            return;
        }

        netHead.GetComponentInChildren<MeshRenderer>().enabled = false;

#if META_BUILD
        ActivateMeta();
#elif XREAL_BUILD
        ActivateXreal();
#endif
    }

    private void ActivateMeta()
    {
        metaRig.SetActive(true);
        xrealRig.SetActive(false);
        srcHead = metaHead;
    }

    private void ActivateXreal()
    {
        metaRig.SetActive(false);
        xrealRig.SetActive(true);
        srcHead = xrealHead;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority || !srcHead)
            return;

        HeadPos = srcHead.position;
        HeadRot = srcHead.rotation;
    }

    public override void Render()
    {
        if (netHead)
        {
            netHead.position = HeadPos;
            netHead.rotation = HeadRot;
        }
    }
}
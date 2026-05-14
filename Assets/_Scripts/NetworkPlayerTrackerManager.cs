using Fusion;
using UnityEngine;

public class NetworkPlayerTrackerManager : NetworkBehaviour
{
    [Header("Network Visual Targets")]
    [SerializeField] private Transform netHead;
    [SerializeField] private Transform netRightHand;
    [SerializeField] private Transform netLeftHand;

    [Header("Rigs")]
    [SerializeField] private GameObject metaRig;
    [SerializeField] private GameObject xrealRig;

    [Header("Tracked Head")]
    [SerializeField] private Transform metaHead;
    [SerializeField] private Transform xrealHead;

    [Header("Tracked Hands (Meta)")]
    [SerializeField] private Transform metaRightHand;
    [SerializeField] private Transform metaLeftHand;

    [Header("Tracked Hands (XREAL)")]
    [SerializeField] private Transform xrealRightHand;
    [SerializeField] private Transform xrealLeftHand;

    private Transform srcHead;
    private Transform srcRightHand;
    private Transform srcLeftHand;

    [Networked] private Vector3 HeadPos { get; set; }
    [Networked] private Quaternion HeadRot { get; set; }

    [Networked] private Vector3 RightHandPos { get; set; }
    [Networked] private Quaternion RightHandRot { get; set; }
    [Networked] private NetworkBool RightHandTracked { get; set; }

    [Networked] private Vector3 LeftHandPos { get; set; }
    [Networked] private Quaternion LeftHandRot { get; set; }
    [Networked] private NetworkBool LeftHandTracked { get; set; }

    public override void Spawned()
    {
        if (!Object.HasStateAuthority)
        {
            metaRig.SetActive(false);
            xrealRig.SetActive(false);
            return;
        }

        // Hide local head mesh
        if (netHead)
        {
            var renderer = netHead.GetComponentInChildren<MeshRenderer>();
            if (renderer) renderer.enabled = false;
        }

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
        srcRightHand = metaRightHand;
        srcLeftHand = metaLeftHand;
    }

    private void ActivateXreal()
    {
        metaRig.SetActive(false);
        xrealRig.SetActive(true);

        srcHead = xrealHead;
        srcRightHand = xrealRightHand;
        srcLeftHand = xrealLeftHand;
    }

    private void OnEnable()
    {
        Application.onBeforeRender += ForceHandTransforms;
    }

    private void OnDisable()
    {
        Application.onBeforeRender -= ForceHandTransforms;
    }

    private void ForceHandTransforms()
    {
        if (!Object)
            return;

        // HEAD
        if (netHead)
        {
            netHead.position = HeadPos;
            netHead.rotation = HeadRot;
        }

        // RIGHT
        if (netRightHand)
        {
            netRightHand.gameObject.SetActive(RightHandTracked);

            if (RightHandTracked)
            {
                netRightHand.position = RightHandPos;
                netRightHand.rotation = RightHandRot;
            }
        }

        // LEFT
        if (netLeftHand)
        {
            netLeftHand.gameObject.SetActive(LeftHandTracked);

            if (LeftHandTracked)
            {
                netLeftHand.position = LeftHandPos;
                netLeftHand.rotation = LeftHandRot;
            }
        }
    }

    public override void Render()
    {
        float t = 20f * Time.deltaTime;

        // HEAD
        if (netHead)
        {
            netHead.position = Vector3.Lerp(netHead.position, HeadPos, t);
            netHead.rotation = Quaternion.Slerp(netHead.rotation, HeadRot, t);
        }

        // RIGHT HAND
        if (netRightHand)
        {
            netRightHand.gameObject.SetActive(RightHandTracked);

            if (RightHandTracked)
            {
                netRightHand.position = Vector3.Lerp(netRightHand.position, RightHandPos, t);
                netRightHand.rotation = Quaternion.Slerp(netRightHand.rotation, RightHandRot, t);
            }
        }

        // LEFT HAND
        if (netLeftHand)
        {
            netLeftHand.gameObject.SetActive(LeftHandTracked);

            if (LeftHandTracked)
            {
                netLeftHand.position = Vector3.Lerp(netLeftHand.position, LeftHandPos, t);
                netLeftHand.rotation = Quaternion.Slerp(netLeftHand.rotation, LeftHandRot, t);
            }
        }
    }
}
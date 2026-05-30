using Fusion;
using UnityEngine;

public class ColocationManager : NetworkBehaviour
{
    public static ColocationManager Instance { get; private set; }

    public Vector3 MetaPosition { get; private set; }
    public Quaternion MetaRotation { get; private set; }

    public Vector3 XrealPosition { get; private set; }
    public Quaternion XrealRotation { get; private set; }

    public float MetaProjectedZ { get; private set; }
    public float XrealProjectedZ { get; private set; }

    public bool MetaReady { get; private set; }
    public bool XrealReady { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Called continuously by QRPlacementTracker

    public void SetMetaLocalTransform(
     Vector3 position,
     Quaternion rotation,
     float projectedZ)
    {
        RPC_UpdateTransform(
            position,
            rotation,
            projectedZ,
            true
        );
    }
    public void SetXrealLocalTransform(
     Vector3 position,
     Quaternion rotation,
     float projectedZ)
    {
        RPC_UpdateTransform(
            position,
            rotation,
            projectedZ,
            false
        );
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_UpdateTransform(
     Vector3 position,
     Quaternion rotation,
     float projectedZ,
     bool isMeta)
    {
        if (isMeta)
        {
            MetaPosition = position;
            MetaRotation = rotation;
            MetaProjectedZ = projectedZ;
        }
        else
        {
            XrealPosition = position;
            XrealRotation = rotation;
            XrealProjectedZ = projectedZ;
        }
    }


    public void ConfirmMeta()
    {
        RPC_Confirm(true);
    }

    public void ConfirmXreal()
    {
        RPC_Confirm(false);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_Confirm(bool isMeta)
    {
        if (isMeta)
        {
            MetaReady = true;
            Debug.Log("META CONFIRMED");
        }
        else
        {
            XrealReady = true;
            Debug.Log("XREAL CONFIRMED");
        }

        Debug.Log(
            $"MetaReady={MetaReady} XrealReady={XrealReady}"
        );
    }
}
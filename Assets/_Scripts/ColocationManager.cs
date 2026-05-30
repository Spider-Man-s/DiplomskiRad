using Fusion;
using UnityEngine;

public class ColocationManager : NetworkBehaviour
{
    public static ColocationManager Instance { get; private set; }

    public Vector3 MetaPosition { get; private set; }
    public Quaternion MetaRotation { get; private set; }
    public bool MetaReady { get; private set; }

    public Vector3 XrealPosition { get; private set; }
    public Quaternion XrealRotation { get; private set; }
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

    public void SubmitMeta(Vector3 position, Quaternion rotation)
    {
        RPC_SubmitPlacement(position, rotation, true);
    }

    public void SubmitXreal(Vector3 position, Quaternion rotation)
    {
        RPC_SubmitPlacement(position, rotation, false);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_SubmitPlacement(
        Vector3 position,
        Quaternion rotation,
        bool isMeta)
    {
        if (isMeta)
        {
            MetaPosition = position;
            MetaRotation = rotation;
            MetaReady = true;

            Debug.Log($"META RECEIVED: {position}");
        }
        else
        {
            XrealPosition = position;
            XrealRotation = rotation;
            XrealReady = true;

            Debug.Log($"XREAL RECEIVED: {position}");
        }

        if (MetaReady && XrealReady)
        {
            Debug.Log("BOTH PLAYERS READY");
        }
    }
}
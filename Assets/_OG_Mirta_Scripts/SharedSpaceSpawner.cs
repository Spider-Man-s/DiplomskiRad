using Fusion;
using UnityEngine;

/// <summary>
/// Optional helper for spawning NetworkObjects at canonical Shared Space poses.
/// Put this on a NetworkObject whose instance has State Authority and call SpawnAtSharedPose.
/// </summary>
public class SharedSpaceSpawner : NetworkBehaviour
{
    public NetworkObject SpawnAtSharedPose(NetworkObject prefab, Vector3 sharedPosition, Vector3 sharedEulerDegrees)
    {
        if (!Object.HasStateAuthority)
        {
            Debug.LogWarning("SharedSpaceSpawner: only State Authority may spawn through this helper.");
            return null;
        }

        SharedSpaceManager manager = SharedSpaceManager.Instance;
        if (manager == null || !manager.IsCalibrated)
        {
            Debug.LogWarning("SharedSpaceSpawner: local Shared Space is not calibrated yet.");
            return null;
        }

        Pose sharedPose = new Pose(sharedPosition, Quaternion.Euler(sharedEulerDegrees));
        Pose localWorldPose = manager.SharedToWorld(sharedPose);

        NetworkObject spawned = Runner.Spawn(prefab, localWorldPose.position, localWorldPose.rotation);

        SharedSpaceNetworkTransform sharedTransform = spawned.GetComponent<SharedSpaceNetworkTransform>();
        if (sharedTransform != null)
            sharedTransform.SetSharedPoseAsAuthority(sharedPose);
        else
            Debug.LogWarning($"{prefab.name} was spawned without SharedSpaceNetworkTransform.");

        return spawned;
    }
}

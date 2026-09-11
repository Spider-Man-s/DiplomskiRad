using UnityEngine;

/// <summary>
/// Helper for spatial interaction data that is not represented by a Transform, e.g. ray origins,
/// directions and world hit points. Only use if those spatial values themselves must cross the network.
/// Semantic events such as "Button 7 pressed" do not need coordinate conversion.
/// </summary>
public static class SharedSpaceRayUtility
{
    public static Ray WorldRayToShared(Ray worldRay)
    {
        SharedSpaceManager manager = SharedSpaceManager.Instance;
        Vector3 origin = manager.WorldPointToShared(worldRay.origin);
        Vector3 direction = manager.WorldDirectionToShared(worldRay.direction).normalized;
        return new Ray(origin, direction);
    }

    public static Ray SharedRayToWorld(Ray sharedRay)
    {
        SharedSpaceManager manager = SharedSpaceManager.Instance;
        Vector3 origin = manager.SharedPointToWorld(sharedRay.origin);
        Vector3 direction = manager.SharedDirectionToWorld(sharedRay.direction).normalized;
        return new Ray(origin, direction);
    }
}

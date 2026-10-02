using Fusion;
using UnityEngine;
using static Unity.Collections.Unicode;

public class TableSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject tablePrefab;
    [SerializeField] private NetworkObject boxPartPrefab;
    [SerializeField] private NetworkObject foldableBoxPrefab;
    [SerializeField] private NetworkObject playgroundPrefab;
    [SerializeField] private Vector3 tableOffset = new Vector3(0.602f, 0.017f, 0.395f);
    [SerializeField] private Vector3 tableRotation = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 foldableBoxOffset = new Vector3(0.509339094f, 0.0559998825f, 0.491354972f);

    [SerializeField] private Vector3 playgroundOffset = new Vector3(-0.75f, 0.017f, -0.104f);

    public bool FoldableOrNot = false;

    private bool tableSpawned = false;


    public void SpawnSharedTable()
    {
        if (tableSpawned) return;

        if (FoldableOrNot)
        {
            EnableAllHandCollidersRpc();
        }

        NetworkObject prefab = FoldableOrNot ? foldableBoxPrefab : tablePrefab;
        Vector3 offset = FoldableOrNot ? foldableBoxOffset : tableOffset;

        NetworkObject root = SharedSpaceSpawner.Instance.SpawnAtSharedPose(prefab, offset, tableRotation);
        if (root == null)
        {
            Debug.LogError($"Failed to spawn {prefab.name}.");
            return;
        }

        if (!FoldableOrNot)
        {
            DisableAllHandCollidersRpc();
            foreach (BoxSideSpawn spawn in root.GetComponentsInChildren<BoxSideSpawn>())
                FusionBoot.Instance.Runner.Spawn(boxPartPrefab, spawn.transform.position, spawn.transform.rotation);
        }

        tableSpawned = true;
        Debug.Log($"Spawned {prefab.name} at offset {offset} from shared origin.");

        //spawn playground pa i collidere
        NetworkObject playground = SharedSpaceSpawner.Instance.SpawnAtSharedPose(playgroundPrefab, playgroundOffset, tableRotation);
        EnableAllHandCollidersRpc();
    }

    public void setFoldableOrNot(bool foldable)
    {
        FoldableOrNot = foldable;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void EnableAllHandCollidersRpc()
    {
        HandColliderManager.Instance.EnableAllHandColliders();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void DisableAllHandCollidersRpc()
    {
        HandColliderManager.Instance.DisableAllHandColliders();
    }
}

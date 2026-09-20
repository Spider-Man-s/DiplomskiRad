using Fusion;
using UnityEngine;
using static Unity.Collections.Unicode;

public class TableSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject tablePrefab;
    [SerializeField] private NetworkObject boxPartPrefab;
    [SerializeField] private NetworkObject foldableBoxPrefab;
    [SerializeField] private Vector3 tableOffset = new Vector3(0.602f, 0.017f, 0.395f);
    [SerializeField] private Vector3 tableRotation = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 foldableBoxOffset = new Vector3(0.509339094f, 0.0559998825f, 0.491354972f);

    public bool FoldableOrNot = false;

    private bool tableSpawned = false;
    private SharedSpaceSpawner spawner;

    private void Start()
    {
        spawner = GetComponent<SharedSpaceSpawner>();
        if (spawner == null)
            Debug.LogError("SharedSpaceSpawner component is missing.");

    }
    public void SpawnSharedTable()
    {
        if (tableSpawned) return;

        if (spawner == null)
        {
            Debug.LogError("SharedSpaceSpawner component is missing.");
            return;
        }

        NetworkObject prefab = FoldableOrNot ? foldableBoxPrefab : tablePrefab;
        Vector3 offset = FoldableOrNot ? foldableBoxOffset : tableOffset;

        NetworkObject root = spawner.SpawnAtSharedPose(prefab, offset, tableRotation);
        if (root == null)
        {
            Debug.LogError($"Failed to spawn {prefab.name}.");
            return;
        }

        if (!FoldableOrNot)
        {
            foreach (BoxSideSpawn spawn in root.GetComponentsInChildren<BoxSideSpawn>())
                Runner.Spawn(boxPartPrefab, spawn.transform.position, spawn.transform.rotation);
        }

        tableSpawned = true;
        Debug.Log($"Spawned {prefab.name} at offset {offset} from shared origin.");
    }

    public void setFoldableOrNot(bool foldable)
    {
        FoldableOrNot = foldable;
    }
}

using Fusion;
using UnityEngine;
using static Unity.Collections.Unicode;

public class TableSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject tablePrefab;
    [SerializeField] private NetworkObject boxPartPrefab;
    [SerializeField] private Vector3 tableOffset = new Vector3(0.602f, 0.017f, 0.395f);
    [SerializeField] private Vector3 tableRotation = new Vector3(0f, 0f, 0f);

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
        NetworkObject table = null;
        if (spawner != null)
        {
            table = spawner.SpawnAtSharedPose(tablePrefab, tableOffset, tableRotation);
        }

        BoxSideSpawn[] spawns = table.GetComponentsInChildren<BoxSideSpawn>();
        foreach (BoxSideSpawn spawn in spawns)
        {
            Runner.Spawn(boxPartPrefab, spawn.transform.position, spawn.transform.rotation);
            Debug.Log($"Spawned box at {spawn.name}");
        }
        tableSpawned = true;

        Debug.Log($"Table spawned at fixed offset {tableOffset} from shared origin.");
    }
}

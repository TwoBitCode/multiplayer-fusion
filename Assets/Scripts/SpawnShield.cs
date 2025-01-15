using Fusion;
using UnityEngine;

public class ShieldSpawner : NetworkBehaviour
{
    [SerializeField] NetworkObject ObjectToSpawn;
    [SerializeField] Vector3 spawnAreaMin; // Minimum bounds of the spawn area
    [SerializeField] Vector3 spawnAreaMax; // Maximum bounds of the spawn area

    [Networked] private bool ShieldSpawned { get; set; } // Track if the shield has been spawned

    public override void Spawned()
    {
        // Call the spawn function on initialization
        // Only the State Authority (host/server) should decide to spawn the shield
        if (HasStateAuthority && !ShieldSpawned)
        {
            SpawnRandomObject();
        }
    }


    // This method will spawn the object at a random position within the defined area
    public void SpawnRandomObject()
    {
        if (ObjectToSpawn == null) return;

        // Generate random position within the floor area
        float randomX = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float randomZ = Random.Range(spawnAreaMin.z, spawnAreaMax.z);
        float spawnY = spawnAreaMin.y; // Assuming a fixed y-value for the floor height

        Vector3 spawnPosition = new Vector3(randomX, spawnY, randomZ);

        // Using Fusion Network.Instantiate to spawn the object and sync across all clients
        Runner.Spawn(ObjectToSpawn, spawnPosition, Quaternion.identity, Object.InputAuthority);

        // Mark the shield as spawned
        ShieldSpawned = true;
    }

}

using UnityEngine;

public class SpawnFood : MonoBehaviour
{
    public GameObject entity;
    public GameObject parent;
    public int amount;
    public float spawnTime;
    public float time;
    public int spawnLimit;

    // Spawn boundaries
    public Vector2 min = new Vector2(-8f, -4f);
    public Vector2 max = new Vector2(8f, 4f);

    // Attempts before giving up on finding a valid location
    private const int maxAttempts = 20;

    void Start()
    {
        for (int i = 0; i < amount; i++)
        {
            Vector3 spawnPos = GetValidSpawnPosition();
            if (spawnPos != Vector3.negativeInfinity)
            {
                SpawnEntity(spawnPos);
            }
            else
            {
                Debug.LogWarning("Failed to find valid spawn position during Start()");
            }
        }
    }

    void Update()
    {
        time += 1;

        Entity[] entities = FindObjectsByType<Entity>(FindObjectsSortMode.None);
        int count = 0;

        foreach (var e in entities)
        {
            if (e.data.id == entity.GetComponent<Entity>().data.id)
            {
                count++;
            }
        }

        if (time >= spawnTime && count < spawnLimit)
        {
            Vector3 spawnPos = GetValidSpawnPosition();
            if (spawnPos != Vector3.negativeInfinity)
            {
                SpawnEntity(spawnPos);
            }
            else
            {
                Debug.LogWarning("Failed to find valid spawn position during Update()");
            }

            time = 0;
        }
    }

    // Spawn helper
    private void SpawnEntity(Vector3 pos)
    {
        GameObject clone = Instantiate(entity);
        clone.transform.parent = parent.transform;
        clone.transform.position = pos;
    }

    // Returns a valid spawn position or Vector3.negativeInfinity if none found
    private Vector3 GetValidSpawnPosition()
    {
        Collider2D prefabCol = entity.GetComponent<Collider2D>();
        Vector2 size = prefabCol != null ? prefabCol.bounds.size : new Vector2(0.5f, 0.5f);

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2 pos = new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));

            // Overlap check
            Collider2D hit = Physics2D.OverlapBox(pos, size, 0f);

            if (hit == null)
            {
                return new Vector3(pos.x, pos.y, 0);
            }
        }

        // No valid position found
        return Vector3.negativeInfinity;
    }

    // Visualize spawn check area in Scene view
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(max.x - min.x, max.y - min.y, 0));
    }
#endif
}

using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class WorldGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float chunkSize = 100f;
    [SerializeField] private int activeChunkRadius = 2;

    [Header("Per-Chunk Assets")]
    [SerializeField, Min(0)] private int maxAssetPerChunk = 256;

    [Header("Player")]
    [SerializeField] private Transform player;

    private readonly Dictionary<Vector2Int, GameObject> chunkRoots = new();
    private readonly Dictionary<Vector2Int, List<ClusterController>> clustersByChunk = new();

    private Vector2Int lastPlayerChunk;
    private bool hasPlayerChunk;

    public Transform Player => player;
    public float ChunkSize => chunkSize;
    public int ActiveChunkRadius => activeChunkRadius;


    private void Awake()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Start()
    {
        if (player != null)
        {
            lastPlayerChunk = WorldToChunk(player.position);
            hasPlayerChunk = true;
            UpdateActiveChunks(force: true);
        }
    }

    private void Update()
    {
        if (player == null) return;

        Vector2Int current = WorldToChunk(player.position);
        if (!hasPlayerChunk || current != lastPlayerChunk)
        {
            lastPlayerChunk = current;
            hasPlayerChunk = true;
            UpdateActiveChunks(force: false);
        }
    }

    public Vector2Int WorldToChunk(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / chunkSize);
        int z = Mathf.FloorToInt(worldPos.z / chunkSize);
        return new Vector2Int(x, z);
    }

    public void RegisterCluster(ClusterController cluster)
    {
        if (cluster == null) return;

        Vector2Int coord = WorldToChunk(cluster.ClusterOrigin);
        cluster.SetChunkCoord(coord);

        GameObject root = GetOrCreateChunkRoot(coord);
        cluster.transform.SetParent(root.transform, worldPositionStays: true);

        if (!clustersByChunk.TryGetValue(coord, out var list))
        {
            list = new List<ClusterController>(8);
            clustersByChunk.Add(coord, list);
        }
        if (!list.Contains(cluster)) list.Add(cluster);

        if (hasPlayerChunk)
        {
            bool active = IsWithinActiveRadius(coord, lastPlayerChunk);
            root.SetActive(active);
        }
    }

    public bool IsPositionTooClose(Vector3 candidatePos, float radius, Vector2Int candidateChunkCoord)
    {
        float radius2 = radius * radius;

        for (int z = -1; z <= 1; z++)
        {
            for (int x = -1; x <= 1; x++)
            {
                Vector2Int coord = new(candidateChunkCoord.x + x, candidateChunkCoord.y + z);
                if (!clustersByChunk.TryGetValue(coord, out var list)) continue;

                if (chunkRoots.TryGetValue(coord, out var root) && root != null && !root.activeInHierarchy)
                    continue;

                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var c = list[i];
                    if (c == null) { list.RemoveAt(i); continue; }
                    if (c.IsTooClose(candidatePos, radius2)) return true;
                }
            }
        }
        return false;
    }

    private GameObject GetOrCreateChunkRoot(Vector2Int coord)
    {
        if (chunkRoots.TryGetValue(coord, out var root) && root != null)
            return root;

        root = new GameObject($"Chunk_{coord.x}_{coord.y}");
        root.transform.SetParent(transform, worldPositionStays: true);

        var runner = root.AddComponent<TerrainAssetManagement>();
        runner.ConfigureBudget(maxAssetPerChunk);

        chunkRoots[coord] = root;
        return root;
    }

    private void UpdateActiveChunks(bool force)
    {
        if (!hasPlayerChunk) return;

        foreach (var kvp in chunkRoots)
        {
            Vector2Int coord = kvp.Key;
            GameObject root = kvp.Value;
            if (root == null) continue;

            bool active = IsWithinActiveRadius(coord, lastPlayerChunk);
            if (force || root.activeSelf != active)
                root.SetActive(active);
        }
    }

    private bool IsWithinActiveRadius(Vector2Int coord, Vector2Int playerCoord)
    {
        int dx = Mathf.Abs(coord.x - playerCoord.x);
        int dz = Mathf.Abs(coord.y - playerCoord.y);
        return dx <= activeChunkRadius && dz <= activeChunkRadius;
    }

    public void ClearAll()
    {
        foreach (var kvp in chunkRoots)
            if (kvp.Value != null) Destroy(kvp.Value);

        chunkRoots.Clear();
        clustersByChunk.Clear();
        hasPlayerChunk = false;
    }
}

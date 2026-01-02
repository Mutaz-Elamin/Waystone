using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class ClusterController : MonoBehaviour
{
    [Header("Cluster Info")]
    [SerializeField] private Vector3 clusterOrigin;
    [SerializeField] private int targetCount;

    [Header("Spawned Prefab")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private float clusterSpread = 10f;

    [Header("Instance Randomisation")]
    [SerializeField] private float minScale = 1f;
    [SerializeField] private float maxScale = 1f;
    [SerializeField] private float minXrot = 0f;
    [SerializeField] private float maxXrot = 0f;
    [SerializeField] private float minYrot = 0f;
    [SerializeField] private float maxYrot = 360f;
    [SerializeField] private float minZrot = 0f;
    [SerializeField] private float maxZrot = 0f;

    [Header("Placement Rules")]
    [SerializeField, Range(0f, 1f)] private float minHeight01 = 0f;
    [SerializeField, Range(0f, 1f)] private float maxHeight01 = 1f;

    [Header("Overlap Avoidance")]
    [SerializeField] private bool overlapAvoid = false;
    [SerializeField] private float overlapAvoidanceRadius = 2f;

    [Header("Runtime Cluster Behaviour")]
    [SerializeField, Min(0.1f)] private float checkInterval = 5f;
    [SerializeField] private ClusterRateSettings spawnSettings = default;
    [SerializeField] private ClusterRateSettings despawnSettings = default;

    private WorldGrid grid;
    private PrefabPool pool;
    private WorldBorder worldBorder;
    private TerrainAssetManagement chunkRunner;

    private Terrain terrain;
    private Vector3 terrainPos;
    private Vector3 terrainSize;

    private float minWorldHeight;
    private float maxWorldHeight;

    private Coroutine routine;
    private bool isInitialised;
    private Vector2Int chunkCoord;

    public Vector3 ClusterOrigin => clusterOrigin;

    public void Initialise(
        Vector3 origin,
        int targetCount,
        GameObject prefab,
        float clusterSpread,
        float minScale,
        float maxScale,
        float minXrot,
        float maxXrot,
        float minYrot,
        float maxYrot,
        float minZrot,
        float maxZrot,
        float minHeight01,
        float maxHeight01,
        bool overlapAvoid,
        float overlapAvoidanceRadius,
        float checkInterval,
        ClusterRateSettings spawnSettings,
        ClusterRateSettings despawnSettings,
        float minWorldHeight,
        float maxWorldHeight,
        WorldGrid grid,
        PrefabPool pool,
        WorldBorder worldBorder)
    {
        clusterOrigin = origin;
        this.targetCount = Mathf.Max(0, targetCount);

        this.prefab = prefab;
        this.clusterSpread = Mathf.Max(0.01f, clusterSpread);

        this.minScale = minScale;
        this.maxScale = Mathf.Max(minScale, maxScale);

        this.minXrot = minXrot; this.maxXrot = maxXrot;
        this.minYrot = minYrot; this.maxYrot = maxYrot;
        this.minZrot = minZrot; this.maxZrot = maxZrot;

        this.minHeight01 = Mathf.Clamp01(minHeight01);
        this.maxHeight01 = Mathf.Clamp01(Mathf.Max(minHeight01, maxHeight01));

        this.overlapAvoid = overlapAvoid;
        this.overlapAvoidanceRadius = Mathf.Max(0f, overlapAvoidanceRadius);

        this.checkInterval = Mathf.Max(0.1f, checkInterval);
        this.spawnSettings = spawnSettings;
        this.despawnSettings = despawnSettings;

        this.minWorldHeight = minWorldHeight;
        this.maxWorldHeight = Mathf.Max(minWorldHeight + 0.01f, maxWorldHeight);

        this.grid = grid;
        this.pool = pool;
        this.worldBorder = worldBorder;

        transform.position = clusterOrigin;

        terrain = Terrain.activeTerrain;
        if (terrain != null)
        {
            terrainPos = terrain.transform.position;
            terrainSize = terrain.terrainData.size;
        }

        chunkRunner = GetComponentInParent<TerrainAssetManagement>();
        isInitialised = true;

        if (isActiveAndEnabled && Application.isPlaying)
            StartClusterRoutine();
    }

    internal void SetChunkCoord(Vector2Int coord) => chunkCoord = coord;

    private void OnEnable()
    {
        if (!Application.isPlaying || !isInitialised) return;
        chunkRunner = GetComponentInParent<TerrainAssetManagement>();
        StartClusterRoutine();
    }

    private void OnDisable()
    {
        if (routine != null) { StopCoroutine(routine); routine = null; }
    }

    private void StartClusterRoutine()
    {
        if (routine != null) return;
        routine = StartCoroutine(ClusterRoutine());
    }

    private IEnumerator ClusterRoutine()
    {
        WaitForSeconds wait = new(checkInterval);
        while (true)
        {
            TickOnce();
            yield return wait;
        }
    }

    public void SpawnInitialPopulation()
    {
        if (!isInitialised) return;

        int spawned = 0;
        int attempts = Mathf.Max(4, targetCount * 2);

        for (int i = 0; i < attempts && spawned < targetCount; i++)
            if (TrySpawnOne()) spawned++;
    }

    public void ReturnAllToPool()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child == null) continue;

            GameObject go = child.gameObject;

            if (chunkRunner != null)
            {
                var a = go.GetComponent<TerrainAssetScript>();
                if (a != null) chunkRunner.UnregisterAsset(a);
            }

            if (pool != null) pool.Release(go);
            else Destroy(go);
        }
    }

    private void TickOnce()
    {
        if (!isInitialised || prefab == null) return;

        bool isNight = false; // Placeholder for day/night logic
        float playerDistance = (grid != null && grid.Player != null)
            ? Vector3.Distance(grid.Player.position, clusterOrigin)
            : float.PositiveInfinity;

        int live = transform.childCount;

        if (live < targetCount)
        {
            if (spawnSettings.CanAttempt(isNight, playerDistance) && Random.value <= spawnSettings.GetChance(isNight))
                TrySpawnOne();
        }
        else if (live > 0)
        {
            if (despawnSettings.CanAttempt(isNight, playerDistance) && Random.value <= despawnSettings.GetChance(isNight))
                TryDespawnOne();
        }
    }

    private bool TrySpawnOne()
    {
        if (!TryFindSpawnPosition(out Vector3 spawnPos))
            return false;

        Quaternion rot = Quaternion.Euler(
            Random.Range(minXrot, maxXrot),
            Random.Range(minYrot, maxYrot),
            Random.Range(minZrot, maxZrot));

        GameObject instance = pool != null
            ? pool.Get(prefab, spawnPos, rot, transform)
            : Instantiate(prefab, spawnPos, rot, transform);

        float sc = Random.Range(minScale, maxScale);
        instance.transform.localScale *= sc;

        if (chunkRunner != null)
        {
            var script = instance.GetComponent<TerrainAssetScript>();
            if (script != null) chunkRunner.RegisterAsset(script);
        }

        return true;
    }

    private void TryDespawnOne()
    {
        int count = transform.childCount;
        if (count <= 0) return;

        int idx = Random.Range(0, count);
        Transform t = transform.GetChild(idx);
        if (t == null) return;

        GameObject go = t.gameObject;

        if (chunkRunner != null)
        {
            var script = go.GetComponent<TerrainAssetScript>();
            if (script != null) chunkRunner.UnregisterAsset(script);
        }

        if (pool != null) pool.Release(go);
        else Destroy(go);
    }

    public bool IsTooClose(Vector3 candidatePos, float radiusSqr)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == null) continue;

            if ((child.position - candidatePos).sqrMagnitude < radiusSqr)
                return true;
        }
        return false;
    }

    private bool TryFindSpawnPosition(out Vector3 worldPos)
    {
        worldPos = clusterOrigin;

        const int attempts = 12;
        for (int i = 0; i < attempts; i++)
        {

            Vector2 offset = Random.insideUnitCircle * clusterSpread;
            float x = clusterOrigin.x + offset.x;
            float z = clusterOrigin.z + offset.y;

            float y = clusterOrigin.y;

            if (terrain != null)
            {
                float tx = Mathf.InverseLerp(terrainPos.x, terrainPos.x + terrainSize.x, x);
                float tz = Mathf.InverseLerp(terrainPos.z, terrainPos.z + terrainSize.z, z);
                if (tx < 0f || tx > 1f || tz < 0f || tz > 1f)
                    continue;

                y = terrain.SampleHeight(new Vector3(x, 0f, z)) + terrainPos.y;

                float h01 = Mathf.InverseLerp(minWorldHeight, maxWorldHeight, y);
                if (h01 < minHeight01 || h01 > maxHeight01)
                    continue;
            }

            Vector3 candidate = new(x, y, z);


            if (worldBorder != null && !worldBorder.IsInsideWorld(candidate))
                continue;

            if (overlapAvoid && overlapAvoidanceRadius > 0f && grid != null)
            {
                Vector2Int cChunk = grid.WorldToChunk(candidate);
                if (grid.IsPositionTooClose(candidate, overlapAvoidanceRadius, cChunk))
                    continue;
            }

            worldPos = candidate;
            return true;
        }

        return false;
    }
}

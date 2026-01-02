using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class WorldBorder : MonoBehaviour
{
    [Header("Shape")]
    [SerializeField, Range(0.1f, 1f)] private float radiusSize = 0.9f;
    [SerializeField, Range(0f, 0.5f)] private float irregularity = 0.12f;
    [SerializeField, Min(0.1f)] private float noiseScale = 2.2f;

    [SerializeField, Range(1, 8)] private int octaves = 4;
    [SerializeField, Range(0.1f, 0.9f)] private float persistence = 0.5f;
    [SerializeField, Range(1.2f, 3f)] private float lacunarity = 2f;

    [Header("Warp")]
    [SerializeField, Range(0f, 0.4f)] private float warpStrength = 0.12f;
    [SerializeField, Min(0.1f)] private float warpScale = 1.1f;

    [Header("Mountains")]
    [SerializeField] private bool useMountains = true;

    [SerializeField, Range(0f, 0.25f)] private float innerWidth = 0.06f;
    [SerializeField, Range(0f, 0.35f)] private float outerWidth = 0.10f;

    [SerializeField, Range(0f, 1f)] private float ridgeAddHeight01 = 0.20f;
    [SerializeField, Range(0f, 0.5f)] private float ridgeNoiseAmplitude = 0.10f;
    [SerializeField, Min(0.1f)] private float ridgeNoiseScale = 8f;

    [SerializeField] private AnimationCurve innerCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve outerCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Spawn Blocking")]
    [SerializeField, Range(0f, 0.25f)] private float noSpawnNearBorder = 0.06f;

    [Header("Border Assets")]
    [SerializeField] private bool useBorderAssets = false;
    [SerializeField] private GameObject borderAssetPrefab;

    [SerializeField, Min(0.5f)] private float borderSpacing = 1f;
    [SerializeField, Min(0f)] private float borderInset = 10f;

    [SerializeField, Min(0)] private int extraBehindCount = 2;
    [SerializeField, Min(0f)] private float extraBehindMin = 2f;
    [SerializeField, Min(0f)] private float extraBehindMax = 12f;
    [SerializeField, Min(0f)] private float extraTangentJitter = 4f;

    [SerializeField] private bool borderOverlapAvoid = true;
    [SerializeField, Min(0f)] private float borderOverlapRadius = 1f;

    [SerializeField] private float borderMinScale = 1;
    [SerializeField] private float borderMaxScale = 1f;

    [SerializeField] private float borderMinYRot = 0f;
    [SerializeField] private float borderMaxYRot = 0f;

    [Header("Border Wall")]
    [SerializeField] private bool useBorderWall = true;
    [SerializeField, Min(0.5f)] private float wallSpacing = 1f;
    [SerializeField, Min(0f)] private float wallInset = 3f;
    [SerializeField, Min(0.1f)] private float wallRadius = 1.5f;
    [SerializeField, Min(0.1f)] private float wallHeight = 200f;
    [SerializeField, Min(0f)] private float wallSink = 2f;
    [SerializeField] private bool carveNavMesh = true;

    [Header("Ceiling")]
    [SerializeField] private bool useCeiling = true;
    [SerializeField, Min(0.1f)] private float ceilingThickness = 2f;

    private readonly System.Collections.Generic.List<GameObject> borderSpawned = new System.Collections.Generic.List<GameObject>(2048);
    private readonly System.Collections.Generic.List<GameObject> wallSpawned = new System.Collections.Generic.List<GameObject>(1024);

    private Transform borderRoot;
    private Transform borderAssetsRoot;
    private Transform borderWallRoot;
    private GameObject ceilingObj;

    private Terrain cachedTerrain;
    private Vector3 terrainPos;
    private Vector3 terrainSize;
    private int seed;

    public void Initialise(Terrain terrain, int seed)
    {
        cachedTerrain = terrain;
        this.seed = seed;

        if (cachedTerrain != null)
        {
            terrainPos = cachedTerrain.transform.position;
            terrainSize = cachedTerrain.terrainData.size;
        }
    }

    public bool IsInsideWorld(Vector3 worldPos)
    {
        if (cachedTerrain == null) return true;

        float u = Mathf.InverseLerp(terrainPos.x, terrainPos.x + terrainSize.x, worldPos.x);
        float v = Mathf.InverseLerp(terrainPos.z, terrainPos.z + terrainSize.z, worldPos.z);

        if (u < 0f || u > 1f || v < 0f || v > 1f) return false;
        return IsInsideUV(u, v);
    }

    public bool IsInsideUV(float u01, float v01)
    {
        float cx = (u01 - 0.5f) * 2f;
        float cy = (v01 - 0.5f) * 2f;

        float r = Mathf.Sqrt(cx * cx + cy * cy);
        if (r < 0.000001f) return true;

        Vector2 dir = new Vector2(cx / r, cy / r);
        float rMax = RayToSquareMaxRadius(dir);
        if (rMax < 0.000001f) return true;

        float rNorm = r / rMax;
        float angle = Mathf.Atan2(cy, cx);
        float rBorder = BorderRadiusAtAngle(angle);

        return rNorm <= rBorder;
    }

    public void ApplyToHeights(float[,] heights)
    {
        if (heights == null) return;
        if (cachedTerrain == null) return;

        int h = heights.GetLength(0);
        int w = heights.GetLength(1);

        float inW = Mathf.Max(0f, innerWidth);
        float outW = Mathf.Max(0f, outerWidth);

        for (int y = 0; y < h; y++)
        {
            float v01 = y / (float)(h - 1);
            float cy = (v01 - 0.5f) * 2f;

            for (int x = 0; x < w; x++)
            {
                float u01 = x / (float)(w - 1);
                float cx = (u01 - 0.5f) * 2f;

                float r = Mathf.Sqrt(cx * cx + cy * cy);
                if (r < 0.000001f)
                    continue;

                Vector2 dir = new Vector2(cx / r, cy / r);
                float rMax = RayToSquareMaxRadius(dir);
                if (rMax < 0.000001f)
                    continue;

                float rNorm = r / rMax;
                float angle = Mathf.Atan2(cy, cx);
                float rBorder = BorderRadiusAtAngle(angle);

                float outerEnd = rBorder + outW;

                if (rNorm > outerEnd)
                {
                    heights[y, x] = 0f;
                    continue;
                }

                float original = heights[y, x];

                float ridge = original;
                if (useMountains)
                {
                    float n = RidgedNoise(u01, v01, ridgeNoiseScale);      // 0..1
                    float signed = (n - 0.5f) * 2f;                       // -1..1
                    float add = ridgeAddHeight01 + signed * ridgeNoiseAmplitude;
                    ridge = Mathf.Clamp01(original + Mathf.Max(0f, add));
                }

                if (outW > 0f && rNorm > rBorder)
                {
                    float t = Mathf.InverseLerp(rBorder, outerEnd, rNorm);
                    float wc = outerCurve != null ? outerCurve.Evaluate(t) : t;

                    heights[y, x] = Mathf.Lerp(ridge, 0f, Mathf.Clamp01(wc));
                    continue;
                }

                if (useMountains && inW > 0f)
                {
                    float innerStart = rBorder - inW;

                    if (rNorm > innerStart)
                    {
                        float t = Mathf.InverseLerp(innerStart, rBorder, rNorm);
                        float wc = innerCurve != null ? innerCurve.Evaluate(t) : t;

                        float target = Mathf.Max(original, ridge);
                        heights[y, x] = Mathf.Lerp(original, target, Mathf.Clamp01(wc));
                    }
                }
            }
        }
    }

    public void ApplyToSpawnMap(float[,] spawnMap)
    {
        if (spawnMap == null) return;

        int h = spawnMap.GetLength(0);
        int w = spawnMap.GetLength(1);

        float block = Mathf.Max(0f, noSpawnNearBorder);

        for (int y = 0; y < h; y++)
        {
            float v01 = y / (float)(h - 1);
            float cy = (v01 - 0.5f) * 2f;

            for (int x = 0; x < w; x++)
            {
                float u01 = x / (float)(w - 1);
                float cx = (u01 - 0.5f) * 2f;

                float r = Mathf.Sqrt(cx * cx + cy * cy);
                if (r < 0.000001f)
                    continue;

                Vector2 dir = new Vector2(cx / r, cy / r);
                float rMax = RayToSquareMaxRadius(dir);
                if (rMax < 0.000001f)
                {
                    spawnMap[y, x] = 0f;
                    continue;
                }

                float rNorm = r / rMax;
                float angle = Mathf.Atan2(cy, cx);
                float rBorder = BorderRadiusAtAngle(angle);

                if (rNorm > rBorder)
                {
                    spawnMap[y, x] = 0f;
                    continue;
                }

                if (block > 0f && rNorm > (rBorder - block))
                {
                    spawnMap[y, x] = 0f;
                }
            }
        }
    }

    private float BorderRadiusAtAngle(float angle)
    {
        float theta01 = (angle + Mathf.PI) / (2f * Mathf.PI);

        float warp = 0f;
        if (warpStrength > 0f)
        {
            float w = FbmOnCircle(theta01, warpScale);
            warp = (w - 0.5f) * 2f * warpStrength;
        }

        float n = FbmOnCircle(theta01 + warp, noiseScale);
        float signed = (n - 0.5f) * 2f;

        float r = radiusSize + signed * irregularity;
        return Mathf.Clamp01(r);
    }

    private float FbmOnCircle(float theta01, float scale)
    {
        float amp = 1f;
        float freq = 1f;
        float sum = 0f;
        float norm = 0f;

        float baseAngle = theta01 * 2f * Mathf.PI;

        for (int i = 0; i < octaves; i++)
        {
            float a = baseAngle * freq;

            float sx = Mathf.Cos(a) * scale + seed * 0.013f;
            float sy = Mathf.Sin(a) * scale + seed * 0.017f;

            float p = Mathf.PerlinNoise(sx, sy);

            sum += p * amp;
            norm += amp;

            amp *= persistence;
            freq *= lacunarity;
        }

        return norm > 0f ? (sum / norm) : 0.5f;
    }

    private float RidgedNoise(float u01, float v01, float scale)
    {
        float x = (u01 * scale) + seed * 0.021f;
        float y = (v01 * scale) + seed * 0.037f;
        float n = Mathf.PerlinNoise(x, y);
        return 1f - Mathf.Abs(2f * n - 1f);
    }

    private static float RayToSquareMaxRadius(Vector2 dir)
    {
        float ax = Mathf.Abs(dir.x);
        float ay = Mathf.Abs(dir.y);

        float tx = ax > 0.0001f ? (1f / ax) : float.PositiveInfinity;
        float ty = ay > 0.0001f ? (1f / ay) : float.PositiveInfinity;

        return Mathf.Min(tx, ty);
    }

    public void ClearBorderAssets(PrefabPool pool)
    {
        for (int i = borderSpawned.Count - 1; i >= 0; i--)
        {
            GameObject go = borderSpawned[i];
            if (go == null) continue;

            if (pool != null) pool.Release(go);
            else Destroy(go);
        }
        borderSpawned.Clear();

        for (int i = wallSpawned.Count - 1; i >= 0; i--)
        {
            GameObject go = wallSpawned[i];
            if (go == null) continue;
            Destroy(go);
        }
        wallSpawned.Clear();

        if (ceilingObj != null)
        {
            Destroy(ceilingObj);
            ceilingObj = null;
        }

        if (borderRoot != null)
        {
            Destroy(borderRoot.gameObject);
            borderRoot = null;
            borderAssetsRoot = null;
            borderWallRoot = null;
        }
    }


    public void GenerateBorderAssets(WorldGrid grid, PrefabPool pool)
    {
        ClearBorderAssets(pool);

        if (cachedTerrain == null) return;

        if (borderRoot == null)
        {
            GameObject r = new GameObject("Border");
            r.transform.SetParent(transform, worldPositionStays: true);
            borderRoot = r.transform;

            GameObject a = new GameObject("Assets");
            a.transform.SetParent(borderRoot, worldPositionStays: false);
            borderAssetsRoot = a.transform;

            GameObject w = new GameObject("Wall");
            w.transform.SetParent(borderRoot, worldPositionStays: false);
            borderWallRoot = w.transform;
        }

        float halfMin = Mathf.Min(terrainSize.x, terrainSize.z) * 0.5f;
        float approxPerimeter = 2f * Mathf.PI * halfMin * Mathf.Clamp01(radiusSize);

        Vector3 center = terrainPos + new Vector3(terrainSize.x * 0.5f, 0f, terrainSize.z * 0.5f);
        float topY = terrainPos.y + Mathf.Max(0.1f, wallHeight);

        if (useBorderWall)
        {
            int segs = Mathf.Max(16, Mathf.CeilToInt(approxPerimeter / Mathf.Max(0.5f, wallSpacing)));
            for (int i = 0; i < segs; i++)
            {
                float theta01 = i / (float)segs;
                if (TryGetBorderPointWorld(theta01, wallInset, out Vector3 p))
                    SpawnWallCollider(p, topY);
            }
        }

        if (useCeiling)
            SpawnCeiling(topY);

        if (!useBorderAssets || borderAssetPrefab == null)
            return;

        int assetSegs = Mathf.Max(16, Mathf.CeilToInt(approxPerimeter / Mathf.Max(0.5f, borderSpacing)));

        int extraCount = Mathf.Max(0, extraBehindCount);
        float minDepth = Mathf.Max(0f, extraBehindMin);
        float maxDepth = Mathf.Max(minDepth, extraBehindMax);
        float jitter = Mathf.Max(0f, extraTangentJitter);

        float rSqr = borderOverlapRadius * borderOverlapRadius;

        for (int i = 0; i < assetSegs; i++)
        {
            float theta01 = i / (float)assetSegs;

            if (!TryGetBorderPointWorld(theta01, borderInset, out Vector3 p))
                continue;

            Vector3 outward = p - center;
            outward.y = 0f;
            outward = outward.sqrMagnitude > 0.0001f ? outward.normalized : Vector3.forward;

            Vector3 inward = -outward;
            Vector3 tangent = new Vector3(-outward.z, 0f, outward.x);

            TrySpawnBorderAsset(grid, pool, p, rSqr);

            for (int e = 0; e < extraCount; e++)
            {
                float depth = Random.Range(minDepth, maxDepth);
                float tj = jitter > 0f ? Random.Range(-jitter, jitter) : 0f;

                Vector3 cand = p + inward * depth + tangent * tj;
                if (!IsInsideWorld(cand)) continue;

                float y = cachedTerrain.SampleHeight(new Vector3(cand.x, 0f, cand.z)) + terrainPos.y;
                cand = new Vector3(cand.x, y, cand.z);

                TrySpawnBorderAsset(grid, pool, cand, rSqr);
            }
        }
    }


    private bool TryGetBorderPointWorld(float theta01, float insetMeters, out Vector3 pos)
    {
        pos = default;

        if (cachedTerrain == null) return false;

        float angle = theta01 * 2f * Mathf.PI - Mathf.PI;

        float rBorder = BorderRadiusAtAngle(angle);

        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        float rMax = RayToSquareMaxRadius(dir);
        if (rMax < 0.000001f) return false;

        float halfMin = Mathf.Min(terrainSize.x, terrainSize.z) * 0.5f;
        float inset01 = (halfMin > 0.0001f) ? (insetMeters / halfMin) : 0f;

        float rNorm = Mathf.Clamp01(rBorder - inset01);
        float r = rNorm * rMax;

        float cx = dir.x * r;
        float cy = dir.y * r;

        float u01 = cx * 0.5f + 0.5f;
        float v01 = cy * 0.5f + 0.5f;

        float wx = terrainPos.x + u01 * terrainSize.x;
        float wz = terrainPos.z + v01 * terrainSize.z;
        float wy = cachedTerrain.SampleHeight(new Vector3(wx, 0f, wz)) + terrainPos.y;

        Vector3 p = new Vector3(wx, wy, wz);
        if (!IsInsideWorld(p)) return false;

        pos = p;
        return true;
    }

    private void TrySpawnBorderAsset(WorldGrid grid, PrefabPool pool, Vector3 pos, float rSqr)
    {
        if (borderOverlapAvoid && borderOverlapRadius > 0f)
        {
            for (int i = 0; i < borderSpawned.Count; i++)
            {
                GameObject existing = borderSpawned[i];
                if (existing == null) continue;
                if ((existing.transform.position - pos).sqrMagnitude < rSqr)
                    return;
            }
        }

        float yaw = (borderMaxYRot != borderMinYRot) ? Random.Range(borderMinYRot, borderMaxYRot) : borderMinYRot;
        Quaternion rot = Quaternion.Euler(0f, yaw, 0f);

        Transform parent = borderAssetsRoot;

        GameObject obj = (pool != null)
            ? pool.Get(borderAssetPrefab, pos, rot, parent)
            : Instantiate(borderAssetPrefab, pos, rot, parent);

        if (grid != null)
            grid.ParentToChunk(obj.transform, pos);

        float minS = Mathf.Min(borderMinScale, borderMaxScale);
        float maxS = Mathf.Max(borderMinScale, borderMaxScale);
        float sc = Random.Range(minS, maxS);
        obj.transform.localScale *= sc;

        borderSpawned.Add(obj);
    }

    private void SpawnWallCollider(Vector3 groundPos, float topY)
    {
        GameObject go = new GameObject("WallCollider");
        go.transform.SetParent(borderWallRoot, worldPositionStays: true);

        float baseY = groundPos.y - Mathf.Max(0f, wallSink);
        float height = Mathf.Max((wallRadius * 2f) + 0.01f, topY - baseY);

        go.transform.position = new Vector3(groundPos.x, baseY, groundPos.z);
        go.transform.rotation = Quaternion.identity;

        CapsuleCollider col = go.AddComponent<CapsuleCollider>();
        col.direction = 1;
        col.radius = Mathf.Max(0.01f, wallRadius);
        col.height = height;
        col.center = new Vector3(0f, height * 0.5f, 0f);

        if (carveNavMesh)
        {
            NavMeshObstacle o = go.AddComponent<NavMeshObstacle>();
            o.shape = NavMeshObstacleShape.Capsule;
            o.radius = col.radius;
            o.height = col.height;
            o.center = col.center;
            o.carving = true;
        }

        wallSpawned.Add(go);
    }

    private void SpawnCeiling(float topY)
    {
        ceilingObj = new GameObject("Ceiling");
        ceilingObj.transform.SetParent(borderRoot, worldPositionStays: true);

        Vector3 center = terrainPos + new Vector3(terrainSize.x * 0.5f, 0f, terrainSize.z * 0.5f);
        float th = Mathf.Max(0.1f, ceilingThickness);

        ceilingObj.transform.position = new Vector3(center.x, topY + th * 0.5f, center.z);
        ceilingObj.transform.rotation = Quaternion.identity;

        BoxCollider box = ceilingObj.AddComponent<BoxCollider>();
        box.size = new Vector3(terrainSize.x, th, terrainSize.z);
        box.center = Vector3.zero;
    }

}

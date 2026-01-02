using UnityEngine;

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
}

using JetBrains.Annotations;
using System;
using System.Runtime.CompilerServices;
using System.Xml;
using Unity.AI.Navigation;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.VFX;

public class RandomTerrain : MonoBehaviour
{
    [Header("Generated Terrain Settings")]
    [SerializeField] protected int heightmapResolution = 1025;
    [SerializeField] protected Vector3 terrainSize = new Vector3(1000, 1000, 1000);

    // Fields used to control terrain generation - for final version these shouldnt be serialized but set in code per biome
    [Header("Terrain Terafroming Generation Settings")]
    [SerializeField] protected float noiseScale = 10f;
    [SerializeField] protected float heightMultiplier = 0.03f;
    [SerializeField] protected AnimationCurve meshHeightCurve;
    [SerializeField] protected int seed = 0;
    [SerializeField] protected int octaves = 4;
    [SerializeField] protected float persistance = 0.5f;
    [SerializeField] protected float lacunarity = 2f;

    [Header("Falloff Map Settings")]
    [SerializeField] protected bool falloff = false;
    [Range(-10, 10)]
    [SerializeField] protected float falloffSlope;
    [Range(0, 10)]
    [SerializeField] protected float falloffPosition;
    [Range(0, 15)]
    [SerializeField] protected int falloffMultiplier;

    // Fields used for spawning assets
    [Header("Asset Spawning")]
    [SerializeField] protected bool spawnAssets = true;
    [SerializeField] protected bool spawnNpcs = true;
    [SerializeField] protected int spawnSeed = 0;
    [SerializeField] protected TerrainAsset[] assetPrefabs;
    [SerializeField] protected TerrainAsset[] npcPrefabs;
    [SerializeField] protected float assetNoiseScale = 20f;

    [Header("Streaming / Grid + Pool")]
    [SerializeField] private WorldBorder worldBorder;
    [SerializeField] private WorldGrid worldGrid;
    [SerializeField] private PrefabPool prefabPool;

    private readonly System.Collections.Generic.List<GameObject> spawnedAssets = new();
    private readonly System.Collections.Generic.List<GameObject> spawnedNpcs = new();

    // Fields for terrain types and colours - can be expanded for biomes
    [Header("Terrain Texturing")]
    [SerializeField] protected Material terrainMaterial;
    protected Texture2D terrainHeightTexture;
    [SerializeField] protected TerrainType[] regions;
    private float maxMultiplier;
    private float minWorldHeight;
    private float maxWorldHeight;

    // References to Terrain and TerrainData components data
    protected Terrain terrain;
    protected TerrainData terrainData;

    // NavMeshSurface reference for NavMesh building
    protected NavMeshSurface navSurface;

    // Initialize terrain generation on Awake
    private void Awake()
    {
        terrainData = new TerrainData
        {
            heightmapResolution = heightmapResolution,
            size = terrainSize
        };

        GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
        terrainObject.name = "Procedural Terrain";
        terrainObject.transform.parent = this.transform;
        terrainObject.transform.localPosition = Vector3.zero;
        terrainObject.layer = LayerMask.NameToLayer("NavMesh");

        terrain = terrainObject.GetComponent<Terrain>();

        if (terrainMaterial != null)
        {
            terrain.materialTemplate = new Material(terrainMaterial);
        }

        if (worldGrid == null)
        {
            worldGrid = GetComponent<WorldGrid>();
            if (worldGrid == null) worldGrid = gameObject.AddComponent<WorldGrid>();
        }

        if (prefabPool == null)
        {
            prefabPool = GetComponent<PrefabPool>();
            if (prefabPool == null) prefabPool = gameObject.AddComponent<PrefabPool>();
        }

        if (worldBorder == null)
        {
            worldBorder = UnityEngine.Object.FindFirstObjectByType<WorldBorder>();
            if (worldBorder == null)
                worldBorder = gameObject.AddComponent<WorldBorder>();
        }


        navSurface = terrainObject.AddComponent<NavMeshSurface>();
        navSurface.layerMask = LayerMask.GetMask("NavMesh");

        // Call terrain generation method
        GenerateTerrain();
    }

    // Method to regenerate terrain on key press for testing purposes - can be removed in final version
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Regenerate terrain with a new random seed for testing random seed quickly - commented out during most use
            seed = UnityEngine.Random.Range(0, 1000);
            spawnSeed = seed * 100;
            GenerateTerrain();
        }
        //GenerateTerrain(this.terrain, this.terrainData, this.noiseScale, this.heightMultiplier, this.seed);
    }

    // Optional: Method to set seeds externally
    public void SetSeed(int newSeed)
    {
        seed = newSeed;
        spawnSeed = newSeed * 100;
        GenerateTerrain();
    }

    // Method to get the current seed
    public int GetSeed()
    {
        return seed;
    }

    // Method to generate a noise map - can be used for heightmaps or other procedural generation needs
    protected float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float mapScale, int octaves, float persistance, float lacunarity, int mapSeed)
    {
        if (mapScale <= 0)
        {
            mapScale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float[,] noiseMap = new float[mapHeight, mapWidth];

        System.Random random = new System.Random(mapSeed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = random.Next(-1000, 1000);
            float offsetY = random.Next(-1000, 1000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float xCoord = (x / mapScale) * frequency + octaveOffsets[i].x;
                    float yCoord = (y / mapScale) * frequency + octaveOffsets[i].y;

                    float noiseHeightI = Mathf.PerlinNoise(xCoord, yCoord) * 2 - 1;
                    noiseHeight += noiseHeightI * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }
                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[y, x] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[y, x] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[y, x]);
            }
        }

        return noiseMap;
    }

    protected float[,] GenerateFalloffMap(int dim)
    {
        float[,] falloffMap = new float[dim, dim];

        for (int y = 0; y < dim; y++)
        {
            for (int x = 0; x < dim; x++)
            {
                float relY = y / (float)dim * 2 - 1;
                float relX = x / (float)dim * 2 - 1;

                float falloffValue = Mathf.Max(Mathf.Abs(relY), Mathf.Abs(relX));
                falloffValue = CalculateFalloffValue(falloffValue);
                falloffMap[y, x] += falloffValue;
            }
        }
        return falloffMap;
    }

    protected float CalculateFalloffValue(float value)
    {
        return Mathf.Pow(value, falloffSlope) / (Mathf.Pow(value, falloffSlope) + Mathf.Pow(falloffPosition - falloffPosition * value, falloffSlope));
    }

    // Main terrain generation method using Perlin noise
    private void GenerateTerrain()
    {
        int heightmapHeight = terrainData.heightmapResolution;
        int heightmapWidth = terrainData.heightmapResolution;

        float[,] noiseMap = GenerateNoiseMap(heightmapHeight, heightmapWidth, noiseScale, octaves, persistance, lacunarity, seed);
        float[,] falloffMap = GenerateFalloffMap(heightmapWidth);

        float[,] heightMap = new float[heightmapHeight, heightmapWidth];

        for (int y = 0; y < heightmapHeight; y++)
        {
            for (int x = 0; x < heightmapWidth; x++)
            {
                if (falloff)
                {
                    noiseMap[y, x] = Mathf.Clamp01(noiseMap[y, x] - falloffMap[y, x]);
                    noiseMap[y, x] = Mathf.Clamp01(noiseMap[y, x] * (1f + falloffMultiplier * 0.025f));
                }

                heightMap[y, x] = noiseMap[y, x] * heightMultiplier * meshHeightCurve.Evaluate(noiseMap[y, x]);
            }
        }

        worldBorder.Initialise(terrain, seed);
        worldBorder.ApplyToHeights(heightMap);

        terrainData.SetHeights(0, 0, heightMap);

        CheckHeightRange();

        ApplyTerrainTypesToShader();

        ReleaseAndDestroySpawnedClusters(spawnedAssets);
        ReleaseAndDestroySpawnedClusters(spawnedNpcs);

        if (worldGrid != null)
            worldGrid.ClearAll();

        if (spawnAssets)
        {
            float[,] spawnMap = GenerateNoiseMap(heightmapHeight, heightmapWidth, assetNoiseScale, octaves, persistance, lacunarity, spawnSeed);
            worldBorder.ApplyToSpawnMap(spawnMap);
            SpawnTerrainAssets(spawnMap, heightMap, assetPrefabs, spawnedAssets);
        }

        if (spawnNpcs)
        {
            float[,] npcMap = GenerateNoiseMap(heightmapHeight, heightmapWidth, assetNoiseScale, octaves, persistance, lacunarity, spawnSeed * 1000);
            worldBorder.ApplyToSpawnMap(npcMap);
            if (navSurface != null)
            {
                Debug.Log("Building NavMesh...");
                navSurface.BuildNavMesh();
            }
            else
            {
                Debug.LogError("NavMeshSurface missing on Terrain GameObject.");
            }
            SpawnTerrainAssets(npcMap, heightMap, npcPrefabs, spawnedNpcs);
        }
    }

    // Finds the highest y value on the curve - biggest height multiplier on the terrain/highest point
    protected float GetCurveMax(AnimationCurve curve)
    {
        float max = float.MinValue;
        foreach (var key in curve.keys)
        {
            if (key.value > max) max = key.value;
        }
        return Mathf.Max(max, 0f);
    }

    // Sets the min and max possible heights of the terrain - terrain types are therfore relative to possible extremes
    protected void CheckHeightRange()
    {
        if (terrain == null || terrain.materialTemplate == null) return;

        float curveMax = GetCurveMax(meshHeightCurve);
        maxMultiplier = Mathf.Clamp01(heightMultiplier * curveMax);

        minWorldHeight = terrain.transform.position.y;
        maxWorldHeight = minWorldHeight + maxMultiplier * terrainData.size.y;

        Material material = terrain.materialTemplate;
        material.SetFloat("minHeight", minWorldHeight);
        material.SetFloat("maxHeight", maxWorldHeight);
    }

    // Updated colouring of terrain mesh using custom shader - new colour method
    protected void ApplyTerrainTypesToShader()
    {
        if (terrain == null || terrain.materialTemplate == null || regions == null || regions.Length == 0)
            return;

        Material mat = terrain.materialTemplate;

        int regionCount = Mathf.Min(regions.Length, 8);

        Color[] colours = new Color[regionCount];
        float[] heights = new float[regionCount];
        float[] blends = new float[regionCount];

        for (int i = 0; i < regionCount; i++)
        {
            colours[i] = regions[i].Colour;
            heights[i] = Mathf.Clamp01(regions[i].StartHeight);
            blends[i] = Mathf.Clamp01(regions[i].Blend * 0.01f);
        }

        mat.SetInt("regionsCount", regionCount);
        mat.SetColorArray("baseColours", colours);
        mat.SetFloatArray("baseHeights", heights);
        mat.SetFloatArray("baseBlends", blends);
    }

    private void ReleaseAndDestroySpawnedClusters(System.Collections.Generic.List<GameObject> spawned)
    {
        if (spawned == null) return;

        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            GameObject go = spawned[i];
            if (go == null) continue;

            ClusterController cluster = go.GetComponent<ClusterController>();
            if (cluster != null)
                cluster.ReturnAllToPool();

            Destroy(go);
        }

        spawned.Clear();
    }

    // Method to randomly spawn assets across the terrain
    private void SpawnTerrainAssets(float[,] spawnMap, float[,] heightMap, TerrainAsset[] assets, System.Collections.Generic.List<GameObject> spawnedArray)
    {
        if (assets == null || assets.Length == 0)
        {
            Debug.LogWarning("No asset prefabs assigned for terrain asset spawning.");
            return;
        }

        if (spawnMap == null || heightMap == null)
            return;

        int height = spawnMap.GetLength(0);
        int width = spawnMap.GetLength(1);

        Vector3 terrainPos = terrain.transform.position;
        Vector3 terrainSize = terrainData.size;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float spawnRate = spawnMap[y, x];

                float currentHeight = 0f;
                if (maxMultiplier > 0f)
                    currentHeight = Mathf.Clamp01(heightMap[y, x] / maxMultiplier);

                for (int i = 0; i < assets.Length; i++)
                {
                    TerrainAsset asset = assets[i];

                    if (!SpawnAssetRequirements(asset, x, y, spawnRate, currentHeight, true))
                        continue;

                    float xNorm = (float)x / (width - 1);
                    float zNorm = (float)y / (height - 1);

                    float worldX = terrainPos.x + xNorm * terrainSize.x;
                    float worldZ = terrainPos.z + zNorm * terrainSize.z;
                    float worldY = terrain.SampleHeight(new Vector3(worldX, 0f, worldZ)) + terrainPos.y;

                    Vector3 clusterOrigin = new Vector3(worldX, worldY, worldZ);

                    int minCount = Mathf.Max(1, asset.ClusterMinCount);
                    int maxCount = Mathf.Max(minCount, asset.ClusterMaxCount);
                    int clusterCount = UnityEngine.Random.Range(minCount, maxCount + 1);

                    if (asset.OverlapAvoid && asset.OverlapAvoidanceRadius > 0f && worldGrid != null)
                    {
                        Vector2Int chunk = worldGrid.WorldToChunk(clusterOrigin);
                        if (worldGrid.IsPositionTooClose(clusterOrigin, asset.OverlapAvoidanceRadius, chunk))
                            continue;
                    }

                    GameObject clusterRoot = new GameObject(asset.AssetName + " Cluster");
                    clusterRoot.transform.position = clusterOrigin;

                    ClusterController cluster = clusterRoot.AddComponent<ClusterController>();
                    cluster.Initialise(
                        clusterOrigin,
                        clusterCount,
                        asset.AssetPrefab,
                        asset.ClusterSpread,
                        asset.MinScale,
                        asset.MaxScale,
                        asset.MinXrotation,
                        asset.MaxXrotation,
                        asset.MinYrotation,
                        asset.MaxYrotation,
                        asset.MinZrotation,
                        asset.MaxZrotation,
                        asset.MinHeight,
                        asset.MaxHeight,
                        asset.OverlapAvoid,
                        asset.OverlapAvoidanceRadius,
                        Mathf.Max(0.1f, asset.ClusterCheckInterval),
                        asset.ClusterSpawnSettings,
                        asset.ClusterDespawnSettings,
                        minWorldHeight,
                        maxWorldHeight,
                        worldGrid,
                        prefabPool,
                        worldBorder
                    );

                    if (worldGrid != null)
                        worldGrid.RegisterCluster(cluster);
                    else
                        clusterRoot.transform.SetParent(terrain.transform, true);

                    spawnedArray.Add(clusterRoot);

                    // Spawn initial members (uses pooling + grid overlap)
                    cluster.SpawnInitialPopulation();

                    // Only allow one asset type per map cell
                    break;
                }
            }
        }
    }

    protected bool IsTooCloseToOtherAssets(Vector3 candidatePos, float minRadius, System.Collections.Generic.List<GameObject> spawnedArray)
    {
        if (minRadius <= 0f)
            return false;

        if (worldGrid != null)
        {
            Vector2Int chunk = worldGrid.WorldToChunk(candidatePos);
            return worldGrid.IsPositionTooClose(candidatePos, minRadius, chunk);
        }

        if (spawnedArray == null || spawnedArray.Count == 0)
            return false;

        float minRadiusSqr = minRadius * minRadius;

        foreach (var obj in spawnedArray)
        {
            if (obj == null) continue;

            for (int i = 0; i < obj.transform.childCount; i++)
            {
                Transform t = obj.transform.GetChild(i);
                if (t == null) continue;

                if ((t.position - candidatePos).sqrMagnitude < minRadiusSqr)
                    return true;
            }
        }

        return false;
    }


    protected bool SpawnAssetRequirements(TerrainAsset asset, float x, float y, float spawnRate, float currentHeight, bool checkStep)
    {
        if (checkStep)
        {
            if (asset.AssetStep > 0)
            {
                if (((int)x) % asset.AssetStep != 0) return false;
                if (((int)y) % asset.AssetStep != 0) return false;
            }

            if (spawnRate < asset.AssetSpawnThreshholdMin || spawnRate > asset.AssetSpawnThreshholdMax)
                return false;

            if (UnityEngine.Random.Range(0f, 1f) > asset.RandomSpawnChance * 0.1f)
                return false;
        }

        if (asset.MinHeight > currentHeight || asset.MaxHeight < currentHeight)
            return false;

        return true;

    }
}

[Serializable]
public struct TerrainType
{
    [SerializeField] private string name;
    [SerializeField, Range(0f, 1f)] private float startHeight;
    [SerializeField] private Color colour;
    [SerializeField, Range(0f, 20f)] private int blend;

    public string Name => name;
    public float StartHeight => startHeight;
    public Color Colour => colour;
    public int Blend => blend;
}

[Serializable]
public struct TerrainAsset
{
    [Header("Prefab")]
    [SerializeField] private string assetName;
    [SerializeField] private GameObject assetPrefab;

    [Header("Randomisation")]
    [SerializeField, Range(0.5f, 2f)] private float minScale;
    [SerializeField, Range(0.5f, 2f)] private float maxScale;

    [SerializeField, Range(-360f, 360f)] private float minXrotation;
    [SerializeField, Range(-360f, 360f)] private float maxXrotation;
    [SerializeField, Range(-360f, 360f)] private float minYrotation;
    [SerializeField, Range(-360f, 360f)] private float maxYrotation;
    [SerializeField, Range(-360f, 360f)] private float minZrotation;
    [SerializeField, Range(-360f, 360f)] private float maxZrotation;

    [Header("Spawn Requirements")]
    [SerializeField] private float assetSpawnThreshholdMin;
    [SerializeField] private float assetSpawnThreshholdMax;
    [SerializeField, Range(0f, 1f)] private float minHeight;
    [SerializeField, Range(0f, 1f)] private float maxHeight;

    [Header("Spawn Pattern")]
    [SerializeField] private int assetStep;
    [SerializeField, Range(0f, 1f)] private float randomSpawnChance;

    [Header("Cluster Settings")]
    [SerializeField] private int clusterMinCount;
    [SerializeField] private int clusterMaxCount;
    [SerializeField] private float clusterSpread;

    [Header("Overlap Avoidance")]
    [SerializeField] private float overlapAvoidanceRadius;
    [SerializeField] private bool overlapAvoid;

    [Header("Cluster Runtime")]
    [SerializeField, Min(0.1f)] private float clusterCheckInterval;
    [SerializeField] private ClusterRateSettings clusterSpawnSettings;
    [SerializeField] private ClusterRateSettings clusterDespawnSettings;

    public string AssetName => assetName;
    public GameObject AssetPrefab => assetPrefab;

    public float MinScale => minScale;
    public float MaxScale => maxScale;

    public float MinXrotation => minXrotation;
    public float MaxXrotation => maxXrotation;
    public float MinYrotation => minYrotation;
    public float MaxYrotation => maxYrotation;
    public float MinZrotation => minZrotation;
    public float MaxZrotation => maxZrotation;

    public float AssetSpawnThreshholdMin => assetSpawnThreshholdMin;
    public float AssetSpawnThreshholdMax => assetSpawnThreshholdMax;
    public float MinHeight => minHeight;
    public float MaxHeight => maxHeight;

    public int AssetStep => assetStep;
    public float RandomSpawnChance => randomSpawnChance;

    public int ClusterMinCount => clusterMinCount;
    public int ClusterMaxCount => clusterMaxCount;
    public float ClusterSpread => clusterSpread;

    public float OverlapAvoidanceRadius => overlapAvoidanceRadius;
    public bool OverlapAvoid => overlapAvoid;

    public float ClusterCheckInterval => clusterCheckInterval;
    public ClusterRateSettings ClusterSpawnSettings => clusterSpawnSettings;
    public ClusterRateSettings ClusterDespawnSettings => clusterDespawnSettings;
}

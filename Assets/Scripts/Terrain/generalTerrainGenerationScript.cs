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

        terrainData.SetHeights(0, 0, heightMap);

        CheckHeightRange();

        ApplyTerrainTypesToShader();

        if (spawnAssets)
        {
            float[,] spawnMap = GenerateNoiseMap(heightmapHeight, heightmapWidth, assetNoiseScale, octaves, persistance, lacunarity, spawnSeed);
            SpawnTerrainAssets(spawnMap, heightMap, assetPrefabs, spawnedAssets);
        }

        if (spawnNpcs)
        {
            float[,] npcMap = GenerateNoiseMap(heightmapHeight, heightmapWidth, assetNoiseScale, octaves, persistance, lacunarity, spawnSeed * 1000);
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
            colours[i] = regions[i].colour;
            heights[i] = Mathf.Clamp01(regions[i].startHeight);
            blends[i] = Mathf.Clamp01(regions[i].blend * 0.01f);
        }

        mat.SetInt("regionsCount", regionCount);
        mat.SetColorArray("baseColours", colours);
        mat.SetFloatArray("baseHeights", heights);
        mat.SetFloatArray("baseBlends", blends);
    }

    // Method to randomly spawn assets across the terrain
    private void SpawnTerrainAssets(float[,] spawnMap, float[,] heightMap, TerrainAsset[] assets, System.Collections.Generic.List<GameObject> spawnedArray)
    {
        foreach (var asset in spawnedArray)
        {
            if (asset != null)
            {
                Destroy(asset);
            }
        }
        spawnedArray.Clear();

        if (assets == null || assets.Length == 0)
        {
            Debug.LogWarning("No asset prefabs assigned for terrain asset spawning.");
            return;
        }

        int height = spawnMap.GetLength(0);
        int width = spawnMap.GetLength(1);

        GameObject[] parentPrefabs = new GameObject[assets.Length];
        for (int i = 0; i < assets.Length; i++)
        {
            GameObject parentObject = new(assets[i].assetName + " Assets");
            parentObject.transform.parent = terrain.transform;
            parentObject.transform.localPosition = Vector3.zero;
            parentPrefabs[i] = parentObject;
        }

        Vector3 terrainPos = terrain.transform.position;
        Vector3 terrainSize = terrainData.size;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float spawnRate = spawnMap[y, x]; // 0..1
                float heightFraction = 0f;
                if (maxMultiplier > 0f)
                {
                    heightFraction = Mathf.Clamp01(heightMap[y, x] / maxMultiplier);
                }

                float currentHeight = heightFraction;

                for (int i = 0; i < assets.Length; i++)
                {
                    if (!SpawnAssetRequirements(assets[i], x, y, spawnRate, currentHeight, true))
                    {
                        continue;
                    }

                    // Normalized coordinates across the terrain (0..1)
                    float xNorm = (float)x / (width - 1);
                    float zNorm = (float)y / (height - 1);

                    // Convert to world position
                    float worldX = terrainPos.x + xNorm * terrainSize.x;
                    float worldZ = terrainPos.z + zNorm * terrainSize.z;

                    int minCount = Mathf.Max(1, assets[i].clusterMinCount);
                    int maxCount = Mathf.Max(minCount, assets[i].clusterMaxCount);
                    int clusterCount = UnityEngine.Random.Range(minCount, maxCount + 1);

                    float assetDistance = assets[i].clusterSpread * UnityEngine.Random.Range(0.75f, 1.3f);

                    for (int c = 0; c < clusterCount; c++)
                    {
                        int mapX = Mathf.RoundToInt((worldX - terrainPos.x) / terrainSize.x * (width - 1));
                        int mapY = Mathf.RoundToInt((worldZ - terrainPos.z) / terrainSize.z * (height - 1));

                        mapX = Mathf.Clamp(mapX, 0, width - 1);
                        mapY = Mathf.Clamp(mapY, 0, height - 1);
                        
                        heightFraction = 0f;
                        if (maxMultiplier > 0f)
                        {
                            heightFraction = Mathf.Clamp01(heightMap[mapY, mapX] / maxMultiplier);
                        }
                        currentHeight = heightFraction;

                        if (SpawnAssetRequirements(assets[i], worldX, worldZ, spawnMap[mapY, mapX], currentHeight, false))
                        {
                            float worldY = terrain.SampleHeight(new Vector3(worldX, 0f, worldZ)) + terrainPos.y;
                            Vector3 spawnPos = new(worldX, worldY, worldZ);

                            GameObject prefab = assets[i].assetPrefab;
                            Quaternion rot = Quaternion.Euler(UnityEngine.Random.Range(assets[i].minXrotation, assets[i].maxXrotation), UnityEngine.Random.Range(assets[i].minYrotation, assets[i].maxYrotation), UnityEngine.Random.Range(assets[i].minZrotation, assets[i].minZrotation));

                            GameObject instance = Instantiate(prefab, spawnPos, rot, transform);
                            float scaleMultiplier = UnityEngine.Random.Range(assets[i].minScale, assets[i].maxScale);
                            instance.transform.localScale *= scaleMultiplier;
                            instance.transform.parent = parentPrefabs[i].transform;
                            spawnedArray.Add(instance);
                        }
                        if (c < clusterCount - 1)
                        {
                            float angle = UnityEngine.Random.Range(0f, 360f);
                            float posx = Mathf.Cos(angle) * assetDistance;
                            float posz = Mathf.Sin(angle) * assetDistance;

                            float nextX = worldX + posx;
                            float nextZ = worldZ + posz;

                            worldX = Mathf.Clamp(nextX, terrainPos.x, terrainPos.x + terrainSize.x);
                            worldZ = Mathf.Clamp(nextZ, terrainPos.z, terrainPos.z + terrainSize.z);
                        }
                    }
                    break;
                }
            }
        }

        for (int i=0; i < parentPrefabs.Length; i++)
        {
            if (parentPrefabs[i] != null && parentPrefabs[i].transform.childCount > 0)
            {
                GameObject parent = parentPrefabs[i];
                GameObject childObject = parent.transform.GetChild(0).gameObject;
                if (childObject.GetComponent<TerrainAssetScript>() != null) 
                {
                    TerrainAssetManagement assetManager;
                    assetManager = parent.AddComponent<TerrainAssetManagement>();
                    Transform transformParent = parent.transform;
                    assetManager.SetCount(parent.transform.childCount);
                    foreach (Transform child in parent.transform)
                    {
                        TerrainAssetScript childScript = child.gameObject.GetComponent<TerrainAssetScript>();
                        assetManager.AddAsset(childScript);
                    }
                    StartCoroutine(assetManager.ParentCoroutine());
                }
            }
        }
    }

    protected bool SpawnAssetRequirements(TerrainAsset asset, float x, float y, float spawnRate, float currentHeight, bool checkStep)
    {
        if (checkStep)
        {
            if (x % asset.assetStep != 0)
                return false;
            if (y % asset.assetStep != 0)
                return false;
            if (spawnRate < asset.assetSpawnThreshholdMin || spawnRate > asset.assetSpawnThreshholdMax)
                return false;
            if (UnityEngine.Random.Range(0f, 1f) > asset.randomSpawnChance * 0.1)
                return false;
        }
        if (asset.minHeight > currentHeight || asset.maxHeight < currentHeight)
        {
            return false;
        }
        return true;
    }
}

[Serializable]
public struct TerrainType
{
    public string name;
    [Range(0f,1f)]
    public float startHeight;
    public Color colour;
    [Range(0f,20f)]
    public int blend;
}

[Serializable]
public struct TerrainAsset
{
    public string assetName;
    public GameObject assetPrefab;
    [Range (0.5f, 2f)]
    public float minScale;
    [Range (0.5f, 2f)]
    public float maxScale;
    [Range (-360f, 360f)]
    public float minXrotation;
    [Range (-360f, 360f)]
    public float maxXrotation;
    [Range (-360f, 360f)]
    public float minYrotation;
    [Range (-360f, 360f)]
    public float maxYrotation;
    [Range (-360f, 360f)]
    public float minZrotation;
    [Range (-360f, 360f)]
    public float maxZrotation;
    public float assetSpawnThreshholdMin;
    public float assetSpawnThreshholdMax;
    public float minHeight;
    public float maxHeight;
    public int assetStep;
    public float randomSpawnChance;
    public int clusterMinCount;
    public int clusterMaxCount;
    public float clusterSpread;
}
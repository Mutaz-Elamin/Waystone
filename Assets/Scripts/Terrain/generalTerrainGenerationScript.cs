using JetBrains.Annotations;
using System;
using System.Runtime.CompilerServices;
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
    [Header("Terrain Generation Settings")]
    [SerializeField] protected float noiseScale = 10f;
    [SerializeField] protected float heightMultiplier = 0.03f;
    [SerializeField] protected AnimationCurve meshHeightCurve;
    [SerializeField] protected int seed = 0;
    [SerializeField] protected int octaves = 4;
    [SerializeField] protected float persistance = 0.5f;
    [SerializeField] protected float lacunarity = 2f;

    // Fields used for spawning assets
    [Header("Asset Spawning")]
    [SerializeField] protected int spawnSeed = 0;
    [SerializeField] protected TerrainAsset[] assetPrefabs;
    [SerializeField] protected TerrainAsset[] npcPrefabs;
    [SerializeField] protected float assetNoiseScale = 20f;

    private readonly System.Collections.Generic.List<GameObject> spawnedAssets = new();
    private readonly System.Collections.Generic.List<GameObject> spawnedNpcs = new();

    // Fields for terrain types and colours - can be expanded for biomes
    [Header("Terrain Regions")]
    [SerializeField] protected TerrainType[] regions;

    // References to Terrain and TerrainData components data
    protected Terrain terrain;
    protected TerrainData terrainData;

    // NavMeshSurface reference for NavMesh building
    protected NavMeshSurface navSurface;

    // Initialize terrain generation on Awake
    private void Awake()
    {
        terrainData = new TerrainData();
        terrainData.heightmapResolution = heightmapResolution;
        terrainData.size = terrainSize;

        GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
        terrainObject.name = "Procedural Terrain";
        terrainObject.transform.parent = this.transform;
        terrainObject.transform.localPosition = Vector3.zero;
        terrainObject.layer = LayerMask.NameToLayer("NavMesh");

        terrain = terrainObject.GetComponent<Terrain>();

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
            float offsetX = random.Next(-100000, 100000);
            float offsetY = random.Next(-100000, 100000);
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

    // Main terrain generation method using Perlin noise
    private void GenerateTerrain()
    {
        int heightmapHeight = terrainData.heightmapResolution;
        int heightmapWidth = terrainData.heightmapResolution;

        float[,] noiseMap = GenerateNoiseMap(heightmapHeight, heightmapWidth, noiseScale, octaves, persistance, lacunarity, seed);

        float[,] spawnMap = GenerateNoiseMap(heightmapHeight, heightmapWidth, assetNoiseScale, octaves, persistance, lacunarity, spawnSeed);
        float[,] npcMap = GenerateNoiseMap(heightmapHeight, heightmapWidth, assetNoiseScale, octaves, persistance, lacunarity, spawnSeed * 1000);

        ColourTerrain(noiseMap);

        float[,] heightMap = new float[heightmapHeight, heightmapWidth];
        for (int y = 0; y < heightmapHeight; y++)
        {
            for (int x = 0; x < heightmapWidth; x++)
            {
                heightMap[y, x] = noiseMap[y, x] * heightMultiplier * meshHeightCurve.Evaluate(noiseMap[y, x]);
            }
        }

        terrainData.SetHeights(0, 0, heightMap);


        if (navSurface != null)
        {
            Debug.Log("Building NavMesh...");
            navSurface.BuildNavMesh();
        }
        else
        {
            Debug.LogError("NavMeshSurface missing on Terrain GameObject.");
        }

        SpawnTerrainAssets(spawnMap, noiseMap, assetPrefabs, spawnedAssets);
        SpawnTerrainAssets(npcMap, noiseMap, npcPrefabs, spawnedNpcs);
    }

    // Method to colour the terrain
    protected void ColourTerrain(float[,] noiseMap)
    {
        int height = noiseMap.GetLength(0);
        int width = noiseMap.GetLength(1);

        Texture2D texture = new(width, height);
        Color[] colourMap = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float currentHeight = noiseMap[y, x];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colourMap[y * width + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }

        texture.SetPixels(colourMap);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();

        TerrainLayer terrainLayer = new();

        var layers = terrainData.terrainLayers;
        terrainData.terrainLayers = new TerrainLayer[] { terrainLayer };

        terrainLayer.diffuseTexture = texture;
        terrainLayer.tileSize = new Vector2(terrainData.size.x, terrainData.size.z);
        terrainLayer.tileOffset = Vector2.zero;
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
                float currentHeight = heightMap[y, x];

                for (int i = 0; i < assets.Length; i++)
                {
                    if (x % assets[i].assetStep != 0)
                        continue;
                    if (y % assets[i].assetStep != 0)
                        continue;
                    if (spawnRate < assets[i].assetSpawnThreshholdMin || spawnRate > assets[i].assetSpawnThreshholdMax)
                        continue;
                    if (assets[i].minHeight > currentHeight || assets[i].maxHeight < currentHeight)
                        continue;
                    if (UnityEngine.Random.Range(0f, 1f) > assets[i].randomSpawnChance * 0.1)
                        continue;

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
                        float worldY = terrain.SampleHeight(new Vector3(worldX, 0f, worldZ)) + terrainPos.y;
                        Vector3 spawnPos = new(worldX, worldY, worldZ);

                        GameObject prefab = assets[i].assetPrefab;
                        Quaternion rot = Quaternion.Euler(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(-5f, 5f));

                        GameObject instance = Instantiate(prefab, spawnPos, rot, transform);
                        instance.transform.parent = parentPrefabs[i].transform;
                        spawnedArray.Add(instance);

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
    }
}

[Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
}

[Serializable]
public struct TerrainAsset
{
    public string assetName;
    public GameObject assetPrefab;
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
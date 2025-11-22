using JetBrains.Annotations;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Terrain))]
public class RandomTerrain : MonoBehaviour
{
    // Fields used to control terrain generation - for final version these shouldnt be serialized but set in code per biome
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
    [SerializeField] protected float assetNoiseScale = 20f;

    private readonly System.Collections.Generic.List<GameObject> spawnedAssets = new();

    // Fields for terrain types and colours - can be expanded for biomes
    [Header("Terrain Regions")]
    [SerializeField] protected TerrainType[] regions;

    // References to Terrain and TerrainData components data
    protected Terrain terrain;
    protected TerrainData terrainData;

    // Initialize terrain generation on Awake
    private void Awake()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;

        spawnSeed = seed * 100;

        // Call terrain generation method
        GenerateTerrain(this.terrain, this.terrainData, this.noiseScale, this.heightMultiplier, this.seed);
    }

    // Method to regenerate terrain on key press for testing purposes - can be removed in final version
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Regenerate terrain with a new random seed for testing random seed quickly - commented out during most use
            seed = UnityEngine.Random.Range(0, 1000);
            GenerateTerrain(this.terrain, this.terrainData, this.noiseScale, this.heightMultiplier, this.seed);
        }
        //GenerateTerrain(this.terrain, this.terrainData, this.noiseScale, this.heightMultiplier, this.seed);
    }

    // Optional: Method to set seeds externally
    public void SetSeed(int newSeed)
    {
        seed = newSeed;
        spawnSeed = newSeed * 100;
        GenerateTerrain(this.terrain, this.terrainData, this.noiseScale, this.heightMultiplier, this.seed);
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
    private void GenerateTerrain(Terrain passedTerrain, TerrainData passedTerrainData, float passedNoiseScale, float passedHeightMultiplier, int passedSeed)
    {
        int heightmapHeight = passedTerrainData.heightmapResolution;
        int heightmapWidth = passedTerrainData.heightmapResolution;

        float[,] noiseMap = GenerateNoiseMap(heightmapHeight, heightmapWidth, passedNoiseScale, octaves, persistance, lacunarity, passedSeed);

        float[,] spawnMap = GenerateNoiseMap(heightmapHeight, heightmapWidth, assetNoiseScale, octaves, persistance, lacunarity, spawnSeed);

        ColourTerrain(noiseMap);

        float[,] heightMap = new float[heightmapHeight, heightmapWidth];
        for (int y = 0; y < heightmapHeight; y++)
        {
            for (int x = 0; x < heightmapWidth; x++)
            {
                heightMap[y, x] = noiseMap[y, x] * passedHeightMultiplier * meshHeightCurve.Evaluate(noiseMap[y, x]);
            }
        }

        passedTerrainData.SetHeights(0, 0, heightMap);

        GenerateTerrainAssets(spawnMap, noiseMap);
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
    private void GenerateTerrainAssets(float[,] spawnMap, float[,] heightMap)
    {
        foreach (var asset in spawnedAssets)
        {
            if (asset != null)
            {
                Destroy(asset);
            }
        }
        spawnedAssets.Clear();

        if (assetPrefabs == null || assetPrefabs.Length == 0)
        {
            Debug.LogWarning("No asset prefabs assigned for terrain asset spawning.");
            return;
        }

        int height = spawnMap.GetLength(0);
        int width = spawnMap.GetLength(1);

        Vector3 terrainPos = terrain.transform.position;
        Vector3 terrainSize = terrainData.size;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float spawnRate = spawnMap[y, x]; // 0..1
                float currentHeight = heightMap[y, x];

                for (int i = 0; i < assetPrefabs.Length; i++)
                {
                    if (x % assetPrefabs[i].assetStep != 0)
                        continue;
                    if (spawnRate < assetPrefabs[i].assetSpawnThreshhold)
                        continue;
                    if (assetPrefabs[i].minHeight > currentHeight || assetPrefabs[i].maxHeight < currentHeight)
                        continue;

                    // Normalized coordinates across the terrain (0..1)
                    float xNorm = (float)x / (width - 1);
                    float zNorm = (float)y / (height - 1);

                    // Convert to world position
                    float worldX = terrainPos.x + xNorm * terrainSize.x;
                    float worldZ = terrainPos.z + zNorm * terrainSize.z;

                    // Sample terrain height at this position
                    float worldY = terrain.SampleHeight(new Vector3(worldX, 0f, worldZ)) + terrainPos.y;

                    Vector3 spawnPos = new(worldX, worldY, worldZ);

                    // With the following lines:
                    GameObject prefab = assetPrefabs[i].assetPrefab;
                    Quaternion rot = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

                    GameObject instance = Instantiate(prefab, spawnPos, rot, transform);
                    spawnedAssets.Add(instance);
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
    public float assetSpawnThreshhold;
    public float minHeight;
    public float maxHeight;
    public int assetStep;
}
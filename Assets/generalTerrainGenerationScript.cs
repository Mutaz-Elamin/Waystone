using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class RandomTerrain : MonoBehaviour
{
    // Fields used to control terrain generation - for final version these shouldnt be serialized but set in code per biome
    [SerializeField] protected float noiseScale = 10f;
    [SerializeField] protected float heightMultiplier = 0.03f;
    [SerializeField] protected int seed = 0;
    [SerializeField] protected int octaves = 4;
    [SerializeField] protected float persistance = 0.5f;
    [SerializeField] protected float lacunarity = 2f;

    // References to Terrain and TerrainData components data
    protected Terrain terrain;
    protected TerrainData terrainData;

    // Initialize terrain generation on Awake
    private void Awake()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;

        // Call terrain generation method
        GenerateTerrain(this.terrain, this.terrainData, this.noiseScale, this.heightMultiplier, this.seed);
    }

    // Method to regenerate terrain on key press for testing purposes - can be removed in final version
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Regenerate terrain with a new random seed for testing random seed quickly - commented out during most use
            //seed = Random.Range(0, 1000);
            GenerateTerrain(this.terrain, this.terrainData, this.noiseScale, this.heightMultiplier, this.seed);
        }
        GenerateTerrain(this.terrain, this.terrainData, this.noiseScale, this.heightMultiplier, this.seed);
    }

    // Main terrain generation method using Perlin noise
    private void GenerateTerrain(Terrain passedTerrain, TerrainData passedTerrainData, float passedNoiseScale, float passedHeightMultiplier, int passedSeed)
    {
        int heightmapWidth = passedTerrainData.heightmapResolution;
        int heightmapHeight = passedTerrainData.heightmapResolution;

        float[,] noiseMap = GenerateNoiseMap(heightmapWidth, heightmapHeight, passedNoiseScale, octaves, persistance, lacunarity, passedSeed);

        ColourTerrain(noiseMap);

        for (int y = 0; y < heightmapHeight; y++)
        {
            for (int x = 0; x < heightmapWidth; x++)
            {
                noiseMap[y, x] *= passedHeightMultiplier;
            }
        }

        passedTerrainData.SetHeights(0, 0, noiseMap);
    }

    // Optional: Method to set seeds externally
    public void SetSeed(int newSeed)
    {
        seed = newSeed;
        GenerateTerrain(this.terrain, this.terrainData, this.noiseScale, this.heightMultiplier, this.seed);
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

        //Random.InitState(mapSeed);
        //Vector2 randomOffset = new(Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));

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
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[y, x]);
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
}

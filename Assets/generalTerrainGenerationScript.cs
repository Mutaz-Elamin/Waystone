using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class RandomTerrain : MonoBehaviour
{
    // Fields used to control terrain generation - for final version these shouldnt be serialized but set in code per biome
    [SerializeField] protected float noiseScale = 10f;
    [SerializeField] protected float heightMultiplier = 0.03f;
    [SerializeField] protected int seed = 0;

    // References to Terrain and TerrainData components data
    protected Terrain terrain;
    protected TerrainData terrainData;
    protected Renderer textureRenderer;

    // Initialize terrain generation on Awake
    private void Awake()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        textureRenderer = GetComponent<Renderer>();

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
    }

    // Main terrain generation method using Perlin noise
    private void GenerateTerrain(Terrain passedTerrain, TerrainData passedTerrainData, float passedNoiseScale, float passedHeightMultiplier, int passedSeed)
    {
        int heightmapWidth = passedTerrainData.heightmapResolution;
        int heightmapHeight = passedTerrainData.heightmapResolution;

        float[,] noiseMap = GenerateNoiseMap(heightmapWidth, heightmapHeight, passedNoiseScale, passedSeed);

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
    protected float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float mapScale, int mapSeed)
    {
        if (mapScale <= 0)
        {
            mapScale = 0.0001f;
        }

        float[,] noiseMap = new float[mapHeight, mapWidth];
        Random.InitState(mapSeed);
        Vector2 randomOffset = new(Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float xCoord = (float)x / mapWidth * mapScale+ randomOffset.x;
                float yCoord = (float)y / mapHeight * mapScale + randomOffset.y;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                noiseMap[y, x] = sample;
            }
        }
        return noiseMap;
    }
}

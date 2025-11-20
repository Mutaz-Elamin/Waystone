using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class RandomTerrain : MonoBehaviour
{
    // Fields used to control terrain generation - for final version these shouldnt be serialized but set in code per biome
    [SerializeField] private float noiseScale = 10f;
    [SerializeField] private float heightMultiplier = 0.03f;
    [SerializeField] private int seed = 0;

    // References to Terrain and TerrainData components data
    private Terrain terrain;
    private TerrainData terrainData;

    // Initialize terrain generation on Awake
    private void Awake()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;

        // Call terrain generation method
        GenerateTerrain();
    }

    // Method to regenerate terrain on key press for testing purposes - can be removed in final version
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Regenerate terrain with a new random seed for testing random seed quickly - commented out during most use
            //seed = Random.Range(0, 1000);
            GenerateTerrain();
        }
    }

    // Main terrain generation method using Perlin noise
    private void GenerateTerrain()
    {
        int heightmapWidth = terrainData.heightmapResolution;
        int heightmapHeight = terrainData.heightmapResolution;

        float[,] heights = new float[heightmapHeight, heightmapWidth];

        Random.InitState(seed);
        Vector2 randomOffset = new(Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));

        for (int y = 0; y < heightmapHeight; y++)
        {
            for (int x = 0; x < heightmapWidth; x++)
            {
                float xCoord = (float)x / heightmapWidth * noiseScale + randomOffset.x;
                float yCoord = (float)y / heightmapHeight * noiseScale + randomOffset.y;

                float sample = Mathf.PerlinNoise(xCoord, yCoord);

                heights[y, x] = sample * heightMultiplier;
            }
        }
        terrainData.SetHeights(0, 0, heights);
    }

    // Optional: Method to set seeds externally
    public void SetSeed(int newSeed)
    {
        seed = newSeed;
        GenerateTerrain();
    }
}

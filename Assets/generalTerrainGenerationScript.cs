using UnityEngine;

public class RandomTerrain : MonoBehaviour
{
    [SerializeField] private float noiseScale = 10f;
    [SerializeField] private float heightMultiplier = 0.03f;

    private Terrain terrain;
    private TerrainData terrainData;

    private void Awake()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;

        GenerateTerrain();
    }

    public void GenerateTerrain()
    {
        int heightmapWidth = terrainData.heightmapResolution;
        int heightmapHeight = terrainData.heightmapResolution;
        float[,] heights = new float[heightmapHeight, heightmapWidth];

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
}

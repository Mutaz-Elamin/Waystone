using System;
using UnityEngine;

public class GenericLandmark : MonoBehaviour
{
    [Header("Landmark Settings")]
    [Header("Prefab")]
    [SerializeField] protected GameObject landmarkPrefab;

    [Header("Height Range (0-1)")]
    [SerializeField] protected float minHeight = 0f;
    [SerializeField] protected float maxHeight = 0f;

    [Header("Distance from Features")]
    [SerializeField] protected float borderDistance = 50f;
    [SerializeField] protected float centreDistance = 50f;

    [Header("Location Attempts")]
    [SerializeField] protected int borderSamples = 128;
    [SerializeField] protected int maxAttempts = 200;

    [Header("Related Scripts")]
    [SerializeField] protected RandomTerrain mapGenerator;
    [SerializeField] protected Terrain terrain;
    [SerializeField] protected WorldBorder worldBorder;

    protected Vector3 landmarkLocation;
    protected GameObject landmarkInstance;
    protected float worldMinHeight;
    protected float worldMaxHeight;

    public virtual void GenerateLocation(int seed)
    {
        if (landmarkInstance == null)
        {
            if (landmarkPrefab == null)
            {
                Debug.LogWarning("No landmark prefab assigned!");
                return;
            }
        }
        else
        {
            Destroy(landmarkInstance);
            landmarkInstance = null;
        }

        landmarkLocation = GetValidWorldPosition(seed);
        landmarkInstance = Instantiate(landmarkPrefab, landmarkLocation, Quaternion.identity);
        landmarkInstance.transform.parent = this.transform;
    }

    protected Vector3 GetValidWorldPosition(int seed)
    {
        if (terrain == null) terrain = Terrain.activeTerrain;
        if (worldBorder == null) worldBorder = UnityEngine.Object.FindFirstObjectByType<WorldBorder>();

        Vector3 tPos = terrain.transform.position;
        Vector3 tSize = terrain.terrainData.size;

        Vector3 centre = new Vector3(tPos.x + tSize.x * 0.5f, 0f, tPos.z + tSize.z * 0.5f);

        float centreDistSqr = centreDistance * centreDistance;
        float borderDistSqr = borderDistance * borderDistance;

        System.Random random = new System.Random(seed);

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float x = (float)random.NextDouble() * tSize.x + tPos.x;
            float z = (float)random.NextDouble() * tSize.z + tPos.z;

            float y = terrain.SampleHeight(new Vector3(x, 0f, z)) + tPos.y;
            Vector3 candidate = new Vector3(x, y, z);

            if (worldBorder != null && !worldBorder.IsInsideWorld(candidate))
                continue;

            Vector3 flatCandidate = new Vector3(candidate.x, 0f, candidate.z);

            if ((flatCandidate - centre).sqrMagnitude < centreDistSqr)
                continue;

            if (worldBorder != null && borderDistance > 0f)
            {
                if (IsTooCloseToBorder(flatCandidate, borderDistSqr))
                    continue;
            }

            float yLerped = Mathf.InverseLerp(worldMinHeight, worldMaxHeight, y);
            if (yLerped < minHeight || yLerped > maxHeight)
                continue;

            return candidate;
        }

        return new Vector3(centre.x, terrain.SampleHeight(centre) + tPos.y, centre.z);
    }

    private bool IsTooCloseToBorder(Vector3 flatCandidate, float borderDistSqr)
    {
        int samples = Mathf.Max(8, borderSamples);

        for (int i = 0; i < samples; i++)
        {
            float theta01 = i / (float)samples;

            if (!worldBorder.TryGetBorderPointWorld(theta01, 0f, out Vector3 borderPoint))
                continue;

            Vector3 flatBorder = new Vector3(borderPoint.x, 0f, borderPoint.z);

            if ((flatCandidate - flatBorder).sqrMagnitude < borderDistSqr)
                return true;
        }

        return false;
    }

    public void setWorldHeights(float minWorldHeight, float maxWorldHeight)
    {
        worldMinHeight = minWorldHeight;
        worldMaxHeight = maxWorldHeight;
    }
}
